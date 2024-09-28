using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Cyh.Net.Data.Predicate
{
    public class Predicate
    {
        class MetaExpression<T>
        {
            public static ParameterExpression ParameterExpression;
            static MetaExpression()
            {
                ParameterExpression = Expression.Parameter(typeof(T));
            }

            public ConstantExpression? ConstantExpression { get; set; }
            public MemberExpression? MemberExpression { get; set; }
            public Expression? BodyExpression { get; set; }
            public MetaExpression() { }

            [MemberNotNullWhen(true, nameof(ConstantExpression))]
            [MemberNotNullWhen(true, nameof(MemberExpression))]
            public bool IsReady => this.MemberExpression != null && this.ConstantExpression != null;
            public void BindMember(PropertyInfo memberProperty)
            {
                this.MemberExpression = Expression.Property(ParameterExpression, memberProperty);
            }
            public void BindMember(string memberName)
            {
                this.MemberExpression = Expression.PropertyOrField(ParameterExpression, memberName);
            }
            public void BindConstant(object? constantValue)
            {
                this.ConstantExpression = Expression.Constant(constantValue);
            }

            public Expression<Func<T, bool>> GetPredicate(CompareType compareType)
            {
                if (this.IsReady)
                {
                    switch (compareType)
                    {
                        case CompareType.Equal:
                            this.BodyExpression = Expression.Equal(this.MemberExpression, this.ConstantExpression);
                            break;
                        case CompareType.NotEqual:
                            this.BodyExpression = Expression.NotEqual(this.MemberExpression, this.ConstantExpression);
                            break;
                        case CompareType.GreaterThan:
                            this.BodyExpression = Expression.GreaterThan(this.MemberExpression, this.ConstantExpression);
                            break;
                        case CompareType.GreaterThanOrEqual:
                            this.BodyExpression = Expression.GreaterThanOrEqual(this.MemberExpression, this.ConstantExpression);
                            break;
                        case CompareType.LessThan:
                            this.BodyExpression = Expression.LessThan(this.MemberExpression, this.ConstantExpression);
                            break;
                        case CompareType.LessThanOrEqual:
                            this.BodyExpression = Expression.LessThanOrEqual(this.MemberExpression, this.ConstantExpression);
                            break;
                        case CompareType.Contains:
                        {
                            Type memberType = this.MemberExpression.Type;
                            Type constantType = this.ConstantExpression.Type;
                            Type constantEnumerableType = GenericEnumerable.MakeGenericType(constantType);
                            if (memberType == typeof(string))
                            {
                                this.BodyExpression = Expression.Call(this.MemberExpression, Predicate.StringContainsMethod, this.ConstantExpression);
                            }
                            else if (memberType.IsAssignableTo(constantEnumerableType))
                            {
                                this.BodyExpression = Expression.Call(null, Predicate.MakeGenericContains(constantType), this.MemberExpression, this.ConstantExpression);
                            }
                            else
                            {
                                throw new NotSupportedException("Invalid type expression");
                            }
                            break;
                        }
                        case CompareType.IsAnyOf:
                        {
                            Type memberType = this.MemberExpression.Type;
                            this.BodyExpression = Expression.Call(null, Predicate.MakeGenericContains(memberType), this.ConstantExpression, this.MemberExpression);
                            break;
                        }
                        default:
                            throw new InvalidOperationException();
                    }
                    return Expression.Lambda<Func<T, bool>>(this.BodyExpression, ParameterExpression);
                }
                throw new InvalidOperationException();
            }
        }

        class ExpressionHolder<T>
        {
            public LinkType LinkType { get; set; }
            public Expression<Func<T, bool>> Expression { get; set; }
            public ExpressionHolder(LinkType linkType, Expression<Func<T, bool>> predicateExpression)
            {
                this.LinkType = linkType;
                this.Expression = predicateExpression;
            }
        }

        static Expression<Func<T, bool>> Combine<T>(Expression<Func<T, bool>> begin, IEnumerable<ExpressionHolder<T>>? expressionHolders)
        {
            if (expressionHolders == null || expressionHolders.Count() == 0)
            {
                return begin;
            }
            Expression<Func<T, bool>> result = begin;
            foreach (var predicateData in expressionHolders)
            {
                bool _and;
                if (predicateData.LinkType == LinkType.And)
                {
                    _and = true;
                }
                else if (predicateData.LinkType == LinkType.Or)
                {
                    _and = false;
                }
                else
                {
                    throw new InvalidDataException(nameof(predicateData.LinkType));
                }

                var predicate = predicateData.Expression;

                if (predicate != null)
                {
                    result = result.UpdateExpression(predicate, _and);
                }
            }
            return result;
        }

        class PredicateHolder<T> : IPredicateHolder<T>
        {
            List<ExpressionHolder<T>>? m_predicate_expressions;
            readonly Expression<Func<T, bool>> m_predicate;

            public PredicateHolder(Expression<Func<T, bool>> predicate)
            {
                this.m_predicate = predicate;
            }

            public IPredicateHolder<T> And(Expression<Func<T, bool>> other)
            {
                this.m_predicate_expressions ??= new();
                this.m_predicate_expressions.Add(new ExpressionHolder<T>(LinkType.And, other));
                return this;
            }

            public IPredicateHolder<T> Or(Expression<Func<T, bool>> other)
            {
                this.m_predicate_expressions ??= new();
                this.m_predicate_expressions.Add(new ExpressionHolder<T>(LinkType.Or, other));
                return this;
            }

            public Expression<Func<T, bool>> GetPredicate()
            {
                return Combine(this.m_predicate, this.m_predicate_expressions);
            }
        }

        class LegacyBindingPredicate<T> : ILegacyBindingPredicate<T>
        {
            readonly MetaExpression<T> m_expression_data;

            public CompareType CompareType { get; set; }
            public LegacyBindingPredicate(CompareType compareType)
            {
                this.CompareType = compareType;
                this.m_expression_data = new();
            }

            public ILegacyBindingPredicate<T> BindMember(PropertyInfo memberProperty)
            {
                this.m_expression_data.BindMember(memberProperty);
                return this;
            }
            public ILegacyBindingPredicate<T> BindMember(string memberName)
            {
                this.m_expression_data.BindMember(memberName);
                return this;
            }
            public ILegacyBindingPredicate<T> BindConstant(object? constantValue)
            {
                this.m_expression_data.BindConstant(constantValue);
                return this;
            }

            public Expression<Func<T, bool>>? GetPredicate()
            {
                if (this.m_expression_data.IsReady)
                {
                    return this.m_expression_data.GetPredicate(this.CompareType);
                }
                else
                {
                    return null;
                }
            }

            public Expression<Func<T, bool>> GetPredicate(PropertyInfo memberProperty, object? constantValue)
            {
                var metaPredicate = new MetaExpression<T>();
                metaPredicate.BindMember(memberProperty);
                metaPredicate.BindConstant(constantValue);
                return metaPredicate.GetPredicate(this.CompareType)!;
            }

            public Expression<Func<T, bool>> GetPredicate(string memberName, object? constantValue)
            {
                var metaPredicate = new MetaExpression<T>();
                metaPredicate.BindMember(memberName);
                metaPredicate.BindConstant(constantValue);
                return metaPredicate.GetPredicate(this.CompareType)!;
            }
        }


        static Dictionary<Type, MethodInfo> GenericContains;
        static MethodInfo EnumerableGenericContainsMethod;
        static MethodInfo StringContainsMethod;
        static Type GenericEnumerable;
        static MethodInfo MakeGenericContains(Type _type)
        {
            Type type = _type.RemoveNullable();
            MethodInfo? genericContains = null;
            if (!GenericContains.TryGetValue(type, out genericContains))
            {
                genericContains = EnumerableGenericContainsMethod.MakeGenericMethod(type);
                GenericContains.Add(type, genericContains);
            }
            return genericContains;
        }
        static MethodInfo MakeGenericContains<T>()
        {
            Type type = typeof(T).RemoveNullable();
            MethodInfo? baseContains = null;
            if (!GenericContains.TryGetValue(type, out baseContains))
            {
                baseContains = EnumerableGenericContainsMethod.MakeGenericMethod(type);
                GenericContains.Add(type, baseContains);
            }
            return baseContains.MakeGenericMethod(type);
        }
        static Predicate()
        {
            GenericContains = new();
            IEnumerable<MethodInfo> methodInfos = typeof(Enumerable).GetMethods().Where(m => m.Name == "Contains");
            EnumerableGenericContainsMethod = methodInfos.FirstOrDefault(x => x.GetParameters().Length == 2)!;
            StringContainsMethod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) })!;
            GenericEnumerable = typeof(IEnumerable<>);
        }

        private static object? GetPropertyValue(object? src, string name)
        {
            if (src == null) return null;
            var property = src.GetType().GetProperty(name);
            try
            {
                return property?.GetValue(src);
            }
            catch
            {
                return null;
            }
        }
        public static IPredicateHolder<T> NewPredicateHolder<T>(Expression<Func<T, bool>> predicate)
        {
            return new PredicateHolder<T>(predicate);
        }
        public static ILegacyBindingPredicate<T> NewLegacyPredicate<T>(CompareType compareType)
        {
            return new LegacyBindingPredicate<T>(compareType);
        }

        public static Expression<Func<T, bool>>? GetExpression<T>(string memberName, CompareType compareType, object? constantValue)
        {
            return new LegacyBindingPredicate<T>(compareType).GetPredicate(memberName, constantValue);
        }
        public static Expression<Func<T, bool>>? GetExpression<T, TKey>(Expression<Func<T, TKey>> getKey, CompareType compareType, object? constantValue)
        {
            string? memberName = GetPropertyValue(GetPropertyValue(getKey.Body, "Member"), "Name")?.ToString();
            if (string.IsNullOrEmpty(memberName)) throw new InvalidOperationException();
            return GetExpression<T>(memberName, compareType, constantValue);
        }

        public static Expression<Func<T, bool>>? GetExpression<T>(IEnumerable<ParameterData> parameterDatas)
        {
            if (parameterDatas == null) return null;
            var iterator = parameterDatas.GetEnumerator();

            PredicateHolder<T>? predicateHolder = null;

            while (iterator.MoveNext())
            {
                var predicate = GetExpression<T>(iterator.Current.MemberName, iterator.Current.CompareType, iterator.Current.ConstantValue);
                if (predicate == null) continue;
                if (predicateHolder == null)
                {
                    predicateHolder = new(predicate);
                }
                else
                {
                    if (iterator.Current.LinkType == LinkType.And)
                    {
                        predicateHolder.And(predicate);
                    }
                    else if (iterator.Current.LinkType == LinkType.Or)
                    {
                        predicateHolder.Or(predicate);
                    }
                }
            }

            return predicateHolder?.GetPredicate();
        }
    }
}
