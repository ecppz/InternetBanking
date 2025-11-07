using Domain.Interfaces;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    public class GenericRepository<Entity> : IGenericRepository<Entity>
        where Entity : class        
    {
        protected readonly InternetBankingContextDB context;

        public GenericRepository(InternetBankingContextDB context)
        {
            this.context = context;
        }
        public virtual async Task<Entity?> AddAsync(Entity entity)
        {
            await context.Set<Entity>().AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        public virtual async Task<Entity?> UpdateAsync(Guid id, Entity entity)
        {
            var entry = await context.Set<Entity>().FindAsync(id);

            if (entry != null)
            {
                context.Entry(entry).CurrentValues.SetValues(entity);
                await context.SaveChangesAsync();
                return entry;
            }

            return null;
        }
        public virtual async Task DeleteAsync(Guid id)
        {
            var entity = await context.Set<Entity>().FindAsync(id);
            if (entity != null)
            {
                context.Set<Entity>().Remove(entity);
                await context.SaveChangesAsync();
            }
        }
        public virtual async Task<List<Entity>> GetAllList()
        {
            return await context.Set<Entity>().ToListAsync();
        }

        public virtual async Task<List<Entity>> GetAllListWithInclude(List<string> properties)
        {
            var query = context.Set<Entity>().AsQueryable();

            foreach(var property in properties)
            {
                query = query.Include(property);
            }

            return await query.ToListAsync(); 
        }    
        public virtual async Task<Entity?> GetById(Guid id)
        {
            return await context.Set<Entity>().FindAsync(id);
        }
        public virtual IQueryable<Entity> GetAllQuery()
        {
            return context.Set<Entity>().AsQueryable();
        }
        public virtual IQueryable<Entity> GetAllQueryWithInclude(List<string> properties)
        {
            var query = context.Set<Entity>().AsQueryable();

            foreach (var property in properties)
            {
                query = query.Include(property);
            }

            return query; 
        }
    }
}