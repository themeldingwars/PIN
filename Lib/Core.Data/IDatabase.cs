using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Core.Data
{
    public enum DelimType
    {
        None = 0,
        Database = 1,
        Schema = 2,
        Table = 3,
        Column = 4,
        Parameter = 5,
        String = 6
    }

    public interface IDatabase
    {
        string Name { get; }
        string DbName { get; }

        Task OpenConnection();
        void CloseConnection();

        Task StartTransaction(IsolationLevel il = IsolationLevel.Unspecified);
        void CommitTransaction();
        void RollbackTransaction();

        Task<IEnumerable<T>> ExecuteQuery<T>(string sql, IEnumerable<IDataParameter> parameters = null)
            where T : IRowView;

        Task<IEnumerable<IDataRecord>> ExecuteQuery(string sql, IEnumerable<IDataParameter> parameters = null);
        Task<int> ExecuteNonQuery(string sql, IEnumerable<IDataParameter> parameters = null);
        Task<T> ExecuteScalar<T>(string sql, IEnumerable<IDataParameter> parameters = null);

        string Delim(string val, DelimType dt);
        string DelimDatabase(string val);
        string DelimSchema(string val);
        string DelimTable(string val);
        string DelimColumn(string val);
        string DelimParameter(string val);
        string DelimString(string val);
    }
}