﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.InventoryModule.Domain;

namespace TaskControl.InventoryModule.DataAccess.Interface
{
    public interface IPositionCellRepository
    {
        Task<PositionCell?> GetByIdAsync(int id);
        Task<IEnumerable<PositionCell>> GetAllAsync();
        Task<int> AddAsync(PositionCell entity);
        Task<int> UpdateAsync(PositionCell entity);
        Task<int> DeleteAsync(int id);
    }
}