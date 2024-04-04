namespace Cyh.Net.Data
{
    public interface IDTOHelperActivator
    {
        IMyDataSource<T> GetDataSource<T>();
    }
}
