namespace Cyh.Net.Data
{
    public interface IDataSource
    {
        IQueryable<T> GetQueryable<T>() where T : class;
        IDataWriter<T> GetWriter<T>() where T : class;
        bool Save();
    }
}
