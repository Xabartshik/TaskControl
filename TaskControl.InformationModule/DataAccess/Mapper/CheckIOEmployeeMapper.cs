using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.DataAccess.Mapper
{
    public static class CheckIoEmployeeMapper
    {
        public static CheckIOEmployeeModel ToModel(this CheckIOEmployee entity)
        {
            if (entity == null) return null;

            return new CheckIOEmployeeModel
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                BranchId = entity.BranchId,
                CheckType = entity.CheckType,
                CheckTimeStamp = entity.CheckTimeStamp
            };
        }

        public static CheckIOEmployee ToDomain(this CheckIOEmployeeModel model)
        {
            if (model == null) return null;

            return new CheckIOEmployee
            {
                Id = model.Id,
                EmployeeId = model.EmployeeId,
                BranchId = model.BranchId,
                CheckType = model.CheckType,
                CheckTimeStamp = model.CheckTimeStamp
            };
        }
    }
}
