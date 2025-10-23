using System.Text;

namespace TransformService.Services
{
    public class GrammarTransformer
    {
        // Divide una producción en tokens (palabras)
        private static List<string> SplitTokens(string production) =>
            production.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

        // Une tokens nuevamente en una sola cadena
        private static string JoinTokens(IEnumerable<string> tokens)
        {
            var list = tokens.Where(s => !string.IsNullOrEmpty(s)).ToList();
            return list.Count == 0 ? "ε" : string.Join(' ', list);
        }

        // Genera un nuevo símbolo que no esté en uso (A, A', A'', etc.)
        private static string NewSymbol(string baseSymbol, HashSet<string> existing)
        {
            var candidate = baseSymbol + "'";
            while (existing.Contains(candidate))
            {
                candidate += "'";
            }
            existing.Add(candidate);
            return candidate;
        }

        // ✅ FACTORIZATION CORRECTED (handles longer prefixes)
        public object Factorize(Dictionary<string, List<string>> grammar)
{
    var productions = grammar.ToDictionary(k => k.Key, v => v.Value.Select(s => s.Trim()).ToList());
    var steps = new List<string>();
    var existing = new HashSet<string>(productions.Keys);

    foreach (var nt in productions.Keys.ToList())
    {
        bool changed;
        do
        {
            changed = false;
            if (!productions.ContainsKey(nt)) break;

            var rhsList = productions[nt];
            if (rhsList.Count <= 1) break;

            // Convertimos cada producción en lista de tokens
            var tokenized = rhsList
                .Select(r => r.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList())
                .ToList();

            // Buscamos el prefijo más largo común a al menos 2 producciones
            string bestPrefix = "";
            List<int> bestGroup = new();
            int bestLength = 0;

            int maxLen = tokenized.Max(t => t.Count);

            // Intentamos prefijos desde la longitud máxima hacia abajo
            for (int L = maxLen; L >= 1; L--)
            {
                var groups = new Dictionary<string, List<int>>();
                for (int i = 0; i < tokenized.Count; i++)
                {
                    var toks = tokenized[i];
                    if (toks.Count < L) continue;
                    var pref = string.Join(' ', toks.Take(L));
                    if (!groups.ContainsKey(pref))
                        groups[pref] = new List<int>();
                    groups[pref].Add(i);
                }

                var candidate = groups
                    .Where(g => g.Value.Count >= 2)
                    .OrderByDescending(g => g.Key.Split(' ').Length)
                    .FirstOrDefault();

                if (!candidate.Equals(default(KeyValuePair<string, List<int>>)))
                {
                    bestPrefix = candidate.Key;
                    bestGroup = candidate.Value;
                    bestLength = bestPrefix.Split(' ').Length;
                    break; // rompemos porque encontramos el más largo
                }
            }

            // Si no hay prefijo común, terminamos
            if (string.IsNullOrEmpty(bestPrefix)) break;

            // Creamos el nuevo no terminal
            var newNT = nt + "'";
            while (existing.Contains(newNT)) newNT += "'";
            existing.Add(newNT);

            var newList = new List<string>();
            var remaining = new List<string>();

            for (int i = 0; i < tokenized.Count; i++)
            {
                var toks = tokenized[i];
                if (bestGroup.Contains(i))
                {
                    var suffix = toks.Skip(bestLength).ToList();
                    newList.Add(suffix.Count == 0 ? "ε" : string.Join(' ', suffix));
                }
                else
                {
                    remaining.Add(string.Join(' ', toks));
                }
            }

            remaining.Add($"{bestPrefix} {newNT}".Trim());

            productions[nt] = remaining;
            productions[newNT] = newList;

            steps.Add($"Factorización aplicada a {nt}: prefijo común '{bestPrefix}' → nuevo no terminal {newNT} con {newList.Count} alternativas.");
            changed = true;

        } while (changed);
    }

    return new { productions, steps };
}



        // ✅ LEFT RECURSION REMOVAL (direct only)
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
                    var symbols = SplitTokens(prod);
                    if (symbols.Count > 0 && symbols[0] == A)
                        alpha.Add(string.Join(' ', symbols.Skip(1)));
                    else
                        beta.Add(prod);
                }

                if (alpha.Any())
                {
                    string A1 = A + "'";
                    result[A] = beta.Select(b => (b == "ε" ? A1 : (b + " " + A1).Trim())).ToList();
                    result[A1] = alpha.Select(a => ((a == "" ? "" : a + " ") + A1).Trim()).ToList();
                    result[A1].Add("ε");
                    steps.Add($"Left recursion eliminated in {A}: created {A1}.");
                }
                else
                {
                    result[A] = kv.Value;
                }
            }

            return new { productions = result, steps };
        }

        // Combined transformation (both processes)
        public object TransformStepByStep(Dictionary<string, List<string>> grammar)
        {
            var factorized = Factorize(grammar);
            var noRecursion = EliminateLeftRecursion(grammar);
            return new
            {
                message = "Full transformation process completed",
                factorization = factorized,
                recursionElimination = noRecursion
            };
        }
    }
}
