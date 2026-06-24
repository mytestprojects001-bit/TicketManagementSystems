using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TicketManagementSystem.Infrastructure
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }

    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;
        public DbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection CreateConnection()
        {
            var connStr = _configuration.GetConnectionString("DefaultConnection");
            var conn = new SqlConnection(connStr);
            conn.Open();
            return conn;
        }
    }
}
