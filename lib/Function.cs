using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace autd_wrapper_generator.lib
{

    internal record Argument
    {
        internal bool IsConst { get; }
        internal TypeSignature TypeSignature { get; }
        internal string Name { get; }

        internal static Argument From(string[] tokens)
        {
            var name = tokens.Last();
            var isConst = tokens.Reverse().Skip(1).Any(x => x.Contains("const"));
            var type = tokens.Reverse().Skip(1).FirstOrDefault(x => !x.Contains("const"));

            return tokens.Length == 3 ? new Argument(tokens[0] == "const", CTypeGen.From(tokens[1]), tokens[2]) : new Argument(false, CTypeGen.From(tokens[0]), tokens[1]);
        }

        private Argument(bool isConst, TypeSignature typeSignature, string name) => (IsConst, TypeSignature, Name) = (IsConst, typeSignature, name);
    }

    internal class Function
    {
        internal TypeSignature ReturnTypeSignature { get; }
        internal string Name { get; }
        internal List<Argument> ArgumentsList { get; }

        public Function(TypeSignature ret, string name, List<Argument> args)
        {
            ReturnTypeSignature = ret;
            Name = name;
            ArgumentsList = args;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{ReturnTypeSignature}:");
            sb.Append($"{Name}:");
            foreach (var argument in ArgumentsList)
                sb.Append($"({argument.TypeSignature}:{argument.Name})");
            return sb.ToString();
        }
    }
}
