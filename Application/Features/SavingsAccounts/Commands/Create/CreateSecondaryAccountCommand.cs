using Application.Interfaces;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.SavingsAccounts.Commands.Create
{
    public class CreateSecondaryAccountCommand : IRequest<CreateSecondaryAccountResponse>
    {
        public required string CedulaCliente { get; set; }
        public decimal BalanceInicial { get; set; }
    }

    public class CreateSecondaryAccountResponse
    {
        public string? AccountNumber { get; set; }
        public bool HasError { get; set; }
        public string? Error { get; set; }
    }

    public class CreateSecondaryAccountCommandHandler : IRequestHandler<CreateSecondaryAccountCommand, CreateSecondaryAccountResponse>
    {
        private readonly ISavingsAccountRepository _accountRepository;
        private readonly IUserAccountServiceForWebApi _userService;
        private readonly ITransactionRepository _transactionRepository;

        public CreateSecondaryAccountCommandHandler(
            ISavingsAccountRepository accountRepository,
            IUserAccountServiceForWebApi userService,
            ITransactionRepository transactionRepository)
        {
            _accountRepository = accountRepository;
            _userService = userService;
            _transactionRepository = transactionRepository;
        }

        public async Task<CreateSecondaryAccountResponse> Handle(CreateSecondaryAccountCommand request, CancellationToken cancellationToken)
        {
            // 1. Validar que el cliente existe por cédula
            var allUsers = await _userService.GetAllCustomersAsync();
            var user = allUsers.FirstOrDefault(u => u.DocumentNumber == request.CedulaCliente);

            if (user == null)
            {
                return new CreateSecondaryAccountResponse { HasError = true, Error = "El cliente no existe." };
            }

            // 2. Generar número de cuenta de 9 dígitos único
            string newAccountNumber;
            bool exists;
            do
            {
                newAccountNumber = new Random().Next(100000000, 999999999).ToString();
                exists = await _accountRepository.ExistsAccountNumberAsync(newAccountNumber);
            } while (exists);

            // 3. Crear la cuenta secundaria
            var newAccount = new SavingsAccount
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(user.Id),
                AccountNumber = newAccountNumber,
                Balance = request.BalanceInicial,
                IsPrimary = false, // Es secundaria
                Status = SavingsAccountStatus.Activa
            };

            await _accountRepository.AddAsync(newAccount);

            // 4. Si hay balance inicial, registrar la transacción
            if (request.BalanceInicial > 0)
            {
                await _transactionRepository.AddAsync(new Transaction
                {
                    Id = Guid.NewGuid(),
                    OriginAccountId = newAccount.Id,
                    DestinationAccountId = null, // Es un depósito, no hay cuenta destino
                    PerformedByUserId = Guid.Empty, // TODO: Deberías pasar el ID del Admin logueado
                    Amount = request.BalanceInicial,
                    Date = DateTime.Now,
                    Type = TransactionType.Deposit,
                    Status = TransactionStatus.Approved,
                    Beneficiary = $"{user.Name} {user.LastName}", // El cliente recibe el dinero
                    Origin = "Sistema - Apertura de Cuenta",      // Origen de la transacción
                    Reason = "Depósito inicial por apertura de cuenta secundaria"
                });
            }

            return new CreateSecondaryAccountResponse
            {
                AccountNumber = newAccountNumber,
                HasError = false
            };
        }
    }


}
