using LinqToDB;
using LinqToDB.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.ReportsModule.Application.DTOs;
using TaskControl.ReportsModule.DataAccess.Interface;

namespace TaskControl.ReportsModule.DataAccess.Repositories
{
    public class OrderAnalyticsRepository : IOrderAnalyticsRepository
    {
        private readonly IReportDataConnection _db;

        public OrderAnalyticsRepository(IReportDataConnection db)
        {
            _db = db;
        }

        // 1. Получение глубокой аналитики по сотрудникам (Группировка с задачами)
        public async Task<List<EmployeeFullReportDto>> GetEmployeeFullReportsAsync(AnalyticsFilterDto filter)
        {
            var sql = @"
        WITH AllAssignments AS (
            SELECT task_id, assigned_to_user_id AS user_id, branch_id, assigned_at, started_at, completed_at, complexity, id as source_id, 'Assembly' as type 
            FROM order_assembly_assignments WHERE status = 3
            UNION ALL
            SELECT oha.task_id, oha.assigned_to_user_id AS user_id, bt.branch_id, oha.assigned_at, oha.started_at, oha.completed_at, oha.complexity, oha.id as source_id, 'Handover' as type 
            FROM order_handover_assignments oha JOIN base_tasks bt ON oha.task_id = bt.task_id WHERE oha.status = 3
            UNION ALL
            SELECT task_id, assigned_to_user_id AS user_id, branch_id, assigned_at, started_at, completed_at, complexity, id as source_id, 'Return' as type 
            FROM return_assignments WHERE status = 3
            UNION ALL
            SELECT task_id, assigned_to_user_id AS user_id, branch_id, assigned_at, started_at, completed_at, 1.0 as complexity, id as source_id, 'Inventory' as type 
            FROM inventory_assignments WHERE status = 3
        )
        SELECT 
            e.employees_id AS ""EmployeeId"", 
            e.surname || ' ' || e.name AS ""FullName"", 
            b.branch_name AS ""BranchName"",
            bt.task_id AS ""TaskId"",
            bt.type AS ""TaskTypeRaw"",
            bt.created_at AS ""CreatedAt"",
            aa.assigned_at AS ""AssignedAt"",
            aa.started_at AS ""StartedAt"",
            aa.completed_at AS ""CompletedAt"",
            CAST(aa.complexity AS INT) AS ""Complexity"",
            CAST(COALESCE(EXTRACT(EPOCH FROM (aa.started_at - aa.assigned_at)), 0) AS INT) AS ""ReactionTimeSeconds"",
            CAST(COALESCE(EXTRACT(EPOCH FROM (aa.completed_at - aa.started_at)), 0) AS INT) AS ""ExecutionTimeSeconds"",
            CAST(COALESCE(SUM(CASE WHEN aa.type = 'Assembly' THEN oal.quantity * i.weight ELSE 0 END), 0) AS FLOAT) AS ""WeightGrams"",
            CAST(COALESCE(SUM(CASE WHEN aa.type = 'Assembly' THEN oal.quantity * (i.length * i.width * i.height) ELSE 0 END), 0) AS FLOAT) AS ""VolumeCubicMm""
        FROM AllAssignments aa
        JOIN employees e ON aa.user_id = e.employees_id
        JOIN branches b ON aa.branch_id = b.branch_id
        JOIN base_tasks bt ON aa.task_id = bt.task_id
        LEFT JOIN order_assembly_lines oal ON aa.type = 'Assembly' AND aa.source_id = oal.order_assembly_assignment_id
        LEFT JOIN item_positions ip ON oal.item_position_id = ip.id
        LEFT JOIN items i ON ip.item_id = i.item_id
        WHERE aa.completed_at >= @StartDate 
          AND aa.completed_at <= @EndDate
          AND (@BranchId IS NULL OR aa.branch_id = @BranchId)
          AND (@EmployeeId IS NULL OR aa.user_id = @EmployeeId)
        GROUP BY e.employees_id, e.surname, e.name, b.branch_name, bt.task_id, bt.type, bt.created_at, aa.assigned_at, aa.started_at, aa.completed_at, aa.complexity";

            // Получаем плоский список всех задач со всеми данными
            var flatData = await ((DataConnection)_db).QueryToListAsync<EmployeeTaskFlatDto>(sql,
                new DataParameter("StartDate", filter.StartDate, DataType.DateTime),
                new DataParameter("EndDate", filter.EndDate, DataType.DateTime),
                new DataParameter("BranchId", filter.BranchId, DataType.Int32),
                new DataParameter("EmployeeId", filter.EmployeeId, DataType.Int32));

            // Группируем в C# для создания иерархии Карточка Сотрудника -> Список его задач
            return flatData.GroupBy(x => new { x.EmployeeId, x.FullName, x.BranchName })
                .Select(g => new EmployeeFullReportDto
                {
                    EmployeeId = g.Key.EmployeeId,
                    FullName = g.Key.FullName,
                    BranchName = g.Key.BranchName,
                    Tasks = g.Select(t => new EmployeeTaskExtendedDto
                    {
                        TaskId = t.TaskId,
                        TaskTypeRaw = t.TaskTypeRaw,
                        CreatedAt = t.CreatedAt,
                        AssignedAt = t.AssignedAt,
                        StartedAt = t.StartedAt,
                        CompletedAt = t.CompletedAt,
                        ReactionTimeSeconds = t.ReactionTimeSeconds,
                        ExecutionTimeSeconds = t.ExecutionTimeSeconds,
                        Complexity = t.Complexity,
                        WeightGrams = t.WeightGrams,
                        VolumeCubicMm = t.VolumeCubicMm
                    }).OrderByDescending(t => t.CompletedAt).ToList()
                }).ToList();
        }

