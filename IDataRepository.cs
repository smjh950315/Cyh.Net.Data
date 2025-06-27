using Cyh.Net.Reflection;
using System.Reflection;

namespace Cyh.Net.Data
{
    public interface IDataRepository
    {
        IDataRepository<T>? GetRepository<T>() where T : class;
        int SaveChanges();
    }
    public interface IDataRepository<T> : IDataRepository where T : class
    {
        IQueryable<T> Queryable { get; }
        void Add(T entity);
        void Update(T entity);
        void Remove(T entity);
        void AddRange(IEnumerable<T> entity);
        void UpdateRange(IEnumerable<T> entity);
        void RemoveRange(IEnumerable<T> entity);
    }

    public static class DataRepositoryExtensions
    {
        static Dictionary<Type, MethodInfo> _repositoryFactories = new Dictionary<Type, MethodInfo>();
        public static object? GetRepository(this IDataRepository repository, Type dataType)
        {
            if (_repositoryFactories.TryGetValue(dataType, out MethodInfo? factory))
            {
                return factory.Invoke(repository, null);
            }
            MethodInfo? method = typeof(IDataRepository).GetMethod(nameof(GetRepository));
            MethodInfo? genericMethod = method?.MakeGenericMethod(dataType);
            if (genericMethod == null) return null;
            _repositoryFactories.Add(dataType, genericMethod);
            return genericMethod.Invoke(repository, null);
        }
        public static void Add(this IDataRepository repository, object entity)
        {
            if (entity == null) return;
            repository.GetRepository(entity.GetType())?.CallMethod("Add", [entity]);
        }
        public static void Update(this IDataRepository repository, object entity)
        {
            if (entity == null) return;
            repository.GetRepository(entity.GetType())?.CallMethod("Update", [entity]);
        }
        public static void Remove(this IDataRepository repository, object entity)
        {
            if (entity == null) return;
            repository.GetRepository(entity.GetType())?.CallMethod("Remove", [entity]);
        }
        public static void AddRange<T>(this IDataRepository repository, IEnumerable<T> entities)
        {
            if (entities.IsNullOrEmpty()) return;
            repository.GetRepository(typeof(T))?.CallMethod("AddRange", [entities]);
        }
        public static void UpdateRange<T>(this IDataRepository repository, IEnumerable<T> entities)
        {
            if (entities.IsNullOrEmpty()) return;
            repository.GetRepository(typeof(T))?.CallMethod("UpdateRange", [entities]);
        }
        public static void RemoveRange<T>(this IDataRepository repository, IEnumerable<T> entities)
        {
            if (entities.IsNullOrEmpty()) return;
            repository.GetRepository(typeof(T))?.CallMethod("RemoveRange", [entities]);
        }
    }
}
