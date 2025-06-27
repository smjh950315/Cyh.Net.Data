using System.Data;

namespace Cyh.Net.Data
{
    public interface IScopedDbConnectionBuilder : IDisposable
    {
        IDbConnection GetConnection();
    }

    internal class DbCommandImpl : IDbCommand
    {
        bool _showCommands { get; set; }
        IDbCommand _command;
        public DbCommandImpl(IDbCommand command, bool _showCommands)
        {
            this._showCommands = _showCommands;
            this._command = command;
        }
        public string CommandText { get => this._command.CommandText; set => this._command.CommandText = value; }
        public int CommandTimeout { get => this._command.CommandTimeout; set => this._command.CommandTimeout = value; }
        public CommandType CommandType { get => this._command.CommandType; set => this._command.CommandType = value; }
        public IDbConnection? Connection { get => this._command.Connection; set => this._command.Connection = value; }
        public IDbTransaction? Transaction { get => this._command.Transaction; set => this._command.Transaction = value; }
        public UpdateRowSource UpdatedRowSource { get => this._command.UpdatedRowSource; set => this._command.UpdatedRowSource = value; }

        public IDataParameterCollection Parameters => this._command.Parameters;

        public void Cancel()
        {
            this._command.Cancel();
        }
        public IDbDataParameter CreateParameter()
        {
            return this._command.CreateParameter();
        }
        public int ExecuteNonQuery()
        {
            if (!this.CommandText.IsNullOrEmpty() && this._showCommands)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Executing Command: {this.CommandText}");
            }
            return this._command.ExecuteNonQuery();
        }
        public IDataReader ExecuteReader()
        {
            if (!this.CommandText.IsNullOrEmpty() && this._showCommands)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Executing Command: {this.CommandText}");
            }
            return this._command.ExecuteReader();
        }
        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            if (!this.CommandText.IsNullOrEmpty() && this._showCommands)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Executing Command: {this.CommandText}");
            }
            return this._command.ExecuteReader(behavior);
        }
        public object? ExecuteScalar()
        {
            if (!this.CommandText.IsNullOrEmpty() && this._showCommands)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Executing Command: {this.CommandText}");
            }
            return this._command.ExecuteScalar();
        }
        public void Prepare()
        {
            this._command.Prepare();
        }
        public void Dispose()
        {
            this._command.Dispose();
        }
    }
    internal class DbConnectionImpl : IDbConnection
    {
        bool _showCommands { get; set; }
        IDbConnection _connection;
        public DbConnectionImpl(IDbConnection connection, bool _showCommands)
        {
            this._connection = connection;
            this._showCommands = _showCommands;
        }

        public string ConnectionString { get => this._connection.ConnectionString; set => this._connection.ConnectionString = value; }

        public int ConnectionTimeout => this._connection.ConnectionTimeout;

        public string Database => this._connection.Database;

        public ConnectionState State => this._connection.State;

        public IDbTransaction BeginTransaction()
        {
            return this._connection.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return this._connection.BeginTransaction(il);
        }

        public void ChangeDatabase(string databaseName)
        {
            this._connection.ChangeDatabase(databaseName);
        }

        public void Close()
        {
            this._connection.Close();
        }

        public IDbCommand CreateCommand()
        {
            return new DbCommandImpl(this._connection.CreateCommand(), this._showCommands);
        }

        public void Dispose()
        {
            this._connection.Dispose();
        }

        public void Open()
        {
            this._connection.Open();
        }
    }

    public class ScopedDbConnectionBuilder : IScopedDbConnectionBuilder
    {
        bool _showConnectionTrack { get; set; }
        Guid _trackId { get; set; }
        Func<IDbConnection> _connectionFactory { get; set; }
        IDbConnection? _connection { get; set; }

        public ScopedDbConnectionBuilder(Func<IDbConnection> connectionFactory, bool _showConnectionTrack)
        {
            this._showConnectionTrack = _showConnectionTrack;
            this._trackId = Guid.Empty;
            this._connectionFactory = connectionFactory;
        }

        public IDbConnection GetConnection()
        {
            if (this._connection == null)
            {
                this._trackId = Guid.NewGuid();
                this._connection = new DbConnectionImpl(this._connectionFactory(), this._showConnectionTrack);
                if (this._showConnectionTrack)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Connection created with Track ID: {this._trackId}");
                }
            }
            if (this._connection.State != ConnectionState.Open)
            {
                this._connection.Open();
            }
            return this._connection;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._connection != null)
            {
                if (this._connection.State != ConnectionState.Closed)
                {
                    this._connection.Close();
                }
                this._connection.Dispose();
                if (this._showConnectionTrack)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Connection disposed with Track ID: {this._trackId}");
                }
            }
            this._connection = null;
        }

        ~ScopedDbConnectionBuilder()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            this.Dispose(disposing: false);
        }

        public void Dispose()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