        // 2. Управленческий Дашборд по Заказам
        public async Task<OrderDashboardReportDto> GetOrderDashboardAsync(AnalyticsFilterDto filter)
        {
            var sql = @"
        SELECT 
            o.order_id AS ""OrderId"", 
            COALESCE(c.first_name || ' ' || c.last_name, 'Неизвестный клиент') AS ""CustomerName"", 
            o.delivery_type AS ""DeliveryTypeRaw"", 
            o.payment_type AS ""PaymentType"",
            o.total_price AS ""TotalPrice"",
            o.status AS ""StatusRaw"",
            o.created_at AS ""CreatedAt"", 
            o.delivery_date AS ""ExpectedDeliveryDate"",
            
            -- Подзапрос для поиска фактического времени выдачи в зависимости от типа логистики
            CAST((CASE 
                WHEN o.delivery_type = 'Express' THEN 
                    (SELECT MAX(oaa.completed_at) FROM order_assembly_assignments oaa WHERE oaa.order_id = o.order_id AND oaa.status = 3)
                ELSE 
                    (SELECT MAX(oha.completed_at) FROM order_handover_assignments oha WHERE oha.order_id = o.order_id AND oha.status = 3)
            END) AS TIMESTAMP) AS ""ActualHandoverAt"",

            STRING_AGG(i.name || ' (' || op.quantity || ' шт)', ', ') AS ""ItemsList"",
            CAST(COALESCE(SUM(op.quantity * i.weight), 0) AS FLOAT) AS ""TotalWeightGrams"",
            CAST(COALESCE(SUM(op.quantity * (i.length * i.width * i.height)), 0) AS FLOAT) AS ""TotalVolumeCubicMm""
        FROM orders o
        LEFT JOIN customers c ON o.customer_id = c.customer_id
        LEFT JOIN order_positions op ON o.order_id = op.order_id
        LEFT JOIN items i ON op.item_id = i.item_id
        WHERE o.created_at >= @StartDate 
          AND o.created_at <= @EndDate
          AND (@BranchId IS NULL OR o.branch_id = @BranchId)
        GROUP BY o.order_id, c.first_name, c.last_name, o.delivery_type, o.payment_type, o.total_price, o.status, o.created_at, o.delivery_date
        ORDER BY o.created_at DESC";

            var orders = await ((DataConnection)_db).QueryToListAsync<OrderRegistryFlatDto>(sql,
                new DataParameter("StartDate", filter.StartDate, DataType.DateTime),
                new DataParameter("EndDate", filter.EndDate, DataType.DateTime),
                new DataParameter("BranchId", filter.BranchId, DataType.Int32));

            var completedOrders = orders.Where(o => o.StatusRaw == "Completed").ToList();

            var dashboard = new OrderDashboardReportDto
            {
                Orders = orders,
                TotalOrders = orders.Count,
                TotalRevenue = completedOrders.Sum(o => o.TotalPrice),
                HeaviestOrder = orders.OrderByDescending(o => o.TotalWeightGrams).FirstOrDefault(),
                MostVoluminousOrder = orders.OrderByDescending(o => o.TotalVolumeCubicMm).FirstOrDefault(),
                FastestOrder = completedOrders.Where(o => o.LeadTimeMinutes > 0).OrderBy(o => o.LeadTimeMinutes).FirstOrDefault(),
                // Получаем топ товары из уже готового метода
                TopItems = await GetTopItemsAsync(filter)
            };

            return dashboard;
        }

