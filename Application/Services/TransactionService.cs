using Application.Dtos.Email;
using Application.Dtos.Transaction;
using Application.Interfaces;
using Application.ViewModels.Transaction;
using AutoMapper;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;
using System.Globalization;
using Domain.Common.Enums.Extensions;


namespace Application.Services
{
    public class TransactionService : GenericService<Transaction, TransactionDto>, ITransactionService
    {
        private readonly ITransactionRepository transactionRepository;
        private readonly ISavingsAccountRepository savingsAccountRepository;
        private readonly IUserAccountService userAccountService;
        private readonly IEmailService emailService;
        private readonly IMapper mapper;

        public TransactionService(ITransactionRepository transactionRepository, IEmailService email, IUserAccountService userAccountService, ISavingsAccountRepository savingsAccountRepository, IMapper mapper) : base(transactionRepository, mapper)
        {
            this.transactionRepository = transactionRepository;
            this.mapper = mapper;
            this.savingsAccountRepository = savingsAccountRepository;
            this.userAccountService = userAccountService;
            emailService = email;
        }

        public async Task<List<TransactionDto>> GetByOriginAccountIdAsync(Guid accountId)
        {
            var transactions = await transactionRepository.GetByOriginAccountIdAsync(accountId);
            return mapper.Map<List<TransactionDto>>(transactions);
        }

        public async Task<List<TransactionDto>> GetByDestinationAccountIdAsync(Guid accountId)
        {
            var transactions = await transactionRepository.GetByDestinationAccountIdAsync(accountId);
            return mapper.Map<List<TransactionDto>>(transactions);
        }

        public async Task<List<TransactionDto>> GetAllByAccountIdAsync(Guid accountId)
        {
            var transactions = await transactionRepository.GetAllByAccountIdAsync(accountId);
            return mapper.Map<List<TransactionDto>>(transactions);
        }

        public async Task<List<TransactionDto>> GetAllByAccountIdOrderedAsync(Guid accountId)
        {
            var transactions = await transactionRepository.GetAllByAccountIdOrderedAsync(accountId);
            return mapper.Map<List<TransactionDto>>(transactions);
        }

        public async Task<List<TransactionDto>> GetAllByUserIdAsync(Guid userId)
        {
            var accounts = await savingsAccountRepository.GetAllByUserIdOrderedAsync(userId);
            var allTransactions = new List<Transaction>();

            foreach (var account in accounts)
            {
                var tx = await transactionRepository.GetAllByAccountIdAsync(account.Id);
                allTransactions.AddRange(tx);
            }

            return mapper.Map<List<TransactionDto>>(allTransactions);
        }

        public async Task<List<TransactionDto>> GetAllByUserIdOrderedAsync(Guid userId)
        {
            var accounts = await savingsAccountRepository.GetAllByUserIdOrderedAsync(userId);
            var allTransactions = new List<Transaction>();

            foreach (var account in accounts)
            {
                var tx = await transactionRepository.GetAllByAccountIdOrderedAsync(account.Id);
                allTransactions.AddRange(tx);
            }

            var ordered = allTransactions.OrderByDescending(t => t.Date).ToList();
            return mapper.Map<List<TransactionDto>>(ordered);
        }

        //Para cajero:
        public async Task<bool> IsAccountValidAsync(string accountNumber)
        {
            var account = await savingsAccountRepository.GetByAccountNumberAsync(accountNumber);
            return account != null && account.Status == SavingsAccountStatus.Activa;
        }

        public async Task<string?> GetAccountOwnerFullNameAsync(string accountNumber)
        {
            var account = await savingsAccountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null) return null;

            var user = await userAccountService.GetUserById(account.UserId.ToString());
            return user != null ? $"{user.Name} {user.LastName}".Trim() : "Desconocido";
        }

