using System.Data;
using System.Linq.Expressions;
using System.Reflection;

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

        public static IDbConnectionBuilder GetDbConnectionBuilder(Func<IDbConnection> connectionFactory)
        {
            return new DbConnectionBuilder(connectionFactory);
        }
    }

    public static class QueryableExtension
    {
        static readonly Type StringType = typeof(string);
        static Dictionary<Type, object?> _cachedTypeDefaultValue = new();
        static MethodInfo? _cachedMethodForSelectorExpr;
        static Dictionary<Type, PropertyInfo[]> _cachedTypeProperties = new();
        static Dictionary<string, object> _cachedByNameSelectExpressions = new();
        static Dictionary<Type, Func<MemberExpression, Expression>> _cachedToStringExpressionMaker = new();
        static Type[] _builtinTypes;
        static readonly MethodInfo _Prototype_MakeByNameSelectExpression = typeof(QueryableExtension).GetMethod(nameof(MakeByNameSelectExpression), BindingFlags.Static | BindingFlags.Public, [])!;

        static QueryableExtension()
        {
            // Pre-cache common value type default values
            Type[] commonValueTypes = [
                typeof(int),
            typeof(long),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(bool),
            typeof(DateTime),
            typeof(Guid),
            StringType
            ];
            List<Type> builtinTypes = [];
            var toString = typeof(Object).GetMethod("ToString", [])!;
            Func<MemberExpression, Expression> funcToStrMaker = (sourceAccessor) =>
            {
                Type sourceType = sourceAccessor.Type;
                if (sourceType.IsValueType)
                {
                    return Expression.Call(sourceAccessor, toString);
                }
                else
                {
                    return Expression.Condition(
                        Expression.Equal(sourceAccessor, Expression.Constant(null, sourceType)),
                        Expression.Constant(null, sourceType),
                        Expression.Call(sourceAccessor, toString)
                        );
                }
            };
            var strIsNullOrEmpty = StringType.GetMethod("IsNullOrEmpty");
            foreach (var type in commonValueTypes)
            {
                if (type.IsValueType)
                {
                    var defaultValue = Activator.CreateInstance(type)!;
                    _cachedTypeDefaultValue[type] = defaultValue;
                    builtinTypes.Add(type);
                    Type nullableType = typeof(Nullable<>).MakeGenericType(type);
                    _cachedTypeDefaultValue[nullableType] = Activator.CreateInstance(nullableType);
                    builtinTypes.Add(nullableType);
                }
                else
                {
                    _cachedTypeDefaultValue[type] = null;
                    builtinTypes.Add(type);
                }
                _cachedToStringExpressionMaker[type] = funcToStrMaker;
            }
            _builtinTypes = builtinTypes.ToArray();
            _cachedMethodForSelectorExpr = typeof(Expression).GetMethod("Lambda", 1, [typeof(Expression), typeof(bool), typeof(IEnumerable<ParameterExpression>)]);
        }

        static Expression GetDefaultValueExpression(Type type)
        {
            if (_cachedTypeDefaultValue.TryGetValue(type, out var constVal))
            {
                return Expression.Constant(constVal, type);
            }
            if (type.IsValueType)
            {
                constVal = Activator.CreateInstance(type);
            }
            else
            {
                constVal = null;
            }
            _cachedTypeDefaultValue.TryAdd(type, constVal);
            return Expression.Constant(constVal, type);
        }
        static MemberAssignment GetDefaultValueAssigment(PropertyInfo property)
        {
            return Expression.Bind(property, GetDefaultValueExpression(property.PropertyType));
        }
        static PropertyInfo[] GetTypeProperties(Type type)
        {
            if (_cachedTypeProperties.TryGetValue(type, out var props))
            {
                return props;
            }
            props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.MemberType == System.Reflection.MemberTypes.Property)
                .ToArray();
            _cachedTypeProperties[type] = props;
            return props;
        }

        static bool IsBuiltinType(Type type)
        {
            return _builtinTypes.Contains(type);
        }

        static MemberAssignment GetConventionOrDefault(PropertyInfo targetProperty, MemberExpression sourceAccessor, Type sourceType)
        {
            if (targetProperty.PropertyType == StringType)
            {
                if (_cachedToStringExpressionMaker.TryGetValue(sourceType, out var toStr))
                {
                    var expr = toStr(sourceAccessor);
                    return Expression.Bind(targetProperty, expr);
                }
            }
            return GetDefaultValueAssigment(targetProperty);
        }

        static List<MemberAssignment> GetMemberAssignments(
            MemberExpression sourceAccessor,
            PropertyInfo targetProperty)
        {
            PropertyInfo sourceProperty = sourceAccessor.Member as PropertyInfo;
            Type sourceType = sourceProperty!.PropertyType;
            Type targetType = targetProperty.PropertyType;
            List<MemberAssignment> memberAssignments = [];

            // 內建型別
            if (IsBuiltinType(sourceType) && IsBuiltinType(targetType))
            {
                if (sourceType == targetType)
                {
                    // same type !
                    memberAssignments.Add(Expression.Bind(targetProperty, sourceAccessor));
                    return memberAssignments;
                }
                else
                {
                    var _sourceType = Nullable.GetUnderlyingType(sourceType);
                    var _targetType = Nullable.GetUnderlyingType(targetType);
                    var sourceAccessType = _sourceType ?? sourceType;
                    var targetAccessType = _targetType ?? targetType;
                    if (_sourceType != null && _targetType != null) // Both are nullable<>
                    {
                        if (sourceAccessType == targetAccessType)
                        {
                            // same type ! (基本不可能)
                            memberAssignments.Add(Expression.Bind(targetProperty, sourceAccessor));
                            return memberAssignments;
                        }
                        memberAssignments.Add(GetConventionOrDefault(targetProperty, sourceAccessor, sourceAccessType));
                        return memberAssignments;
                    }
                    else if (_sourceType == null && _targetType == null)
                    {
                        if (sourceAccessType == targetAccessType)
                        {
                            // same type ! (基本不可能)
                            memberAssignments.Add(Expression.Bind(targetProperty, sourceAccessor));
                            return memberAssignments;
                        }
                        memberAssignments.Add(GetConventionOrDefault(targetProperty, sourceAccessor, sourceAccessType));
                        return memberAssignments;
                    }
                    else if (_sourceType == null && _targetType != null)
                    {
                        if (sourceAccessType == targetAccessType) // x? = x
                        {
                            // x? = x
                            memberAssignments.Add(Expression.Bind(targetProperty, sourceAccessor));
                            return memberAssignments;
                        }
                        memberAssignments.Add(GetConventionOrDefault(targetProperty, sourceAccessor, sourceAccessType));
                        return memberAssignments;
                    }
                    else if (_sourceType != null && _targetType == null)
                    {
                        if (sourceAccessType == targetAccessType) // x = x?
                        {
                            var exprTest = Expression.Equal(sourceAccessor, Expression.Constant(null, sourceType));
                            var exprIfT = GetDefaultValueExpression(targetType);
                            var exprIfF = Expression.Convert(sourceAccessor, targetType);
                            var getIfNotNull = Expression.Condition(
                                exprTest,
                                exprIfT,
                                exprIfF);
                            memberAssignments.Add(Expression.Bind(targetProperty, getIfNotNull));
                            return memberAssignments;
                        }
                        memberAssignments.Add(GetConventionOrDefault(targetProperty, sourceAccessor, sourceAccessType));
                        return memberAssignments;
                    }
                    throw new NotImplementedException();
                }
            }
            else
            {
                var sourceProps = GetTypeProperties(sourceType)
                    .Where(p => p.CanRead && p.MemberType == System.Reflection.MemberTypes.Property)
                    .ToArray();
                var targetProps = GetTypeProperties(targetType)
                    .Where(p => p.CanWrite && p.MemberType == System.Reflection.MemberTypes.Property)
                    .ToArray();
                MemberInitExpression exprInit;
                {
                    var newExpr = Expression.New(targetType);
                    List<MemberAssignment> subMemberAssignments = [];
                    foreach (var tProp in targetProps)
                    {
                        var sProp = sourceProps.FirstOrDefault(x => x.Name == tProp.Name);
                        if (sProp != null)
                        {
                            var subSourceProperty = sProp;
                            var subTargetProperty = tProp;
                            var subSourcePropertyAccessor = Expression.Property(sourceAccessor, subSourceProperty);
                            var subSourceType = subSourceProperty.PropertyType;
                            var subTargetType = subTargetProperty.PropertyType;
                            subMemberAssignments.AddRange(GetMemberAssignments(
                                subSourcePropertyAccessor,
                                subTargetProperty));
                        }
                        else
                        {
                            subMemberAssignments.Add(GetDefaultValueAssigment(tProp));
                        }
                    }
                    exprInit = Expression.MemberInit(newExpr, subMemberAssignments);
                }
                var sourcePropertyIsNull = Expression.Equal(sourceAccessor, Expression.Constant(null, sourceType));
                var ifNullSetNull = Expression.Condition(
                    sourcePropertyIsNull,
                    Expression.Constant(null, targetType),
                    exprInit);
                memberAssignments.Add(Expression.Bind(targetProperty, ifNullSetNull));
                return memberAssignments;
            }
        }

        public static Expression<Func<TSource, TTarget>> MakeByNameSelectExpression<TSource, TTarget>() where TSource : class where TTarget : class
        {
            Type sourceType = typeof(TSource);
            Type targetType = typeof(TTarget);
            string cacheKey = $"{sourceType.FullName}->{targetType.FullName}";
            if (_cachedByNameSelectExpressions.TryGetValue(cacheKey, out var cachedExpr))
            {
                return (Expression<Func<TSource, TTarget>>)cachedExpr;
            }
            var parameter = Expression.Parameter(sourceType, "src");
            var memberAssignments = new List<MemberAssignment>();
            var sourceProperties = GetTypeProperties(sourceType)
                .Where(p => p.CanRead && p.MemberType == System.Reflection.MemberTypes.Property)
                .ToArray();
            var targetProperties = GetTypeProperties(targetType)
                .Where(p => p.CanWrite && p.MemberType == System.Reflection.MemberTypes.Property)
                .ToArray();

            var newExpr = Expression.New(targetType);
            foreach (var tprop in targetProperties)
            {
                var sProp = sourceProperties.FirstOrDefault(x => x.Name == tprop.Name);
                if (sProp != null)
                {
                    memberAssignments.AddRange(GetMemberAssignments(Expression.Property(parameter, sProp), tprop));
                }
                else
                {
                    memberAssignments.Add(GetDefaultValueAssigment(tprop));
                }
            }
            var exprInit = Expression.MemberInit(newExpr, memberAssignments);
            Type funcType = typeof(Func<,>).MakeGenericType(sourceType, targetType);
            object expr = _cachedMethodForSelectorExpr.MakeGenericMethod(funcType)
                .Invoke(null, [exprInit, false, new ParameterExpression[] { parameter }])!;
            _cachedByNameSelectExpressions[cacheKey] = expr;
            return (Expression<Func<TSource, TTarget>>)expr;
        }

        public static object MakeByNameSelectExpression(Type sourceType, Type targetType)
        {
            string cacheKey = $"{sourceType.FullName}->{targetType.FullName}";
            if (_cachedByNameSelectExpressions.TryGetValue(cacheKey, out object? expr))
                return expr;
            return _Prototype_MakeByNameSelectExpression.MakeGenericMethod(sourceType, targetType).Invoke(null, [])!;
        }

        public static IQueryable<TResult> SelectByNameMapping<TSource, TResult>(this IQueryable<TSource> source)
            where TSource : class
            where TResult : class
        {
            var selector = MakeByNameSelectExpression<TSource, TResult>();
            return source.Select(selector);
        }
    }
}