        //public async Task<List<EmployeeTaskDetailDto>> GetEmployeeTasksDetailAsync(int employeeId, AnalyticsFilterDto filter)
        //{
        //    var sql = @"
        //SELECT 
        //    bt.id AS ""TaskId"", 
        //    bt.type AS ""TaskTypeRaw"", 
        //    bt.completed_at AS ""CompletedAt"", 
        //    bt.complexity AS ""Complexity"",
        //    CAST(EXTRACT(EPOCH FROM (bt.completed_at - bt.created_at)) AS INT) AS ""DurationSeconds""
        //FROM task_assignations ta
        //JOIN base_tasks bt ON ta.base_task_id = bt.id
        //WHERE ta.assigned_to_user_id = @EmployeeId
        //  AND ta.status = 3 -- Статус Completed для назначений
        //  AND bt.completed_at >= @StartDate 
        //  AND bt.completed_at <= @EndDate
        //ORDER BY bt.completed_at DESC";

        //    return await ((DataConnection)_db).QueryToListAsync<EmployeeTaskDetailDto>(sql,
        //        new DataParameter("EmployeeId", employeeId, DataType.Int32),
        //        new DataParameter("StartDate", filter.StartDate, DataType.DateTime),
        //        new DataParameter("EndDate", filter.EndDate, DataType.DateTime));
        //}

        public async Task<List<OrderLeadTimeDto>> GetOrderLeadTimesAsync(AnalyticsFilterDto filter)
        {
            var sql = @"
                SELECT 
                    o.order_id AS ""OrderId"", 
                    b.branch_name AS ""BranchName"", 
                    o.created_at AS ""CreatedAt"", 
                    o.delivery_date AS ""DeliveryDate"", 
                    o.delivery_type AS ""DeliveryTypeRaw"", 
                    o.status AS ""StatusRaw"", 
                    o.total_price AS ""TotalPrice""
                FROM orders o
                JOIN branches b ON o.branch_id = b.branch_id
                WHERE o.created_at >= @StartDate 
                  AND o.created_at <= @EndDate
                  AND (@BranchId IS NULL OR o.branch_id = @BranchId)
                ORDER BY o.created_at DESC";

            // Используем QueryToListAsync вместо QueryAsync
            return await ((DataConnection)_db).QueryToListAsync<OrderLeadTimeDto>(sql,
                new DataParameter("StartDate", filter.StartDate, DataType.DateTime),
                new DataParameter("EndDate", filter.EndDate, DataType.DateTime),
                new DataParameter("BranchId", filter.BranchId, DataType.Int32));
        }

