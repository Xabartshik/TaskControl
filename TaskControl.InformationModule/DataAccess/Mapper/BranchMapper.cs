using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.DataAccess.Mapper
{
    public static class BranchMapper
    {
        public static BranchModel ToModel(this Branch entity)
        {
            if (entity == null) return null;

            return new BranchModel
            {
                BranchId = entity.BranchId,
                BranchName = entity.BranchName,
                BranchType = entity.BranchType,
                Address = entity.Address,
                CreatedAt = DateTime.UtcNow // Устанавливается при создании
            };
        }

        public static Branch ToDomain(this BranchModel model)
        {
            if (model == null) return null;

            return new Branch
            {
                BranchId = model.BranchId,
                BranchName = model.BranchName,
                BranchType = model.BranchType,
                Address = model.Address
                // CreatedAt не передается в бизнес-сущность
            };
        }
    }
}
