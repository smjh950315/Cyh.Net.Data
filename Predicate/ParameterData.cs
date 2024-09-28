namespace Cyh.Net.Data.Predicate
{
    public class ParameterData
    {
        public string MemberName { get; set; } = string.Empty;
        public LinkType LinkType { get; set; }
        public CompareType CompareType { get; set; }
        public object? ConstantValue { get; set; }
    }
}