        public async Task<List<TopItemDto>> GetTopItemsAsync(AnalyticsFilterDto filter)
        {
            var sql = @"
                SELECT 
                    i.item_id AS ""ItemId"", 
                    i.name AS ""ItemName"", 
                    CAST(SUM(op.quantity) AS INT) AS ""QuantitySold"",
                    CAST(SUM(op.quantity * i.weight) AS FLOAT) AS ""TotalWeightGrams"",
                    CAST(SUM(op.quantity * (i.length * i.width * i.height)) AS FLOAT) AS ""TotalVolumeCubicMm""
                FROM order_positions op
                JOIN orders o ON op.order_id = o.order_id
                JOIN items i ON op.item_id = i.item_id
                WHERE o.status = 'Completed'
                  AND o.created_at >= @StartDate 
                  AND o.created_at <= @EndDate
                  AND (@BranchId IS NULL OR o.branch_id = @BranchId)
                GROUP BY i.item_id, i.name
                ORDER BY ""QuantitySold"" DESC
                LIMIT 100";

            return await ((DataConnection)_db).QueryToListAsync<TopItemDto>(sql,
                new DataParameter("StartDate", filter.StartDate, DataType.DateTime),
                new DataParameter("EndDate", filter.EndDate, DataType.DateTime),
                new DataParameter("BranchId", filter.BranchId, DataType.Int32));
        }

        //public async Task<List<EmployeeKpiDto>> GetEmployeeKpiAsync(AnalyticsFilterDto filter)
        //{
        //    var sql = @"
        //        SELECT 
        //            e.employees_id AS ""EmployeeId"", 
        //            e.surname || ' ' || e.name AS ""FullName"", 
        //            b.branch_name AS ""BranchName"",
        //            COUNT(DISTINCT oaa.id) AS ""TasksCompleted"",
        //            CAST(COALESCE(SUM(EXTRACT(EPOCH FROM (oaa.completed_at - oaa.started_at))), 0) AS INT) AS ""TotalDurationSeconds"",
        //            CAST(COALESCE(SUM(oal.quantity * i.weight), 0) AS FLOAT) AS ""TotalWeightMovedGrams"",
        //            CAST(COALESCE(SUM(oal.quantity * (i.length * i.width * i.height)), 0) AS FLOAT) AS ""TotalVolumeMovedCubicMm""
        //        FROM order_assembly_assignments oaa
        //        JOIN employees e ON oaa.assigned_to_user_id = e.employees_id
        //        JOIN branches b ON oaa.branch_id = b.branch_id
        //        LEFT JOIN order_assembly_lines oal ON oaa.id = oal.order_assembly_assignment_id
        //        LEFT JOIN item_positions ip ON oal.item_position_id = ip.id
        //        LEFT JOIN items i ON ip.item_id = i.item_id
        //        WHERE oaa.status = 3
        //          AND oaa.completed_at >= @StartDate 
        //          AND oaa.completed_at <= @EndDate
        //          AND (@BranchId IS NULL OR oaa.branch_id = @BranchId)
        //          AND (@EmployeeId IS NULL OR oaa.assigned_to_user_id = @EmployeeId)
        //        GROUP BY e.employees_id, e.surname, e.name, b.branch_name
        //        ORDER BY ""TasksCompleted"" DESC";

        //    return await ((DataConnection)_db).QueryToListAsync<EmployeeKpiDto>(sql,
        //        new DataParameter("StartDate", filter.StartDate, DataType.DateTime),
        //        new DataParameter("EndDate", filter.EndDate, DataType.DateTime),
        //        new DataParameter("BranchId", filter.BranchId, DataType.Int32),
        //        new DataParameter("EmployeeId", filter.EmployeeId, DataType.Int32));
        //}

        public async Task<List<BranchSummaryDto>> GetBranchSummaryAsync(AnalyticsFilterDto filter)
        {
            var sql = @"
                SELECT 
                    b.branch_id AS ""BranchId"", 
                    b.branch_name AS ""BranchName"", 
                    COUNT(o.order_id) AS ""TotalOrdersCompleted"",
                    COALESCE(SUM(o.total_price), 0) AS ""TotalRevenue"",
                    CAST(COALESCE(AVG(EXTRACT(EPOCH FROM (o.delivery_date - o.created_at)) / 60), 0) AS INT) AS ""AverageLeadTimeMinutes""
                FROM branches b
                LEFT JOIN orders o ON b.branch_id = o.branch_id 
                    AND o.status = 'Completed'
                    AND o.created_at >= @StartDate 
                    AND o.created_at <= @EndDate
                WHERE (@BranchId IS NULL OR b.branch_id = @BranchId)
                GROUP BY b.branch_id, b.branch_name
                ORDER BY ""TotalRevenue"" DESC";

            return await ((DataConnection)_db).QueryToListAsync<BranchSummaryDto>(sql,
                new DataParameter("StartDate", filter.StartDate, DataType.DateTime),
                new DataParameter("EndDate", filter.EndDate, DataType.DateTime),
                new DataParameter("BranchId", filter.BranchId, DataType.Int32));
        }

