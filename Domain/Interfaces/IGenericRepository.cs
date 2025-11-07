namespace Domain.Interfaces
{
    public interface IGenericRepository<Entity>
        where Entity : class
    {
        Task<Entity?> AddAsync(Entity entity);
        Task DeleteAsync(Guid id);
        Task<List<Entity>> GetAllList();
        IQueryable<Entity> GetAllQuery();
        Task<Entity?> GetById(Guid id);
        Task<Entity?> UpdateAsync(Guid id, Entity entity);
        Task<List<Entity>> GetAllListWithInclude(List<string> properties);
        IQueryable<Entity> GetAllQueryWithInclude(List<string> properties);
    }
}
