using System.Collections.Generic;
using System.Text;

namespace autd_wrapper_generator.lib
{
    internal class Function
    {
        internal TypeSignature ReturnTypeSignature { get; }
        internal string Name { get; }
        internal List<(TypeSignature, string)> ArgumentsList { get; }

        public Function(TypeSignature ret, string name, List<(TypeSignature, string)> args)
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
            foreach (var (type, name) in ArgumentsList)
                sb.Append($"({type}:{name})");
            return sb.ToString();
        }
    }
}
