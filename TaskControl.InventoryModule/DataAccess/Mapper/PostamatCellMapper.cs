
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.InventoryModule.Domain;
using UnitsNet;

namespace TaskControl.InventoryModule.DataAccess.Mapper
{
    public static class PostamatCellMapper
    {
        public static PostamatCellModel? ToModel(this PostamatCell entity)
        {
            if (entity == null) return null;

            return new PostamatCellModel
            {
                CellId = entity.CellId,
                PostamatId = entity.PostamatId,
                CellNumber = entity.CellNumber,
                SizeLabel = entity.SizeLabel,

                // Извлекаем значение в миллиметрах для сохранения в БД
                Length = entity.Length.Millimeters,
                Width = entity.Width.Millimeters,
                Height = entity.Height.Millimeters,

                Status = entity.Status.ToString()
            };
        }

        public static PostamatCell? ToDomain(this PostamatCellModel model)
        {
            if (model == null) return null;

            return new PostamatCell
            {
                CellId = model.CellId,
                PostamatId = model.PostamatId,
                CellNumber = model.CellNumber,
                SizeLabel = model.SizeLabel,

                // Преобразуем double из БД в строгий тип UnitsNet.Length
                Length = Length.FromMillimeters(model.Length),
                Width = Length.FromMillimeters(model.Width),
                Height = Length.FromMillimeters(model.Height),

                Status = Enum.TryParse<PostamatCellStatus>(model.Status, out var status)
                            ? status
                            : PostamatCellStatus.Available
            };
        }
    }
}