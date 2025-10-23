using System.Text.RegularExpressions;

namespace TransformService.Services
{
    public class GrammarTransformer
    {
        public object Factorize(Dictionary<string, List<string>> grammar)
        {
            var result = new Dictionary<string, List<string>>();
            var steps = new List<string>();

            foreach (var kv in grammar)
            {
                string nonTerminal = kv.Key;
                var productions = kv.Value;

                // Agrupar por prefijo común
                var grouped = productions.GroupBy(p => p.Split(' ')[0]).ToList();
                var newProductions = new List<string>();

                foreach (var group in grouped)
                {
                    if (group.Count() > 1)
                    {
                        string prefix = group.Key;
                        string newNT = nonTerminal + "'";
                        newProductions.Add(prefix + " " + newNT);
                        var subRules = group.Select(g => string.Join(' ', g.Split(' ').Skip(1))).ToList();
                        result[newNT] = subRules;
                        steps.Add($"Factorización aplicada a {nonTerminal}: prefijo común '{prefix}' → nuevo no terminal {newNT}.");
                    }
                    else
                    {
                        newProductions.Add(group.First());
                    }
                }

                result[nonTerminal] = newProductions;
            }

            return new { productions = result, steps };
        }

        public object EliminateLeftRecursion(Dictionary<string, List<string>> grammar)
        {
            var result = new Dictionary<string, List<string>>();
            var steps = new List<string>();

            foreach (var kv in grammar)
            {
                string A = kv.Key;
                var alpha = new List<string>();
                var beta = new List<string>();

                foreach (var prod in kv.Value)
                {
                    var symbols = prod.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (symbols[0] == A)
                        alpha.Add(string.Join(' ', symbols.Skip(1)));
                    else
                        beta.Add(prod);
                }

                if (alpha.Any())
                {
                    string A1 = A + "'";
                    result[A] = beta.Select(b => b + " " + A1).ToList();
                    result[A1] = alpha.Select(a => a + " " + A1).ToList();
                    result[A1].Add("ε");
                    steps.Add($"Eliminada recursión izquierda en {A}: se creó {A1}.");
                }
                else
                {
                    result[A] = kv.Value;
                }
            }

            return new { productions = result, steps };
        }

        public object TransformStepByStep(Dictionary<string, List<string>> grammar)
        {
            var factorized = Factorize(grammar);
            var noRecursion = EliminateLeftRecursion(grammar);
            return new
            {
                message = "Proceso completo de transformación",
                factorization = factorized,
                recursionElimination = noRecursion
            };
        }
    }
}
