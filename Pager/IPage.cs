using System.Collections;

namespace Cyh.Net.Data.Pager
{
    public interface IPage : IEnumerable
    {
        int Index { get; }
        IEnumerable Items { get; }
    }
    public interface IPage<T> : IPage, IEnumerable<T>
    {
        new IEnumerable<T> Items { get; }
    }
}
