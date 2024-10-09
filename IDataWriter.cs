namespace Cyh.Net.Data
{
    public interface IDataWriter<T> where T : class
    {
        void Add(T item);
        void AddRange(IEnumerable<T> items);
        void Remove(T item);
        void Update(T item);
    }
}
