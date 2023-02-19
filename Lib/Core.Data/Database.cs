using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Core.Data;

public abstract class Database : IDatabase, IDisposable
{
    private bool disposedValue;


    protected Database(string name, string dbName, string connString)
    {
        Name = name;
        DbName = dbName;
        ConnectionString = connString;
    }


    protected DbTransaction CurrentTransaction { get; private set; }
    protected DbConnection Connection { get; private set; }
    private string ConnectionString { get; }
    public string Name { get; }
    public string DbName { get; }


    public async Task OpenConnection()
    {
        Connection = GetDbConnection(ConnectionString);
        await Connection.OpenAsync();
    }

    public void CloseConnection()
    {
        if (Connection == null)
        {
            throw new InvalidOperationException("Can not close a connection that does not exist.");
        }

        if (Connection.State != ConnectionState.Open)
        {
            throw new InvalidOperationException("Can not close a connection that is not open.");
        }

        Connection.Close();
        Connection = null;
    }

    public async Task StartTransaction(IsolationLevel il = IsolationLevel.Unspecified)
    {
        await OpenConnectionIfNeeded();
        CurrentTransaction = Connection.BeginTransaction(il);
    }

    public void CommitTransaction()
    {
        if (CurrentTransaction == null)
        {
            throw new InvalidOperationException("Can not commit a transaction that has not been started.");
        }

        CurrentTransaction.Commit();
        CurrentTransaction = null;
    }

    public void RollbackTransaction()
    {
        if (CurrentTransaction == null)
        {
            throw new InvalidOperationException("Can not commit a transaction that has not been started.");
        }

        CurrentTransaction.Rollback();
        CurrentTransaction = null;
    }

    public async Task<IEnumerable<T>> ExecuteQuery<T>(string sql, IEnumerable<IDataParameter> parameters = null)
        where T : IRowView
    {
        var records = await ExecuteQuery(sql, parameters);

        return records.Select(MagicManager.GetActiveRecord<T>).ToList();
    }

    public async Task<IEnumerable<IDataRecord>> ExecuteQuery(string sql, IEnumerable<IDataParameter> parameters = null)
    {
        var cmd = await BuildCommand(sql, parameters);
        var rdr = await cmd.ExecuteReaderAsync();
        var ret = new List<IDataRecord>();

        while (await rdr.ReadAsync())
        {
            ret.Add(rdr);
        }

        return ret;
    }

    public async Task<int> ExecuteNonQuery(string sql, IEnumerable<IDataParameter> parameters = null)
    {
        var cmd = await BuildCommand(sql, parameters);

        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<T> ExecuteScalar<T>(string sql, IEnumerable<IDataParameter> parameters = null)
    {
        var cmd = await BuildCommand(sql, parameters);
        var ret = await cmd.ExecuteScalarAsync();

        return ret == DBNull.Value ? default : (T)ret;
    }


    public string DelimDatabase(string val) { return Delim(val, DelimType.Database); }
    public string DelimSchema(string val) { return Delim(val, DelimType.Schema); }
    public string DelimTable(string val) { return Delim(val, DelimType.Table); }
    public string DelimColumn(string val) { return Delim(val, DelimType.Column); }
    public string DelimParameter(string val) { return Delim(val, DelimType.Parameter); }
    public string DelimString(string val) { return Delim(val, DelimType.String); }
    public abstract string Delim(string val, DelimType dt);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected async Task OpenConnectionIfNeeded()
    {
        if (Connection == null || Connection.State == ConnectionState.Closed)
        {
            await OpenConnection();
        }
    }

    protected abstract DbConnection GetDbConnection(string cs);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected async Task<DbCommand> BuildCommand(string sql, IEnumerable<IDataParameter> parameters = null)
    {
        await OpenConnectionIfNeeded();

        var cmd = Connection.CreateCommand();
        cmd.Transaction = CurrentTransaction;

        foreach (var p in parameters)
        {
            cmd.Parameters.Add(p);
        }

        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;

        return cmd;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposedValue)
        {
            return;
        }

        if (disposing)
        {
            if (CurrentTransaction != null)
            {
                CurrentTransaction.Dispose();
                CurrentTransaction = null;
            }

            if (Connection != null)
            {
                if (Connection.State != ConnectionState.Closed)
                {
                    Connection.Close();
                }

                Connection.Dispose();
                Connection = null;
            }
        }

        disposedValue = true;
    }
}