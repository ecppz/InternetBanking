using Application;
using Application.Services;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Shared;
using Hangfire;

namespace InternetBankingApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Login/Index";
                options.AccessDeniedPath = "/Login/AccessDenied";
            });

            builder.Services.AddSession(opt =>
            {
                opt.IdleTimeout = TimeSpan.FromMinutes(60);
                opt.Cookie.HttpOnly = true;
            });

            builder.Services.PersistenceLayerIoc(builder.Configuration);
            builder.Services.ApplicationLayerIoc();
            builder.Services.AddIdentityLayerIocForWebApp(builder.Configuration);
            builder.Services.SharedLayerIoc(builder.Configuration);
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


            //hangfire
            builder.Services.AddHangfire(config =>
                config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddHangfireServer();


            builder.Services.AddScoped<LoanInstallmentStatusUpdater>();

            var app = builder.Build();

            await app.Services.RunIdentitySeedAsync();

            app.UseHangfireDashboard("/hangfire");

            RecurringJob.AddOrUpdate<LoanInstallmentStatusUpdater>("update-late-installments", updater => updater
                        .UpdateLateInstallmentsAsync(),Cron.Daily
            );

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Login}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
