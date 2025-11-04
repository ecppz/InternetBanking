using Application.Interfaces;
using Domain.Settings;
using Infrastructure.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Shared
{
    public static class ServicesRegistration
    {
        public static void SharedLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            // Configurations
            services.Configure<MailSettings>(config.GetSection("MailSettings"));

            // Services IOC
            services.AddScoped<IEmailService, EmailService>();
        }
    }
}
