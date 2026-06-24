using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace TicketManagementSystem.Infrastructure
{
    public interface IGenericRepository<T>
    {
        Task<IEnumerable<T>> QueryAsync(string sql, Func<SqlDataReader, T> map, params SqlParameter[] parameters);
        Task<int> ExecuteAsync(string sql, params SqlParameter[] parameters);
        Task<T> QuerySingleAsync(string sql, Func<SqlDataReader, T> map, params SqlParameter[] parameters);
    }

    public class GenericRepository : IGenericRepository<object>
    {
        private readonly UnitOfWork _unitOfWork;
        public GenericRepository(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> ExecuteAsync(string sql, params SqlParameter[] parameters)
        {
            using var cmd = _unitOfWork.Connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = System.Data.CommandType.Text;
            if (_unitOfWork.Transaction != null) cmd.Transaction = _unitOfWork.Transaction;
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<object>> QueryAsync(string sql, Func<SqlDataReader, object> map, params SqlParameter[] parameters)
        {
            var list = new List<object>();
            using var cmd = _unitOfWork.Connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = System.Data.CommandType.Text;
            if (_unitOfWork.Transaction != null) cmd.Transaction = _unitOfWork.Transaction;
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(map(reader));
            }
            return list;
        }

        public async Task<object> QuerySingleAsync(string sql, Func<SqlDataReader, object> map, params SqlParameter[] parameters)
        {
            using var cmd = _unitOfWork.Connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = System.Data.CommandType.Text;
            if (_unitOfWork.Transaction != null) cmd.Transaction = _unitOfWork.Transaction;
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) return map(reader);
            return default;
        }
    }
}
