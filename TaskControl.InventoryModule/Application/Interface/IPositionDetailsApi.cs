using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InventoryModule.Application.DTOs;

namespace TaskControl.InventoryModule.Application.Interface
{
    public interface IPositionDetailsApi
    {
        /// <summary>
        /// Получить детали позиции по её ID
        /// </summary>
        Task<PositionDetailsDto?> GetPositionDetailsByIdAsync(int positionId);

        /// <summary>
        /// Получить детали нескольких позиций (batch запрос)
        /// </summary>
        Task<IEnumerable<PositionDetailsDto>> GetPositionDetailsAsync(IEnumerable<int> positionIds);

        /// <summary>
        /// Получить детали позиций по филиалу
        /// </summary>
        Task<IEnumerable<PositionDetailsDto>> GetPositionsByBranchAsync(int branchId);
    }
}
