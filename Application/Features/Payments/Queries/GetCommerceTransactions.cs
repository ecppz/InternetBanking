using Application.Interfaces;
using Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Payments.Queries
{
    public class GetCommerceTransactionsQuery : IRequest<CommerceTransactionsResponse>
    {
        public int? CommerceId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class CommerceTransactionsResponse
    {
        public List<CommerceTransactionDto> Transacciones { get; set; } = new();
    }

    public class CommerceTransactionDto
    {
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public string Tipo { get; set; } = "DÉBITO";
        public string Beneficiario { get; set; } = string.Empty;
        public string Origen { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class GetCommerceTransactionsQueryHandler : IRequestHandler<GetCommerceTransactionsQuery, CommerceTransactionsResponse>
    {
        private readonly ISavingsAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserAccountServiceForWebApi _userService;

        public GetCommerceTransactionsQueryHandler(
            ISavingsAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IUserAccountServiceForWebApi userService)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _userService = userService;
        }

        public async Task<CommerceTransactionsResponse> Handle(GetCommerceTransactionsQuery request, CancellationToken cancellationToken)
        {
            var response = new CommerceTransactionsResponse();

            var allUsers = await _userService.GetAllActiveUsers();
            var commerceUser = allUsers.FirstOrDefault(u => u.CommerceId == request.CommerceId);

            if (commerceUser == null) return response;


            var mainAccount = await _accountRepository.GetPrimaryByUserIdAsync(Guid.Parse(commerceUser.Id));
            if (mainAccount == null) return response;


            var transactions = await _transactionRepository.GetAllByAccountIdOrderedAsync(mainAccount.Id);

         
            response.Transacciones = transactions
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => new CommerceTransactionDto
                {
                    Fecha = t.Date,
                    Monto = t.Amount,
                    Tipo = t.Type.ToString().ToUpper(),
                    Beneficiario = t.Beneficiary,
                    Origen = t.Origin,
                    Estado = t.Status.ToString().ToUpper()
                }).ToList();

            return response;
        }
    }
}