        public async Task<bool> HasSufficientFundsAsync(string accountNumber, decimal amount)
        {
            var account = await savingsAccountRepository.GetByAccountNumberAsync(accountNumber);
            return account != null && account.Balance >= amount;
        }

        public async Task<ConfirmThirdPartyTransferViewModel?> PrepareTransferConfirmationAsync(string originAccountNumber, string destinationAccountNumber, decimal amount)
        {
            var destination = await savingsAccountRepository.GetByAccountNumberAsync(destinationAccountNumber);
            if (destination == null) return null;

            var user = await userAccountService.GetUserById(destination.UserId.ToString());
            if (user == null) return null;

            return new ConfirmThirdPartyTransferViewModel
            {
                OriginAccountNumber = originAccountNumber,
                DestinationAccountNumber = destinationAccountNumber,
                DestinationFullName = user != null ? $"{user.Name} {user.LastName}".Trim() : "Desconocido",
                Amount = amount,
                Timestamp = DateTime.Now
            };
        }

        public async Task<bool> ExecuteThirdPartyTransferAsync(string originAccountNumber, string destinationAccountNumber, decimal amount)
        {
            // Validación 1: Cuenta origen
            if (!await IsOriginAccountValidAsync(originAccountNumber))
            {
                await RegisterRejectedTransactionAsync(originAccountNumber, destinationAccountNumber, amount, "Cuenta origen inválida o inactiva");
                return false;
            }

            // Validación 2: Fondos suficientes
            if (!await HasSufficientFundsAsync(originAccountNumber, amount))
            {
                await RegisterRejectedTransactionAsync(originAccountNumber, destinationAccountNumber, amount, "Fondos insuficientes");
                return false;
            }

            // Validación 3: Cuenta destino
            if (!await IsDestinationAccountValidAsync(destinationAccountNumber))
            {
                await RegisterRejectedTransactionAsync(originAccountNumber, destinationAccountNumber, amount, "Cuenta destino inválida o inactiva");
                return false;
            }

            // Recuperar cuentas
            var origin = await savingsAccountRepository.GetByAccountNumberAsync(originAccountNumber);
            var destination = await savingsAccountRepository.GetByAccountNumberAsync(destinationAccountNumber);

            // Actualizar balances
            origin.Balance -= amount;
            destination.Balance += amount;

            await savingsAccountRepository.UpdateAsync(origin.Id, origin);
            await savingsAccountRepository.UpdateAsync(destination.Id, destination);

            var now = DateTime.UtcNow;

            // Registro único de la transacción
            await transactionRepository.RegisterTransactionAsync(new Transaction
            {
                Id = Guid.NewGuid(),
                OriginAccountId = origin.Id,
                DestinationAccountId = destination.Id,
                Amount = amount,
                Date = now,
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Approved,
                Origin = origin.AccountNumber,
                Beneficiary = destination.AccountNumber
            });

            // Envío de correos
            var originUser = await userAccountService.GetUserById(origin.UserId.ToString());
            var destinationUser = await userAccountService.GetUserById(destination.UserId.ToString());

            if (originUser != null && destinationUser != null)
            {
                var formattedAmount = amount.ToString("C", new CultureInfo("es-DO"));
                var formattedDate = DateTime.Now.ToString("dd/MM/yyyy h:mm tt", new CultureInfo("es-DO"));

                // Correo para origen
                var last4Dest = destination.AccountNumber[^4..];
                var subjectOrigin = $"Transacción realizada a la cuenta {last4Dest}";
                var bodyOrigin = $@"
                    <div style='font-family:Arial,sans-serif;color:#333'>
                        <h2 style='color:#2E86C1'>Transferencia Exitosa</h2>
                        <p>Se ha enviado <strong>{formattedAmount}</strong> a la cuenta destino <strong>{last4Dest}</strong>.</p>
                        <p>Fecha y hora: <strong>{formattedDate}</strong></p>
                        <p style='margin-top:20px'>Gracias por usar nuestro servicio.</p>
                    </div>";

                await emailService.SendAsync(new EmailRequestDto
                {
                    To = originUser.Email,
                    Subject = subjectOrigin,
                    HtmlBody = bodyOrigin
                });

                // Correo para destino
                var last4Origin = origin.AccountNumber[^4..];
                var subjectDest = $"Transacción enviada desde la cuenta {last4Origin}";
                var bodyDest = $@"
                    <div style='font-family:Arial,sans-serif;color:#333'>
                        <h2 style='color:#28B463'>Depósito Recibido</h2>
                        <p>Ha recibido un depósito de <strong>{formattedAmount}</strong> desde la cuenta <strong>{last4Origin}</strong>.</p>
                        <p>Fecha y hora: <strong>{formattedDate}</strong></p>
                        <p style='margin-top:20px'>Gracias por confiar en nosotros.</p>
                    </div>";

                await emailService.SendAsync(new EmailRequestDto
                {
                    To = destinationUser.Email,
                    Subject = subjectDest,
                    HtmlBody = bodyDest
                });
            }

            return true;
        }

