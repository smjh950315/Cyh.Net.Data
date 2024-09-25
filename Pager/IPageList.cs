namespace Cyh.Net.Data.Pager
{
    public interface IPageList : IEnumerable<IPage>
    {
        int PageCount { get; }
        int PageSize { get; }
        int TotalCount { get; }
        IEnumerable<IPage> Pages { get; }
    }
    public interface IPageList<T> : IPageList, IEnumerable<IPage<T>>
    {
        new IEnumerable<IPage<T>> Pages { get; }
    }
}
