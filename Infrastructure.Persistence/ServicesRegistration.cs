using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repositories;

namespace Infrastructure.Persistence
{
    public static class ServicesRegistration
    {
        public static void PersistenceLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            //Contexts
            if (config.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<InternetBankingContextDB>(opt => opt.UseInMemoryDatabase("AppDb"));
            }
            else
            {
                var connectionString = config.GetConnectionString("DefaultConnection");
                services.AddDbContext<InternetBankingContextDB>(
                  (serviceProvider, opt) =>
                  {
                      opt.EnableSensitiveDataLogging();
                      opt.UseSqlServer(connectionString,
                      m => m.MigrationsAssembly(typeof(InternetBankingContextDB).Assembly.FullName));
                  },
                    contextLifetime: ServiceLifetime.Scoped,
                    optionsLifetime: ServiceLifetime.Scoped
                 );

                //Repositories IOC
                services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
                services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
                services.AddScoped<ICreditCardRepository, CreditCardRepository>();
                services.AddScoped<ICreditCardTransactionRepository, CreditCardTransactionRepository>();
                services.AddScoped<ILoanRepository, LoanRepository>();
                services.AddScoped<ILoanInstallmentRepository, LoanInstallmentRepository>();
                services.AddScoped<ISavingsAccountRepository, SavingsAccountRepository>();
                services.AddScoped<ITransactionRepository, TransactionRepository>();
            }
        }
    }
}
