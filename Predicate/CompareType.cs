namespace Cyh.Net.Data.Predicate
{
    public enum CompareType
    {
        None = 0,
        Equal = 1,
        NotEqual = 2,
        GreaterThan = 3,
        GreaterThanOrEqual = 4,
        LessThan = 5,
        LessThanOrEqual = 6,

        /// <summary>
        /// Target = ?Constant?
        /// </summary>
        Contains = 7,

        /// <summary>
        /// Target = ? or ? or ...
        /// </summary>
        IsAnyOf = 8,

        /// <summary>
        /// Target = ?Constant_1? or ?Constant_2? or ?Constant_3?...
        /// </summary>
        ContainsAnyOf = 9,

        /// <summary>
        /// ?Target? = Constant_1 or Constant_2 or Constant_3...
        /// </summary>
        IncludedByAnyOf = 10,
    }
}
