using Application.Dtos.Email;
using Application.Dtos.Transaction;
using Application.Interfaces;
using Application.ViewModels.Transaction;
using Application.ViewModels.TransactionBeneficiaryTransfer;
using AutoMapper;
using Domain.Common.Enums;
using Domain.Common.Enums.Extensions;
using Domain.Entities;
using Domain.Interfaces;
using System.Globalization;


namespace Application.Services
{
    public class TransactionService : GenericService<Transaction, TransactionDto>, ITransactionService
    {
        private readonly ITransactionRepository transactionRepository;
        private readonly ISavingsAccountRepository savingsAccountRepository;
        private readonly IUserAccountServiceForWebApp userAccountService;
        private readonly IBeneficiaryRepository beneficiaryRepository;
        private readonly IEmailService emailService;
        private readonly IMapper mapper;

        public TransactionService(ITransactionRepository transactionRepository, IBeneficiaryRepository beneficiaryRepository, IEmailService email, IUserAccountServiceForWebApp userAccountService, ISavingsAccountRepository savingsAccountRepository, IMapper mapper) : base(transactionRepository, mapper)
        {
            this.transactionRepository = transactionRepository;
            this.mapper = mapper;
            this.savingsAccountRepository = savingsAccountRepository;
            this.userAccountService = userAccountService;
            emailService = email;
            this.beneficiaryRepository = beneficiaryRepository;
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

            // Solo trasacciones que pertenecenen a la cuenta
            var filtered = transactions
           .Where(t => t.OriginAccountId == accountId || t.DestinationAccountId == accountId)
               .ToList();

            var dtos = mapper.Map<List<TransactionDto>>(filtered);

            foreach (var dto in dtos)
            {
                dto.EsPropia = dto.OriginAccountId == accountId;
                dto.VisualType = dto.EsPropia ? "DÉBITO" : "CRÉDITO";
                dto.Description = dto.EsPropia
                    ? $"Transferencia a Beneficiario {dto.Beneficiary}"
                    : $"Transferencia recibida de {dto.Origin}";
            }

            return dtos;
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
                var filtered = tx
                    .Where(t => !(t.OriginAccountId == account.Id && t.DestinationAccountId == account.Id))
                    .ToList();

                allTransactions.AddRange(filtered);
            }

            var ordered = allTransactions.OrderByDescending(t => t.Date).ToList();
            var dtos = mapper.Map<List<TransactionDto>>(ordered);

            foreach (var dto in dtos)
            {
                var accountId = accounts.FirstOrDefault(a => a.Id == dto.OriginAccountId || a.Id == dto.DestinationAccountId)?.Id;
                dto.VisualType = dto.OriginAccountId == accountId ? "DÉBITO" : "CRÉDITO";

                var original = ordered.First(t => t.Id == dto.Id);
                dto.Description = GetFriendlyDescription(original, dto.VisualType, accountId ?? Guid.Empty);
            }

            return dtos;
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

