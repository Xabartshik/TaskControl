using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.InventoryModule.Domain;

namespace TaskControl.InventoryModule.DataAccess.Mapper
{
    public static class PostamatMapper
    {
        public static PostamatModel? ToModel(this Postamat entity)
        {
            if (entity == null) return null;

            return new PostamatModel
            {
                PostamatId = entity.PostamatId,
                Address = entity.Address,
                Status = entity.Status.ToString(),
                CreatedAt = DateTime.UtcNow
            };
        }

        public static Postamat? ToDomain(this PostamatModel model)
        {
            if (model == null) return null;

            return new Postamat
            {
                PostamatId = model.PostamatId,
                Address = model.Address,
                Status = Enum.TryParse<PostamatStatus>(model.Status, out var status)
                            ? status
                            : PostamatStatus.Active
            };
        }
    }
}