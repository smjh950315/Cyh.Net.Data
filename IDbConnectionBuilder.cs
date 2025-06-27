using System.Data;

namespace Cyh.Net.Data
{
    public interface IDbConnectionBuilder
    {
        IDbConnection CreateConnection();
    }
}
