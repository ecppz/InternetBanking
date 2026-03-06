using Application.Dtos.CreditCard;
using Application.Dtos.Email;
using Application.Dtos.User;
using Application.Interfaces;
using AutoMapper;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class CreditCardService : GenericService<CreditCard, CreditCardDto>, ICreditCardService
    {
        private readonly ICreditCardRepository creditCardRepository;
        private readonly IMapper mapper;

        public CreditCardService(ICreditCardRepository creditCardRepository, IMapper mapper)
            : base(creditCardRepository, mapper)
        {
            this.creditCardRepository = creditCardRepository;
            this.mapper = mapper;
        }

        public async Task<CreditCardResponseDto> AssignCardAsync(AssignCreditCardDto dto)
        {
            var hasActive = await creditCardRepository.HasActiveCardAsync(dto.UserId);
            if (hasActive)
            {
                return new CreditCardResponseDto
                {
                    Success = false,
                    Message = "El cliente ya tiene una tarjeta activa."
                };
            }

            var cardNumber = GenerateUniqueCardNumber();
            var cvcHash = GenerateCvcHash();
            var expirationDate = DateTime.UtcNow.AddYears(3);

            var card = new CreditCard
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                CreditLimit = dto.CreditLimit,
                CurrentDebt = 0,
                CardNumber = cardNumber,
                CvcHash = cvcHash,
                ExpirationDate = expirationDate,
                Status = CreditCardStatus.Active,
                AdminUserId = dto.AdminUserId,
                CreatedAt = DateTime.UtcNow,
            };

            await creditCardRepository.AddAsync(card);

            return new CreditCardResponseDto
            {
                Success = true,
                Message = "Tarjeta asignada correctamente.",
                CardId = card.Id,
                CardNumber = cardNumber[^4..],
                CreditLimit = card.CreditLimit,
                CurrentDebt = card.CurrentDebt,
                ExpirationDate = card.ExpirationDate
            };
        }

        public async Task<bool> CancelCardAsync(CancelCreditCardDto dto)
        {
            var card = await creditCardRepository.GetById(dto.CardId);
            if (card == null)
            {
                return false;
            }

            if (card.CurrentDebt > 0)
            {
                return false;
            }

            card.Status = CreditCardStatus.Cancelled;
            await creditCardRepository.UpdateAsync(card.Id, card);

            return true;
        }

        public async Task<int> ExpireCardsAsync()
        {
            return await creditCardRepository.ExpireCardsAsync();
        }

        public async Task<List<CreditCardDto>> GetActiveCardsAsync()
        {
            var cards = await creditCardRepository.GetActiveCardsAsync();
            return mapper.Map<List<CreditCardDto>>(cards);
        }

        public async Task<List<CreditCardDto>> GetActiveCardsByUserIdAsync(Guid userId)
        {
            var cards = await creditCardRepository.GetActiveCardsByUserIdAsync(userId);
            return mapper.Map<List<CreditCardDto>>(cards);
        }


        public async Task<List<CreditCardDto>> GetCancelledCardsAsync()
        {
            var cards = await creditCardRepository.GetCancelledCardsAsync();
            return mapper.Map<List<CreditCardDto>>(cards);
        }

        public async Task<List<EligibleCustomerForCreditCardDto>> GetEligibleCustomersForCreditCard(List<UserDto> customers)
        {
            var eligible = new List<EligibleCustomerForCreditCardDto>();

            foreach (var user in customers)
            {
                var hasCard = await creditCardRepository.HasActiveCardAsync(Guid.Parse(user.Id));
                if (hasCard) continue;

                var debt = await creditCardRepository.GetTotalDebtByUserAsync(Guid.Parse(user.Id));

                eligible.Add(new EligibleCustomerForCreditCardDto
                {
                    UserId = Guid.Parse(user.Id),
                    DocumentNumber = user.DocumentNumber,
                    Name = user.Name,
                    LastName = user.LastName,
                    Email = user.Email,
                    CurrentDebt = debt
                });
            }

            return eligible.OrderByDescending(e => e.Name).ToList();
        }


        public async Task<List<CreditCardDisplayDto>> GetAllDisplayAsync(List<UserDto> users, string? documentNumber, string? statusFilter)
        {
            var cards = await creditCardRepository.GetAllQuery().ToListAsync();
            var userIds = cards.Select(c => c.UserId).Distinct().ToList();

            if (!string.IsNullOrEmpty(documentNumber))
            {
                var matchingUserIds = users
                    .Where(u => u.DocumentNumber.Contains(documentNumber))
                    .Select(u => Guid.Parse(u.Id))
                    .ToList();

                cards = cards.Where(c => matchingUserIds.Contains(c.UserId)).ToList();
            }

            var result = new List<CreditCardDisplayDto>();

            foreach (var card in cards)
            {
                var user = users.FirstOrDefault(u => Guid.Parse(u.Id) == card.UserId);

                var status = card.Status;

                if (!string.IsNullOrEmpty(statusFilter) && status.ToString() != statusFilter)
                {
                    continue;
                }

                result.Add(new CreditCardDisplayDto
                {
                    Id = card.Id,
                    CardNumber = card.CardNumber,
                    CustomerFullName = user != null ? $"{user.Name} {user.LastName}" : "",
                    DocumentNumber = user?.DocumentNumber ?? "",
                    CreditLimit = card.CreditLimit,
                    CurrentDebt = card.CurrentDebt,
                    Status = status,
                    ExpirationDate = card.ExpirationDate,
                    CreatedAt = card.CreatedAt
                });
            }

            return result
                .OrderByDescending(c => c.Status == CreditCardStatus.Active)
                .ThenByDescending(c => c.CreatedAt)
                .ToList();
        }

        public async Task<CreditCardDetailsDto> GetCardDetailsAsync(Guid cardId)
        {
            var card = await creditCardRepository.GetById(cardId);
            if (card == null)
            {
                return null;
            }

            var dto = mapper.Map<CreditCardDetailsDto>(card);
            return dto;
        }

        public async Task<Guid?> GetCardIdByNumberAsync(string cardNumber)
        {
            var card = await creditCardRepository.GetByNumberAsync(cardNumber);
            return card?.Id;
        }

        public async Task<bool> UpdateCreditLimitAsync(EditCreditCardDto dto, UserDto user)
        {
            var card = await creditCardRepository.GetById(dto.CardId);
            if (card == null)
            {
                return false;
            }


            if (dto.NewLimit < card.CurrentDebt)
            {
                return false;
            }

            card.CreditLimit = dto.NewLimit;
            await creditCardRepository.UpdateAsync(card.Id, card);

            return true;
        }

        public async Task<decimal> GetAverageDebtAsync()
        {
            var totalDebt = await creditCardRepository.GetTotalDebtAsync();
            var count = await creditCardRepository.GetCardCountAsync();

            return count == 0 ? 0 : totalDebt / count;
        }


        #region private methods

        private string GenerateUniqueCardNumber()
        {
            var random = new Random();
            var number = string.Concat(Enumerable.Range(0, 16).Select(_ => random.Next(0, 10).ToString()));
            return number;
        }

        private string GenerateCvcHash()
        {
            var random = new Random();
            var cvc = random.Next(100, 999).ToString();

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(cvc);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public async Task<int> GetActiveCreditCardsCountAsync()
        {
            return await creditCardRepository.GetActiveCreditCardsCountAsync();
        }

        #endregion


    }
}