        public async Task<bool> IsOriginAccountValidAsync(string accountNumber)
        {
            var account = await savingsAccountRepository.GetByAccountNumberAsync(accountNumber);
            return account != null && account.Status == SavingsAccountStatus.Activa;
        }

        public async Task<bool> IsDestinationAccountValidAsync(string accountNumber)
        {
            var account = await savingsAccountRepository.GetByAccountNumberAsync(accountNumber);
            return account != null && account.Status == SavingsAccountStatus.Activa;
        }

        public async Task<string?> GetAccountStatusAsync(string accountNumber)
        {
            var account = await savingsAccountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null) return null;
            return account.Status.ToVisualLabel();
        }

        public async Task RegisterRejectedTransactionAsync(string originAccountNumber, string destinationAccountNumber, decimal amount, string reason)
        {
            var origin = await savingsAccountRepository.GetByAccountNumberAsync(originAccountNumber);
            var destination = await savingsAccountRepository.GetByAccountNumberAsync(destinationAccountNumber);

            await transactionRepository.RegisterTransactionAsync(new Transaction
            {
                Id = Guid.NewGuid(),
                OriginAccountId = origin?.Id ?? Guid.Empty,
                DestinationAccountId = destination?.Id ?? Guid.Empty,
                Amount = amount,
                Date = DateTime.UtcNow,
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Rejected,
                Reason = reason,
                Origin = originAccountNumber,
                Beneficiary = destinationAccountNumber
            });
        }

        //Para deposito y retiro

        public async Task<DepositConfirmationDto?> ValidateDepositAsync(DepositRequestDto request)
        {
            var destination = await savingsAccountRepository.GetByAccountNumberAsync(request.DestinationAccountNumber);

            if (destination == null || destination.Status != SavingsAccountStatus.Activa)
                return null;

            var user = await userAccountService.GetUserById(destination.UserId.ToString());
            if (user == null)
                return null;

            return new DepositConfirmationDto
            {
                DestinationAccountNumber = destination.AccountNumber,
                DestinationOwnerFullName = $"{user.Name?.Trim()} {user.LastName?.Trim()}".Trim(),
                Amount = request.Amount
            };
        }

        public async Task<bool> ExecuteDepositAsync(DepositRequestDto request)
        {
            var destination = await savingsAccountRepository.GetByAccountNumberAsync(request.DestinationAccountNumber);
            if (destination == null || destination.Status != SavingsAccountStatus.Activa)
                return false;

            destination.Balance += request.Amount;
            await savingsAccountRepository.UpdateAsync(destination.Id, destination);

            var now = DateTime.UtcNow;

            await transactionRepository.RegisterTransactionAsync(new Transaction
            {
                Id = Guid.NewGuid(),
                OriginAccountId = Guid.Empty, // No hay cuenta origen
                DestinationAccountId = destination.Id,
                Amount = request.Amount,
                Date = now,
                Type = TransactionType.Deposit,
                Status = TransactionStatus.Approved,
                Origin = "DEPÓSITO",
                Beneficiary = destination.AccountNumber
            });

            var user = await userAccountService.GetUserById(destination.UserId.ToString());
            if (user != null)
            {
                var formattedAmount = request.Amount.ToString("C", new CultureInfo("es-DO"));
                var formattedDate = DateTime.Now.ToString("dd/MM/yyyy h:mm tt", new CultureInfo("es-DO"));
                var last4 = destination.AccountNumber[^4..];

                var subject = $"Depósito realizado a su cuenta {last4}";
                var body = $@"
                        <div style='font-family:Arial,sans-serif;color:#333'>
                            <h2 style='color:#28B463'>Depósito Recibido</h2>
                            <p>Se ha depositado <strong>{formattedAmount}</strong> en su cuenta <strong>{last4}</strong>.</p>
                            <p>Fecha y hora: <strong>{formattedDate}</strong></p>
                            <p style='margin-top:20px'>Gracias por confiar en nosotros.</p>
                        </div>";

                await emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = subject,
                    HtmlBody = body
                });
            }

            return true;
        }


        public async Task<WithdrawalConfirmationDto?> ValidateWithdrawalAsync(WithdrawalRequestDto request)
        {
            var origin = await savingsAccountRepository.GetByAccountNumberAsync(request.OriginAccountNumber);

            if (origin == null || origin.Status != SavingsAccountStatus.Activa || origin.Balance < request.Amount)
                return null;

            var user = await userAccountService.GetUserById(origin.UserId.ToString());
            if (user == null)
                return null;

            return new WithdrawalConfirmationDto
            {
                OriginAccountNumber = origin.AccountNumber,
                OriginOwnerFullName = $"{user.Name?.Trim()} {user.LastName?.Trim()}".Trim(),
                Amount = request.Amount
            };
        }

        public async Task<bool> ExecuteWithdrawalAsync(WithdrawalRequestDto request)
        {
            var origin = await savingsAccountRepository.GetByAccountNumberAsync(request.OriginAccountNumber);
            if (origin == null || origin.Status != SavingsAccountStatus.Activa || origin.Balance < request.Amount)
                return false;

            origin.Balance -= request.Amount;
            await savingsAccountRepository.UpdateAsync(origin.Id, origin);

            var now = DateTime.UtcNow;

            await transactionRepository.RegisterTransactionAsync(new Transaction
            {
                Id = Guid.NewGuid(),
                OriginAccountId = origin.Id,
                DestinationAccountId = null, // No hay cuenta destino
                Amount = request.Amount,
                Date = now,
                Type = TransactionType.CashWithdrawal,
                Status = TransactionStatus.Approved,
                Origin = origin.AccountNumber,
                Beneficiary = "RETIRO"
            });

            var user = await userAccountService.GetUserById(origin.UserId.ToString());
            if (user != null)
            {
                var formattedAmount = request.Amount.ToString("C", new CultureInfo("es-DO"));
                var formattedDate = DateTime.Now.ToString("dd/MM/yyyy h:mm tt", new CultureInfo("es-DO"));
                var last4 = origin.AccountNumber[^4..];

                var subject = $"Retiro realizado a su cuenta {last4}";
                var body = $@"
                        <div style='font-family:Arial,sans-serif;color:#333'>
                            <h2 style='color:#C0392B'>Retiro Procesado</h2>
                            <p>Se ha retirado <strong>{formattedAmount}</strong> de su cuenta <strong>{last4}</strong>.</p>
                            <p>Fecha y hora: <strong>{formattedDate}</strong></p>
                            <p style='margin-top:20px'>Gracias por confiar en nosotros.</p>
                        </div>";

                await emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = subject,
                    HtmlBody = body
                });
            }

            return true;
        }


    }
}
