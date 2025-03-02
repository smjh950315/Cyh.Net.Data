using System.Linq.Expressions;

namespace Cyh.Net.Data
{
    public static class Extensions
    {
        public static bool Any<T>(this IDataRepository<T> dataRepository, Expression<Func<T, bool>>? predicate) where T : class
        {
            return predicate != null ? dataRepository.Queryable.Any(predicate) : dataRepository.Queryable.Any();
        }
        public static IQueryable<T> Where<T>(this IDataRepository<T> dataRepository, Expression<Func<T, bool>>? predicate) where T : class
        {
            return predicate != null ? dataRepository.Queryable.Where(predicate) : dataRepository.Queryable;
        }
        public static IQueryable<V> Select<T, V>(this IDataRepository<T> dataRepository, Expression<Func<T, V>> selector) where T : class
        {
            return dataRepository.Queryable.Select(selector);
        }
    }
}
