
using Application.Interfaces;
using Application.Services;
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
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            //services IOC

        }

    }
}
