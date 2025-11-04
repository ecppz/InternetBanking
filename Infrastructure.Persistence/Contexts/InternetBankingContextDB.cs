using Microsoft.EntityFrameworkCore;
using System.Reflection;


namespace Infrastructure.Persistence.Contexts
{
    public class InternetBankingContextDB : DbContext 
    {
        public InternetBankingContextDB(DbContextOptions<InternetBankingContextDB> options) : base(options) { }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); //Liskov-substitution

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
