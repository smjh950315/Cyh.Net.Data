using System.Linq.Expressions;

namespace Cyh.Net.Data.Predicate
{
    public interface IPredicateHolder<T>
    {
        IPredicateHolder<T> And(Expression<Func<T, bool>> other);
        IPredicateHolder<T> Or(Expression<Func<T, bool>> other);
        Expression<Func<T, bool>> GetPredicate();
    }
}
