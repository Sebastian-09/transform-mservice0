
namespace TransformService.Models
{
    public class Production
    {
        public string NonTerminal { get; set; } = string.Empty;
        public string RightSide { get; set; } = string.Empty;
    }

    public class Grammar
    {
        public string StartSymbol { get; set; } = string.Empty;
        public List<Production> Productions { get; set; } = new();
    }
}
