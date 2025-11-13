namespace Domain.Interfaces
{
    public interface IGenericRepository<Entity>
        where Entity : class
    {
        Task<Entity?> AddAsync(Entity entity);
        Task<int> AddRangeAsync(IEnumerable<Entity> entities);
        Task DeleteAsync(Guid id);
        Task<List<Entity>> GetAllList();
        IQueryable<Entity> GetAllQuery();
        Task<Entity?> GetById(Guid id);
        Task<Entity?> UpdateAsync(Guid id, Entity entity);
        Task<int> UpdateRangeAsync(IEnumerable<Entity> entities);
        Task<List<Entity>> GetAllListWithInclude(List<string> properties);
        IQueryable<Entity> GetAllQueryWithInclude(List<string> properties);

    }
}
