using Application.Interfaces;
using AutoMapper;
using Domain.Interfaces;

namespace Application.Services
{
    public class GenericService<Entity , DtoModel> : IGenericService<DtoModel>
        where Entity : class
        where DtoModel : class
    {
        private readonly IGenericRepository<Entity> repository;
        private readonly IMapper mapper;

        public GenericService(IGenericRepository<Entity> repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }
        public virtual async Task<DtoModel?> AddAsync(DtoModel dto)
        {
            try
            {
                Entity entity = mapper.Map<Entity>(dto);
                Entity? returnEntity = await repository.AddAsync(entity);
                if (returnEntity == null)
                {
                    return null;
                }

                return mapper.Map<DtoModel>(returnEntity);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public virtual async Task<DtoModel?> UpdateAsync(DtoModel dto, Guid id)
        {
            try
            {
                Entity entity = mapper.Map<Entity>(dto);
                Entity? returnEntity = await repository.UpdateAsync(id, entity);
                if (returnEntity == null)
                {
                    return null;
                }

                return mapper.Map<DtoModel>(returnEntity);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public virtual async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                await repository.DeleteAsync(id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public virtual async Task<DtoModel?> GetById(Guid id)
        {
            try
            {
                var entity = await repository.GetById(id);
                if (entity == null)
                {
                    return null;
                }

                DtoModel dto = mapper.Map<DtoModel>(entity);
                return dto;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public virtual async Task<List<DtoModel>> GetAll()
        {
            try
            {
                var listEntities = await repository.GetAllList();
                var listEntityDtos = mapper.Map<List<DtoModel>>(listEntities);

                return listEntityDtos;
            }
            catch (Exception)
            {
                return [];
            }
        }     
    }
}