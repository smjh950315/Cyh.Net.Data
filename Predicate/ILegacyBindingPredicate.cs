using System.Linq.Expressions;
using System.Reflection;

namespace Cyh.Net.Data.Predicate
{
    public interface ILegacyBindingPredicate<T>
    {
        CompareType CompareType { get; set; }
        Expression<Func<T, bool>>? GetPredicate();
        Expression<Func<T, bool>> GetPredicate(PropertyInfo memberProperty, object? constantValue);
        Expression<Func<T, bool>> GetPredicate(string memberName, object? constantValue);
        ILegacyBindingPredicate<T> BindMember(PropertyInfo memberProperty);
        ILegacyBindingPredicate<T> BindMember(string memberName);
        ILegacyBindingPredicate<T> BindConstant(object? constantValue);
    }
}
