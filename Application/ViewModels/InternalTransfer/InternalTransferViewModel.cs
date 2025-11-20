using Application.Dtos.SavingsAccount;
using Application.Dtos.Transfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.InternalTransfer
{
    public class InternalTransferViewModel
    {
        // Lista de cuentas del usuario para seleccionar origen y destino
        public List<SavingsAccountDto> UserAccounts { get; set; } = new();

        // Datos del formulario de transferencia
        public InternalTransferRequestDto TransferRequest { get; set; } = new InternalTransferRequestDto
        {
            OriginAccountId = Guid.Empty,
            DestinationAccountId = Guid.Empty,
            Amount = 0
        };

        // Resultado de la operación (opcional)
        public InternalTransferResultDto? TransferResult { get; set; }
    }
}
