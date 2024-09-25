using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Cyh.Net.Data.Predicate
{
    public class Predicate
    {
        class PredicateHolder<T> : IPredicateHolder<T>
        {
            static Expression<Func<T, bool>> GetPredicate_t(PredicateHolder<T> impl)
            {
                if (impl.m_predicate_datas == null || impl.m_predicate_datas.Count == 0)
                {
                    return impl.m_predicate;
                }
                Expression<Func<T, bool>> result = impl.m_predicate;
                for (int i = 0; i < impl.m_predicate_datas.Count; ++i)
                {
                    PredicateData predicateData = impl.m_predicate_datas[i];
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

            class PredicateData
            {
                public LinkType LinkType { get; set; }
                public Expression<Func<T, bool>> Expression { get; set; }
                public PredicateData(LinkType linkType, Expression<Func<T, bool>> predicateExpression)
                {
                    this.LinkType = linkType;
                    this.Expression = predicateExpression;
                }
            }
            List<PredicateData>? m_predicate_datas;
            readonly Expression<Func<T, bool>> m_predicate;

            public PredicateHolder(Expression<Func<T, bool>> predicate)
            {
                this.m_predicate = predicate;
            }

            public IPredicateHolder<T> And(Expression<Func<T, bool>> other)
            {
                this.m_predicate_datas ??= new();
                this.m_predicate_datas.Add(new PredicateData(LinkType.And, other));
                return this;
            }

            public IPredicateHolder<T> Or(Expression<Func<T, bool>> other)
            {
                this.m_predicate_datas ??= new();
                this.m_predicate_datas.Add(new PredicateData(LinkType.Or, other));
                return this;
            }

            public Expression<Func<T, bool>> GetPredicate()
            {
                return GetPredicate_t(this);
            }
        }
        class LegacyBindingPredicate<T> : ILegacyBindingPredicate<T>
        {
            class ExpressionData
            {
                public static ParameterExpression ParameterExpression;
                static ExpressionData()
                {
                    ParameterExpression = Expression.Parameter(typeof(T));
                }
                public ConstantExpression? ConstantExpression { get; set; }
                public MemberExpression? MemberExpression { get; set; }
                public Expression? BodyExpression { get; set; }
                public ExpressionData() { }

                [MemberNotNullWhen(true, nameof(ConstantExpression))]
                [MemberNotNullWhen(true, nameof(MemberExpression))]
                public bool IsReady => this.MemberExpression != null && this.ConstantExpression != null;

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
                                if (memberType == typeof(string))
                                {
#pragma warning disable CS8600
                                    this.BodyExpression = Expression.Call(this.MemberExpression, Predicate.StringContainsMethod, this.ConstantExpression);
#pragma warning restore CS8600
                                }
                                else
                                {
                                    this.BodyExpression = Expression.Call(null, Predicate.MakeGenericContains(memberType), this.ConstantExpression, this.MemberExpression);
                                }
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

            readonly ExpressionData m_expression_data;

            public CompareType CompareType { get; set; }
            public LegacyBindingPredicate(CompareType compareType)
            {
                this.CompareType = compareType;
                this.m_expression_data = new();
            }

            public void BindMember(PropertyInfo memberProperty)
            {
                this.m_expression_data.MemberExpression = Expression.Property(ExpressionData.ParameterExpression, memberProperty);
            }
            public void BindMember(string memberName)
            {
                this.m_expression_data.MemberExpression = Expression.PropertyOrField(ExpressionData.ParameterExpression, memberName);
            }
            public void BindConstant(object? constantValue)
            {
                this.m_expression_data.ConstantExpression = Expression.Constant(constantValue);
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
                var metaPredicate = new LegacyBindingPredicate<T>(this.CompareType);
                metaPredicate.BindMember(memberProperty);
                metaPredicate.BindConstant(constantValue);
                return metaPredicate.GetPredicate()!;
            }

            public Expression<Func<T, bool>> GetPredicate(string memberName, object? constantValue)
            {
                var metaPredicate = new LegacyBindingPredicate<T>(this.CompareType);
                metaPredicate.BindMember(memberName);
                metaPredicate.BindConstant(constantValue);
                return metaPredicate.GetPredicate()!;
            }
        }

        static Dictionary<Type, MethodInfo> GenericContains;
        static MethodInfo EnumerableGenericContainsMethod;
        static MethodInfo StringContainsMethod;
        static MethodInfo MakeGenericContains(Type _type)
        {
            Type type = _type.RemoveNullable();
            MethodInfo? baseContains = null;
            if (!GenericContains.TryGetValue(type, out baseContains))
            {
                baseContains = EnumerableGenericContainsMethod.MakeGenericMethod(type);
                GenericContains.Add(type, baseContains);
            }
            return baseContains.MakeGenericMethod(type);
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
        public static Expression<Func<T, bool>> GetExpression<T>(string memberName, CompareType compareType, object? constantValue)
        {
            return new LegacyBindingPredicate<T>(compareType).GetPredicate(memberName, constantValue);
        }
        public static Expression<Func<T, bool>> GetExpression<T, TKey>(Expression<Func<T, TKey>> getKey, CompareType compareType, object? constantValue)
        {
            string? memberName = GetPropertyValue(GetPropertyValue(getKey.Body, "Member"), "Name")?.ToString();
            if (string.IsNullOrEmpty(memberName)) throw new InvalidOperationException();
            return new LegacyBindingPredicate<T>(compareType).GetPredicate(memberName, constantValue);
        }
    }
}
