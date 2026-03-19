using System.Data;

namespace Cyh.Net.Data
{
    public interface IDbConnectionBuilder
    {
        IDbConnection CreateConnection();
    }
    internal class DbConnectionBuilder : IDbConnectionBuilder
    {
        Func<IDbConnection> _factory;
        internal DbConnectionBuilder(Func<IDbConnection> factory) { this._factory = factory; }
        public IDbConnection CreateConnection() => this._factory();
    }
}
