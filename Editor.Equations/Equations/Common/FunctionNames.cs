using System.Collections.Generic;
using System.Linq;

namespace Editor
{
    internal static class FunctionNames
    {
        private static List<string> Names { get; } = new List<string>();

        static FunctionNames()
        {
            Names.AddRange(new [] {
                "arccos", "arcsin", "arctan", "arg", "cos", "cosh", "cot", "coth",
                "cov", "csc", "curl", "deg", "det", "dim", "div", "erf", "exp", "gcd", "glb", "grad", "hom", "lm",
                "inf", "int", "ker", "lg", "lim", "ln", "log", "lub", "max",
                "min", "mod", "Pr", "Re", "rot", "sec", "sgn", "sin", "sinh", "sup", "tan", "tanh", "var",                                           
            });
        }

        public static bool IsFunctionName(string text)
        {
            return Names.Contains(text);
        }

        public static string CheckForFunctionName(string text)
        {
            return Names.FirstOrDefault(text.EndsWith);
        }
    }
}