        public async Task<bool> ExecuteThirdPartyTransferAsync(string originAccountNumber, string destinationAccountNumber, decimal amount, Guid userId)
        {
            // Validación 1: Cuenta origen
            if (!await IsOriginAccountValidAsync(originAccountNumber))
            {
                await RegisterRejectedTransactionAsync(originAccountNumber, destinationAccountNumber, amount, "Cuenta origen inválida o inactiva", userId);
                return false;
            }

            // Validación 2: Fondos suficientes
            if (!await HasSufficientFundsAsync(originAccountNumber, amount))
            {
                await RegisterRejectedTransactionAsync(originAccountNumber, destinationAccountNumber, amount, "Fondos insuficientes", userId);
                return false;
            }

            // Validación 3: Cuenta destino
            if (!await IsDestinationAccountValidAsync(destinationAccountNumber))
            {
                await RegisterRejectedTransactionAsync(originAccountNumber, destinationAccountNumber, amount, "Cuenta destino inválida o inactiva", userId);
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
                Beneficiary = destination.AccountNumber,
                PerformedByUserId = userId
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

        public async Task RegisterRejectedTransactionAsync(string originAccountNumber, string destinationAccountNumber, decimal amount, string reason, Guid userId)
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
                Beneficiary = destinationAccountNumber,
                PerformedByUserId = userId
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

        public async Task<bool> ExecuteDepositAsync(DepositRequestDto request, Guid userId)
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
                Beneficiary = destination.AccountNumber,
                PerformedByUserId = userId
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

        public async Task<bool> ExecuteWithdrawalAsync(WithdrawalRequestDto request, Guid userId)
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
                Beneficiary = "RETIRO",
                PerformedByUserId = userId
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

        private string GetFriendlyDescription(Transaction transaction, string visualType, Guid currentAccountId)
        {
            if (transaction.Type == TransactionType.Deposit && transaction.Origin == "DEPÓSITO")
                return "Depósito por Cajero Automático";

            if (transaction.Type == TransactionType.CashWithdrawal && transaction.Beneficiary == "RETIRO")
                return "Retiro por Cajero Automático";

            if (transaction.Type == TransactionType.Transfer)
            {
                bool isOwnTransfer = transaction.OriginAccountId != Guid.Empty
                                     && transaction.DestinationAccountId != Guid.Empty
                                     && transaction.OriginAccountId != transaction.DestinationAccountId
                                     && transaction.OriginAccountId != Guid.Empty
                                     && transaction.DestinationAccountId != Guid.Empty;

                if (visualType == "DÉBITO")
                    return $"Transferencia a {transaction.Beneficiary}";

                if (isOwnTransfer)
                    return $"Transferencia de cuenta propia ({transaction.Origin})";

                return $"Transferencia recibida de {transaction.Origin}";
            }

            return "Transacción";
        }

        //

        public async Task<ConfirmBeneficiaryTransferViewModel> PrepareBeneficiaryTransferConfirmationAsync(
            string originAccountNumber,
            string beneficiaryAccountNumber,
            decimal amount,
            Guid ownerUserId)
        {
            var originAccount = await savingsAccountRepository.GetByAccountNumberAsync(originAccountNumber);
            var beneficiary = await beneficiaryRepository.GetByAccountNumberAndOwnerAsync(ownerUserId, beneficiaryAccountNumber);

            if (originAccount == null || beneficiary == null)
                throw new InvalidOperationException("No se pudo preparar la confirmación. Datos inválidos.");

            return new ConfirmBeneficiaryTransferViewModel
            {
                OriginAccountId = originAccount.Id,
                OriginAccountNumber = originAccount.AccountNumber,
                BeneficiaryAccountNumber = beneficiary.BeneficiaryAccountNumber,
                BeneficiaryFullName = $"{beneficiary.Name} {beneficiary.LastName}",
                Amount = amount,
                Timestamp = DateTime.Now
            };
        }

        public async Task<bool> ExecuteBeneficiaryTransferAsync(ExecuteBeneficiaryTransferDto model)
        {
            var originAccount = await savingsAccountRepository.GetByAccountNumberAsync(model.OriginAccountNumber);
            var destinationAccount = await savingsAccountRepository.GetByAccountNumberAsync(model.BeneficiaryAccountNumber);

            if (originAccount == null || destinationAccount == null)
                return false;

            if (originAccount.Balance < model.Amount)
                return false;

            // Actualizar balances
            originAccount.Balance -= model.Amount;
            destinationAccount.Balance += model.Amount;

            await savingsAccountRepository.UpdateAsync(originAccount.Id, originAccount);
            await savingsAccountRepository.UpdateAsync(destinationAccount.Id, destinationAccount);

            var now = model.Timestamp;

            // ✅ Registro único de la transacción
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                OriginAccountId = originAccount.Id,
                DestinationAccountId = destinationAccount.Id,
                Amount = model.Amount,
                Date = now,
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Approved,
                Origin = originAccount.AccountNumber,
                Beneficiary = destinationAccount.AccountNumber
            };

            var saved = await transactionRepository.RegisterTransactionAsync(transaction);
            if (!saved)
                return false;

            // ✅ Envío de correos
            var sender = await userAccountService.GetUserById(originAccount.UserId.ToString());
            var receiver = await userAccountService.GetUserById(destinationAccount.UserId.ToString());

            if (sender != null && receiver != null)
            {
                var formattedAmount = model.Amount.ToString("C", new CultureInfo("es-DO"));
                var formattedDate = now.ToString("dd/MM/yyyy h:mm tt", new CultureInfo("es-DO"));

                var last4Dest = destinationAccount.AccountNumber[^4..];
                var last4Origin = originAccount.AccountNumber[^4..];

                // Correo para origen
                await emailService.SendAsync(new EmailRequestDto
                {
                    To = sender.Email,
                    Subject = $"Transacción realizada a la cuenta {last4Dest}",
                    HtmlBody = $@"
                <div style='font-family:Arial,sans-serif;color:#333'>
                    <h2 style='color:#2E86C1'>Transferencia Exitosa</h2>
                    <p>Se ha enviado <strong>{formattedAmount}</strong> a la cuenta destino <strong>{last4Dest}</strong>.</p>
                    <p>Fecha y hora: <strong>{formattedDate}</strong></p>
                    <p style='margin-top:20px'>Gracias por usar nuestro servicio.</p>
                </div>"
                });

                // Correo para destino
                await emailService.SendAsync(new EmailRequestDto
                {
                    To = receiver.Email,
                    Subject = $"Transacción enviada desde la cuenta {last4Origin}",
                    HtmlBody = $@"
                <div style='font-family:Arial,sans-serif;color:#333'>
                    <h2 style='color:#28B463'>Depósito Recibido</h2>
                    <p>Ha recibido un depósito de <strong>{formattedAmount}</strong> desde la cuenta <strong>{last4Origin}</strong>.</p>
                    <p>Fecha y hora: <strong>{formattedDate}</strong></p>
                    <p style='margin-top:20px'>Gracias por confiar en nosotros.</p>
                </div>"
                });
            }

            return true;
        }

        // Retorna todas las transacciones registradas en el sistema.
        // Se utiliza en el Dashboard para calcular indicadores globales.
        public async Task<List<TransactionDto>> GetAllTransactionsAsync()
        {
            // Consulta al repositorio para obtener todas las transacciones
            var transactions = await transactionRepository.GetAllTransactionsAsync();

            // Convierte las entidades Transaction a DTOs para exponerlos a la capa de presentación
            return mapper.Map<List<TransactionDto>>(transactions);
        }

        // Retorna todas las transacciones de tipo "Pago" registradas en el sistema.
        // Se utiliza en el Dashboard para calcular la cantidad de pagos procesados.
        public async Task<List<TransactionDto>> GetAllPaymentsAsync()
        {
            // Consulta al repositorio filtrando por tipo de transacción "Pago"
            var payments = await transactionRepository.GetByTypeAsync(TransactionType.Payment);

            // Convierte las entidades Transaction a DTOs
            return mapper.Map<List<TransactionDto>>(payments);
        }


        //home cajero--------------------------------------------------

        //transaciones
        public async Task<int> GetTransactionsByCashierAndDateAsync(Guid userId, DateTime date)
        {
            var user = await userAccountService.GetUserById(userId.ToString());
            if (user == null) return 0;

            return await transactionRepository.GetTransactionsByCashierAndDateAsync(userId, date);
        }

        //pagos
        public async Task<int> GetPaymentsCountByCashierAndDateAsync(Guid userId, DateTime date)
        {
            var user = await userAccountService.GetUserById(userId.ToString());
            if (user == null) return 0;

            return await transactionRepository.GetPaymentsCountByCashierAndDateAsync(userId, date);
        }

        //depositos
        public async Task<int> GetDepositsCountByCashierAndDateAsync(Guid userId, DateTime date)
        {
            var user = await userAccountService.GetUserById(userId.ToString());
            if (user == null) return 0;

            return await transactionRepository.GetDepositsCountByCashierAndDateAsync(userId, date);
        }

        //retiros
        public async Task<int> GetWithdrawalsCountByCashierAndDateAsync(Guid userId, DateTime date)
        {
            var user = await userAccountService.GetUserById(userId.ToString());
            if (user == null) return 0;

            return await transactionRepository.GetWithdrawalsCountByCashierAndDateAsync(userId, date);
        }

        public async Task<(int TotalPayments, int TodayPayments)> GetLoanAndCreditCardPaymentsAsync() 
        {
            return await transactionRepository.GetLoanAndCreditCardPaymentsAsync(); 
        }

    }
}
