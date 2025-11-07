namespace Application.Interfaces
{
    public interface IGenericService<DtoModel>        
        where DtoModel : class
    {
        Task<DtoModel?> AddAsync(DtoModel dto);
        Task<DtoModel?> UpdateAsync(DtoModel dto, Guid id);
        Task<bool> DeleteAsync(Guid id);
        Task<DtoModel?> GetById(Guid id);
        Task<List<DtoModel>> GetAll();     
    }
}