        // ИСПРАВЛЕННЫЙ МЕТОД: Подсчет KPI с объединением всех таблиц назначений
        public async Task<List<EmployeeKpiDto>> GetEmployeeKpiAsync(AnalyticsFilterDto filter)
        {
            var sql = @"
        WITH AllAssignments AS (
            -- Сборка заказов
            SELECT task_id, assigned_to_user_id AS user_id, branch_id, started_at, completed_at, complexity, id as source_id, 'Assembly' as type 
            FROM order_assembly_assignments WHERE status = 3
            UNION ALL
            -- Выдача заказов (берем branch_id из base_tasks, так как в handover его нет)
            SELECT oha.task_id, oha.assigned_to_user_id AS user_id, bt.branch_id, oha.started_at, oha.completed_at, oha.complexity, oha.id as source_id, 'Handover' as type 
            FROM order_handover_assignments oha JOIN base_tasks bt ON oha.task_id = bt.task_id WHERE oha.status = 3
            UNION ALL
            -- Возвраты
            SELECT task_id, assigned_to_user_id AS user_id, branch_id, started_at, completed_at, complexity, id as source_id, 'Return' as type 
            FROM return_assignments WHERE status = 3
            UNION ALL
            -- Инвентаризация (здесь нет поля complexity, поэтому считаем за 1.0)
            SELECT task_id, assigned_to_user_id AS user_id, branch_id, started_at, completed_at, 1.0 as complexity, id as source_id, 'Inventory' as type 
            FROM inventory_assignments WHERE status = 3
        )
        SELECT 
            e.employees_id AS ""EmployeeId"", 
            e.surname || ' ' || e.name AS ""FullName"", 
            b.branch_name AS ""BranchName"",
            COUNT(aa.task_id) AS ""TasksCompleted"",
            CAST(COALESCE(SUM(aa.complexity), 0) AS INT) AS ""TotalComplexity"",
            CAST(COALESCE(SUM(EXTRACT(EPOCH FROM (aa.completed_at - aa.started_at))), 0) AS INT) AS ""TotalDurationSeconds"",
            -- Считаем вес и объем только для задач сборки, присоединяя товары
            CAST(COALESCE(SUM(CASE WHEN aa.type = 'Assembly' THEN oal.quantity * i.weight ELSE 0 END), 0) AS FLOAT) AS ""TotalWeightMovedGrams"",
            CAST(COALESCE(SUM(CASE WHEN aa.type = 'Assembly' THEN oal.quantity * (i.length * i.width * i.height) ELSE 0 END), 0) AS FLOAT) AS ""TotalVolumeMovedCubicMm""
        FROM AllAssignments aa
        JOIN employees e ON aa.user_id = e.employees_id
        JOIN branches b ON aa.branch_id = b.branch_id
        LEFT JOIN order_assembly_lines oal ON aa.type = 'Assembly' AND aa.source_id = oal.order_assembly_assignment_id
        LEFT JOIN item_positions ip ON oal.item_position_id = ip.id
        LEFT JOIN items i ON ip.item_id = i.item_id
        WHERE aa.completed_at >= @StartDate 
          AND aa.completed_at <= @EndDate
          AND (@BranchId IS NULL OR aa.branch_id = @BranchId)
          AND (@EmployeeId IS NULL OR aa.user_id = @EmployeeId)
        GROUP BY e.employees_id, e.surname, e.name, b.branch_name
        ORDER BY ""TotalComplexity"" DESC";

            return await ((DataConnection)_db).QueryToListAsync<EmployeeKpiDto>(sql,
                new DataParameter("StartDate", filter.StartDate, DataType.DateTime),
                new DataParameter("EndDate", filter.EndDate, DataType.DateTime),
                new DataParameter("BranchId", filter.BranchId, DataType.Int32),
                new DataParameter("EmployeeId", filter.EmployeeId, DataType.Int32));
        }

