using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InformationModule.Application.DTOs;
using TaskControl.InformationModule.Application.Services;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase, ICrudController<EmployeeDto, int>
    {
        private readonly IService<EmployeeDto> _service;
        private readonly CourierCapabilityService _courierCapabilityService;

        public EmployeeController(
            IService<EmployeeDto> service,
            CourierCapabilityService courierCapabilityService) 
        {
            _service = service;
            _courierCapabilityService = courierCapabilityService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
        {
            var employees = await _service.GetAll();
            return Ok(employees);
        }

        /// <summary>
        /// Специальный эндпоинт для регистрации курьера вместе с его машиной
        /// </summary>
        [HttpPost("courier")]
        public async Task<ActionResult<int>> AddCourier([FromBody] RegisterCourierDto dto)
        {
            // 1. Создаем DTO для сотрудника и жестко задаем роль Courier
            var employeeDto = new EmployeeDto
            {
                Surname = dto.Surname,
                Name = dto.Name,
                MiddleName = dto.MiddleName,
                Role = WorkerRole.Courier // Роль = 4
            };

            // 2. Сохраняем сотрудника в таблицу employees
            var newEmployeeId = await _service.Add(employeeDto);

            // 3. Формируем характеристики машины
            var capabilityModel = new CourierCapabilityModel
            {
                EmployeeId = newEmployeeId,
                VehicleTypeId = dto.VehicleTypeId,
                MaxWeightGrams = dto.MaxWeightGrams,
                MaxLengthMm = dto.MaxLengthMm,
                MaxWidthMm = dto.MaxWidthMm,
                MaxHeightMm = dto.MaxHeightMm
            };

            // 4. Сохраняем характеристики (ВНУТРИ этого метода вызовется событие для создания ячейки на складе!)
            // Указываем BranchId, к которому приписан курьер (-1 - виртуальная ячейка курьера)
            await _courierCapabilityService.AddOrUpdateCourierCapabilityAsync(capabilityModel, defaultBranchId: -1);

            return CreatedAtAction(nameof(GetById), new { id = newEmployeeId }, newEmployeeId);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> GetById(int id)
        {
            var employee = await _service.GetById(id);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Add(EmployeeDto dto)
        {
            var newId = await _service.Add(dto);
            return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
        }

        [HttpPut]
        public async Task<IActionResult> Update(EmployeeDto dto)
        {
            var result = await _service.Update(dto);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}