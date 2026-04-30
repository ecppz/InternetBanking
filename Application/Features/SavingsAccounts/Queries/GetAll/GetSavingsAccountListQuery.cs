using Application.Interfaces;
using Domain.Common.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.SavingsAccounts.Queries.GetAll
{
    public class GetSavingsAccountListQuery : IRequest<SavingsAccountListResponse>
    {
        public string? DocumentNumber { get; set; } // Cédula
        public string? Status { get; set; } // activo | cancelado
        public string? Type { get; set; } // principal | secundaria
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

 
    public class SavingsAccountListResponse
    {
        public List<SavingsAccountListDto> Data { get; set; } = new();
        public PaginationMetadata Paginacion { get; set; } = new();
    }

    public class SavingsAccountListDto
    {
        public string NumeroCuenta { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string ApellidoCliente { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string TipoCuenta { get; set; } = string.Empty; // principal | secundaria
        public string Estado { get; set; } = string.Empty; // activo | cancelado
    }

    public class PaginationMetadata
    {
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
    }

    public class GetSavingsAccountListQueryHandler : IRequestHandler<GetSavingsAccountListQuery, SavingsAccountListResponse>
    {
        private readonly ISavingsAccountRepository _accountRepository;
        private readonly IUserAccountServiceForWebApi _userService;

        public GetSavingsAccountListQueryHandler(
            ISavingsAccountRepository accountRepository,
            IUserAccountServiceForWebApi userService)
        {
            _accountRepository = accountRepository;
            _userService = userService;
        }

        public async Task<SavingsAccountListResponse> Handle(GetSavingsAccountListQuery request, CancellationToken cancellationToken)
        {
            // 1. Obtener todas las cuentas (o filtradas por repositorio si es posible)
            // Nota: Usamos GetAllSavingsAccountsAsync como base para filtros manuales o GetFilteredAsync
            var allAccounts = await _accountRepository.GetAllSavingsAccountsAsync();

            // 2. Filtro por Cédula (DocumentNumber)
            if (!string.IsNullOrEmpty(request.DocumentNumber))
            {
                var allUsers = await _userService.GetAllCustomersAsync();
                var user = allUsers.FirstOrDefault(u => u.DocumentNumber == request.DocumentNumber);

                if (user != null)
                    allAccounts = allAccounts.Where(a => a.UserId == Guid.Parse(user.Id)).ToList();
                else
                    allAccounts = new List<Domain.Entities.SavingsAccount>(); // No hay match
            }

            // 3. Filtro por Estado (activo | cancelado)
            if (!string.IsNullOrEmpty(request.Status))
            {
                var statusEnum = request.Status.ToLower() == "activo"
                    ? SavingsAccountStatus.Activa
                    : SavingsAccountStatus.Cancelada;

                allAccounts = allAccounts.Where(a => a.Status == statusEnum).ToList();
            }

            // 4. Filtro por Tipo (principal | secundaria)
            if (!string.IsNullOrEmpty(request.Type))
            {
                bool lookForPrimary = request.Type.ToLower() == "principal";
                allAccounts = allAccounts.Where(a => a.IsPrimary == lookForPrimary).ToList();
            }

            // 5. Paginación manual
            int totalRegistros = allAccounts.Count;
            int totalPaginas = (int)Math.Ceiling(totalRegistros / (double)request.PageSize);

            var pagedAccounts = allAccounts
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // 6. Mapeo final incluyendo datos de usuario
            var responseData = new List<SavingsAccountListDto>();

            foreach (var acc in pagedAccounts)
            {
                var user = await _userService.GetUserById(acc.UserId.ToString());
                responseData.Add(new SavingsAccountListDto
                {
                    NumeroCuenta = acc.AccountNumber,
                    NombreCliente = user?.Name ?? "N/A",
                    ApellidoCliente = user?.LastName ?? "N/A",
                    Balance = acc.Balance,
                    TipoCuenta = acc.IsPrimary ? "principal" : "secundaria",
                    Estado = acc.Status == SavingsAccountStatus.Activa ? "activo" : "cancelado"
                });
            }

            return new SavingsAccountListResponse
            {
                Data = responseData,
                Paginacion = new PaginationMetadata
                {
                    PaginaActual = request.Page,
                    TotalPaginas = totalPaginas,
                    TotalRegistros = totalRegistros
                }
            };
        }
    }

}
