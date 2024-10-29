using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Lab1App.Models;

namespace Lab1App.Data
{
    public class AppDBContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = "Server=tcp:lab1krok1server.database.windows.net,1433;Initial Catalog=Lab1DB;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            var credential = new InteractiveBrowserCredential();

            var sqlConnection = new SqlConnection(connectionString);
            sqlConnection.AccessToken = credential.GetToken(
                new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" })
            ).Token;

            optionsBuilder.UseSqlServer(sqlConnection);
        }
    }
}
