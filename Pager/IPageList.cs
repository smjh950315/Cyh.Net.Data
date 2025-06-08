namespace Cyh.Net.Data.Pager
{
    public interface IPageList : IEnumerable<IPage>
    {
        /// <summary>
        /// Item count per page
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// Total item count
        /// </summary>
        int Total { get; }

        /// <summary>
        /// Page count
        /// </summary>
        /// <returns>Count of pages</returns>
        int Count { get; }

        /// <summary>
        /// Pages
        /// </summary>
        IEnumerable<IPage> Pages { get; }
    }
    public interface IPageList<T> : IPageList, IEnumerable<IPage<T>>
    {
        /// <summary>
        /// Pages
        /// </summary>
        new IEnumerable<IPage<T>> Pages { get; }

        /// <summary>
        /// Get page by index
        /// </summary>
        /// <param name="index">Zero based index</param>
        /// <returns>Page of the index</returns>
        IPage<T> this[int index] { get; }
    }
}
