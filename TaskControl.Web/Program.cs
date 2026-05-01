
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Cors.Infrastructure;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;
using TaskControl.Core.AppSettings;
using TaskControl.Core.Infrastructure;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InformationModule.Application.DTOs;
using TaskControl.InformationModule.DataAccess.Infrastructure;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.DataAccess.Repositories;
using TaskControl.InformationModule.Services;
using TaskControl.InformationModule.Services.BackgroundServices;
using TaskControl.InformationModule.Services.Hubs;
using TaskControl.InventoryModule.DataAccess.Infrastructure;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.TaskModule.Application.Jobs;
using TaskControl.TaskModule.Application.Services.Hubs;

namespace TaskControl.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

            builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection(AppSettings.SectionName));

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();



            //builder.Services.AddScoped<IInventoryDataConnection, InventoryDataConnection>();
            builder.Services.AddServicesGroup(builder.Configuration);
            // Hangfire
            builder.Services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddHangfireServer();

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            builder.Services.AddSignalR();
            builder.Services.AddHostedService<QRGeneratorService>();
            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new Web.Infrastructure.UtcDateTimeConverter());
            });

            var app = builder.Build();
            try
            {
                Log.Information("Запуск приложения {AppName} версии {AppVersion}",
                    builder.Configuration["AppSettings:AppName"],
                    builder.Configuration["AppSettings:AppVersion"]);

                app.UseSwagger();
                app.UseSwaggerUI();

                app.UseStaticFiles();

                app.UseHttpsRedirection();

                app.UseAuthorization();

                app.UseHangfireDashboard("/hangfire");



                RecurringJob.AddOrUpdate<TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob>(
                    "order-assembly-planner",
                    job => job.ExecuteAsync(),
                    "*/5 * * * *"); // Каждые 5 минут
                RecurringJob.AddOrUpdate<AutoEndBreakJob>(
                    "auto-end-expired-breaks",
                    job => job.ExecuteAsync(),
                    Cron.Minutely);
                RecurringJob.AddOrUpdate<PriorityEscalationJob>(
                    "priority-escalation-job",
                    job => job.ExecuteAsync(),
                    "*/1 * * * *");
                app.MapHub<QRHub>("/qrhub");
                app.MapHub<TaskNotificationHub>("/hubs/task-notifications");
                app.MapControllers();
                Log.Information("Приложение настроено и готово к работе на порту {Port}",
    builder.Configuration["urls"] ?? "default");
                Console.WriteLine("Тест");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Критическая ошибка при запуске приложения");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
        /*
         -- ДЛЯ ТЕСТОВ

-- Удалить старые отметки check-out для тестовых работников
DELETE FROM check_io_employees
WHERE employee_id IN (1, 2, 3)
  AND check_type = 'out'
  AND check_timestamp > NOW() - INTERVAL '1 day';

-- Добавить работников на смену в филиале 1 (Центральный склад)
INSERT INTO check_io_employees (employee_id, branch_id, check_type, check_timestamp)
VALUES
  (1, 1, 'in', NOW() - INTERVAL '2 hours'),      -- Иванов пришёл 2 часа назад
  (2, 1, 'in', NOW() - INTERVAL '1 hour'),       -- Петрова пришла час назад
  (3, 1, 'in', NOW() - INTERVAL '30 minutes');   -- Сидоров пришёл 30 минут назад

-- НЕ добавляем 'out', чтобы они считались на смене

TRUNCATE TABLE
  inventory_discrepancies,
  inventory_statistics,
  inventory_assignment_lines,
  inventory_assignments,
  active_assigned_tasks,
  base_tasks
RESTART IDENTITY CASCADE;

        {
  "branchId": 1,
  "itemPositionIds": [1, 2, 4, 6, 8],
  "priority": 8,
  "workerCount": 2,
  "divisionStrategy": 0,
  "description": "Плановая инвентаризация зоны A",
  "deadlineDate": "2025-12-25T18:00:00.000Z"
}
         */
    }
}
