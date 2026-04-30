using LinqToDB;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Mapper;
using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Repositories
{
    public class OrderAssemblyAssignmentRepository : IOrderAssemblyAssignmentRepository
    {
        private readonly ITaskDataConnection _db;
        private readonly ILogger<OrderAssemblyAssignmentRepository> _logger;

        public async Task<List<OrderAssemblyAssignmentModel>> GetAllByTaskIdAsync(int taskId)
        {
            return await _db.GetTable<OrderAssemblyAssignmentModel>()
                .Where(a => a.TaskId == taskId)
                .ToListAsync();
        }
        public async Task<OrderAssemblyAssignmentModel> GetByTaskAndUserAsync(int taskId, int workerId)
        {
            return await _db.GetTable<OrderAssemblyAssignmentModel>()
                .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedToUserId == workerId);
        }
        public OrderAssemblyAssignmentRepository(
            ITaskDataConnection db,
            ILogger<OrderAssemblyAssignmentRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<OrderAssemblyAssignment> GetByIdAsync(int id)
        {
            var assignment = await _db.OrderAssemblyAssignments.FirstOrDefaultAsync(a => a.Id == id);
            if (assignment == null) return null;

            var lines = await _db.OrderAssemblyLines
                .Where(l => l.OrderAssemblyAssignmentId == id)
                .ToListAsync();

            var domainLines = lines.Select(l => l.ToDomain()).ToList();
            return assignment.ToDomainWithLines(domainLines);
        }

        public async Task<OrderAssemblyAssignment> GetByTaskIdAsync(int taskId)
        {
            var assignment = await _db.OrderAssemblyAssignments.FirstOrDefaultAsync(a => a.TaskId == taskId);
            if (assignment == null) return null;

            var lines = await _db.OrderAssemblyLines
                .Where(l => l.OrderAssemblyAssignmentId == assignment.Id)
                .ToListAsync();

            var domainLines = lines.Select(l => l.ToDomain()).ToList();
            return assignment.ToDomainWithLines(domainLines);
        }

        public async Task<List<OrderAssemblyAssignment>> GetByUserIdAsync(int userId)
        {
            var assignments = await _db.OrderAssemblyAssignments.Where(a => a.AssignedToUserId == userId).ToListAsync();
            var result = new List<OrderAssemblyAssignment>();

            foreach (var model in assignments)
            {
                var lines = await _db.OrderAssemblyLines
                    .Where(l => l.OrderAssemblyAssignmentId == model.Id)
                    .ToListAsync();

                result.Add(model.ToDomainWithLines(lines.Select(l => l.ToDomain()).ToList()));
            }
            return result;
        }

        public async Task<List<OrderAssemblyAssignment>> GetByStatusAsync(AssignmentStatus status)
        {
            var assignments = await _db.OrderAssemblyAssignments.Where(a => a.Status == (int)status).ToListAsync();
            var result = new List<OrderAssemblyAssignment>();

            foreach (var model in assignments)
            {
                var lines = await _db.OrderAssemblyLines
                    .Where(l => l.OrderAssemblyAssignmentId == model.Id)
                    .ToListAsync();

                if (lines.Count > 0)
                {
                    result.Add(model.ToDomainWithLines(lines.Select(l => l.ToDomain()).ToList()));
                }
            }
            return result;
        }

        public async Task<List<OrderAssemblyAssignment>> GetByBranchIdAsync(int branchId)
        {
            var assignments = await _db.OrderAssemblyAssignments.Where(a => a.BranchId == branchId).ToListAsync();
            var result = new List<OrderAssemblyAssignment>();

            foreach (var model in assignments)
            {
                var lines = await _db.OrderAssemblyLines
                    .Where(l => l.OrderAssemblyAssignmentId == model.Id)
                    .ToListAsync();

                result.Add(model.ToDomainWithLines(lines.Select(l => l.ToDomain()).ToList()));
            }
            return result;
        }

        public async Task<int> AddAsync(OrderAssemblyAssignment assignment)
        {
            var model = assignment.ToModel();
            model.Id = 0;
            return await _db.InsertWithInt32IdentityAsync(model);
        }

        public async Task<int> UpdateAsync(OrderAssemblyAssignment assignment)
        {
            var model = assignment.ToModel();
            return await _db.UpdateAsync(model);
        }
    }

    public class OrderAssemblyLineRepository : IOrderAssemblyLineRepository
    {
        private readonly ITaskDataConnection _db;
        private readonly ILogger<OrderAssemblyLineRepository> _logger;

        public OrderAssemblyLineRepository(
            ITaskDataConnection db,
            ILogger<OrderAssemblyLineRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<OrderAssemblyLine> GetByIdAsync(int id)
        {
            var line = await _db.OrderAssemblyLines.FirstOrDefaultAsync(l => l.Id == id);
            return line?.ToDomain();
        }

        public async Task<List<OrderAssemblyLine>> GetByAssignmentIdAsync(int assignmentId)
        {
            var lines = await _db.OrderAssemblyLines.Where(l => l.OrderAssemblyAssignmentId == assignmentId).ToListAsync();
            return lines.Select(l => l.ToDomain()).ToList();
        }

        public async Task<int> AddAsync(OrderAssemblyLine line)
        {
            var model = line.ToModel();
            model.Id = 0;
            return await _db.InsertWithInt32IdentityAsync(model);
        }

        public async Task<int> UpdateAsync(OrderAssemblyLine line)
        {
            var model = line.ToModel();
            return await _db.UpdateAsync(model);
        }

        public async Task<int> AddBatchAsync(List<OrderAssemblyLine> lines)
        {
            var models = lines.Select(l => l.ToModel()).ToList();
            foreach (var m in models) m.Id = 0;

            int inserted = 0;
            foreach (var m in models)
            {
                inserted += await _db.InsertWithInt32IdentityAsync(m); // Возвращает Id вставленной записи, просто считаем что успешно вставили. В реальности можно использовать BulkCopy
            }
            return inserted;
        }
    }
}