        // ИСПРАВЛЕННЫЙ МЕТОД: Детализация задач сотрудника с тем же механизмом UNION ALL
        public async Task<List<EmployeeTaskDetailDto>> GetEmployeeTasksDetailAsync(int employeeId, AnalyticsFilterDto filter)
        {
            var sql = @"
        WITH AllAssignments AS (
            SELECT task_id, assigned_to_user_id AS user_id, started_at, completed_at, complexity FROM order_assembly_assignments WHERE status = 3
            UNION ALL
            SELECT task_id, assigned_to_user_id AS user_id, started_at, completed_at, complexity FROM order_handover_assignments WHERE status = 3
            UNION ALL
            SELECT task_id, assigned_to_user_id AS user_id, started_at, completed_at, complexity FROM return_assignments WHERE status = 3
            UNION ALL
            SELECT task_id, assigned_to_user_id AS user_id, started_at, completed_at, 1.0 as complexity FROM inventory_assignments WHERE status = 3
        )
        SELECT 
            bt.task_id AS ""TaskId"", 
            bt.type AS ""TaskTypeRaw"", 
            aa.completed_at AS ""CompletedAt"", 
            CAST(aa.complexity AS INT) AS ""Complexity"",
            CAST(COALESCE(EXTRACT(EPOCH FROM (aa.completed_at - aa.started_at)), 0) AS INT) AS ""DurationSeconds""
        FROM AllAssignments aa
        JOIN base_tasks bt ON aa.task_id = bt.task_id
        WHERE aa.user_id = @EmployeeId
          AND aa.completed_at >= @StartDate 
          AND aa.completed_at <= @EndDate
        ORDER BY aa.completed_at DESC";

            return await ((DataConnection)_db).QueryToListAsync<EmployeeTaskDetailDto>(sql,
                new DataParameter("EmployeeId", employeeId, DataType.Int32),
                new DataParameter("StartDate", filter.StartDate, DataType.DateTime),
                new DataParameter("EndDate", filter.EndDate, DataType.DateTime));
        }


        // НОВЫЙ МЕТОД: Получение детализации заказов (с агрегацией товаров)
        public async Task<List<OrderDetailedDto>> GetDetailedOrdersAsync(AnalyticsFilterDto filter)
        {
            var sql = @"
        SELECT 
            o.order_id AS ""OrderId"", 
            o.created_at AS ""CreatedAt"", 
            o.status AS ""StatusRaw"",
            STRING_AGG(i.name || ' (' || op.quantity || ' шт)', ', ') AS ""ItemsList"",
            CAST(COALESCE(SUM(op.quantity * i.weight), 0) AS FLOAT) AS ""TotalWeightGrams"",
            CAST(COALESCE(SUM(op.quantity * (i.length * i.width * i.height)), 0) AS FLOAT) AS ""TotalVolumeCubicMm""
        FROM orders o
        LEFT JOIN order_positions op ON o.order_id = op.order_id
        LEFT JOIN items i ON op.item_id = i.item_id
        WHERE o.created_at >= @StartDate 
          AND o.created_at <= @EndDate
          AND (@BranchId IS NULL OR o.branch_id = @BranchId)
        GROUP BY o.order_id, o.created_at, o.status
        ORDER BY o.created_at DESC";

            return await ((DataConnection)_db).QueryToListAsync<OrderDetailedDto>(sql,
                new DataParameter("StartDate", filter.StartDate, DataType.DateTime),
                new DataParameter("EndDate", filter.EndDate, DataType.DateTime),
                new DataParameter("BranchId", filter.BranchId, DataType.Int32));
        }
    }
}