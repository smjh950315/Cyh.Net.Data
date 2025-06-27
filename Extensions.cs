using System.Data;
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
        public static T? FirstOrDefault<T>(this IDataRepository<T> repository, Expression<Func<T, bool>>? predicate) where T : class
        {
            if (predicate == null)
                return repository.Queryable.FirstOrDefault();
            return repository.Queryable.FirstOrDefault(predicate);
        }
        public static IQueryable<V> Select<T, V>(this IDataRepository<T> dataRepository, Expression<Func<T, V>> selector) where T : class
        {
            return dataRepository.Queryable.Select(selector);
        }

        public static IScopedDbConnectionBuilder GetScopedDbConnectionBuilder(Func<IDbConnection> connectionFactory, bool showConnectionTrack = false)
        {
            return new ScopedDbConnectionBuilder(connectionFactory, showConnectionTrack);
        }

    }
}
