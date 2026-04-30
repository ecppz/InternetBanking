using Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.SavingsAccounts.Queries.GetAccountTrasactions
{
    public class GetAccountTransactionsQuery : IRequest<AccountTransactionsResponse>
    {
        public string AccountNumber { get; set; } = string.Empty;
    }

    public class AccountTransactionsResponse
    {
        public List<AccountTransactionDto> Transacciones { get; set; } = new();
    }

    public class AccountTransactionDto
    {
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Beneficiario { get; set; } = string.Empty;
        public string Origen { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class GetAccountTransactionsQueryHandler : IRequestHandler<GetAccountTransactionsQuery, AccountTransactionsResponse?>
    {
        private readonly ISavingsAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;

        public GetAccountTransactionsQueryHandler(
            ISavingsAccountRepository accountRepository,
            ITransactionRepository transactionRepository)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<AccountTransactionsResponse?> Handle(GetAccountTransactionsQuery request, CancellationToken cancellationToken)
        {
            // 1. Validar existencia de la cuenta
            var account = await _accountRepository.GetByAccountNumberAsync(request.AccountNumber);
            if (account == null) return null;

            // 2. Obtener transacciones ordenadas
            var transactions = await _transactionRepository.GetAllByAccountIdOrderedAsync(account.Id);

            // 3. Mapear al formato exacto del PDF
            return new AccountTransactionsResponse
            {
                Transacciones = transactions.Select(t => new AccountTransactionDto
                {
                    Fecha = t.Date,
                    Monto = t.Amount,
                    // Mapeo manual para asegurar los strings exactos del mandato
                    Tipo = t.Type.ToString().ToUpper(),
                    Beneficiario = t.Beneficiary,
                    Origen = t.Origin,
                    Estado = t.Status.ToString().ToUpper()
                }).ToList()
            };
        }
    }

}
