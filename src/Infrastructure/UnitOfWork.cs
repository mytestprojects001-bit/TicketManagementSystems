using System;
using System.Data;
using System.Data.SqlClient;

namespace TicketManagementSystem.Infrastructure
{
    public interface IUnitOfWork : IDisposable
    {
        SqlConnection Connection { get; }
        SqlTransaction Transaction { get; }
        void BeginTransaction();
        void Commit();
        void Rollback();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private bool _disposed;
        public SqlConnection Connection { get; private set; }
        public SqlTransaction Transaction { get; private set; }

        public UnitOfWork(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
            Connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
        }

        public void BeginTransaction()
        {
            if (Transaction == null)
                Transaction = Connection.BeginTransaction(IsolationLevel.Serializable);
        }

        public void Commit()
        {
            Transaction?.Commit();
            Transaction = null;
        }

        public void Rollback()
        {
            Transaction?.Rollback();
            Transaction = null;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Transaction?.Dispose();
                Connection?.Dispose();
                _disposed = true;
            }
        }
    }
}
