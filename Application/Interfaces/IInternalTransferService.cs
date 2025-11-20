using Application.Dtos.Transfer;

namespace Application.Interfaces
{
    public interface IInternalTransferService : IGenericService<InternalTransferRequestDto>
    {
        // Ejecuta la transferencia entre cuentas propias validando propiedad, estado y fondos
        Task<InternalTransferResultDto> TransferAsync(Guid userId, InternalTransferRequestDto dto);
    }
}
