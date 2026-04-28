
using Application.Behaviors;
using Application.Interfaces;
using Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application
{
    public static class ServicesRegistration
    {
        //Extension method - Decorator pattern
        public static void ApplicationLayerIoc(this IServiceCollection services)
        {
            //configurations
            services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());
            services.AddMediatR(opt => opt.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            //services ioc

            services.AddScoped<IBeneficiaryService, BeneficiaryService>();
            services.AddScoped<ICreditCardService, CreditCardService>();
            services.AddScoped<ICreditCardTransactionService, CreditCardTransactionService>();
            services.AddScoped<ICashAdvanceService, CashAdvanceService>();
            services.AddScoped<ILoanService, LoanService>();
            services.AddScoped<ILoanPaymentService, LoanPaymentService>();
            services.AddScoped<ILoanInstallmentService, LoanInstallmentService>();
            services.AddScoped<ISavingsAccountService, SavingsAccountService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IInternalTransferService, InternalTransferService>();

        }

    }
}
