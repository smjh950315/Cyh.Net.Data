using System.Collections;

namespace Cyh.Net.Data.Pager
{
    public interface IPage : IEnumerable
    {
        int PageIndex { get; }
        IEnumerable Items { get; }
    }
    public interface IPage<T> : IPage, IEnumerable<T>
    {
        new IEnumerable<T> Items { get; }
    }
}
