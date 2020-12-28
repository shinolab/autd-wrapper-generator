using System;
using autd_wrapper_generator.lib;

namespace autd_wrapper_generator
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: awg.exe [capi_header_file_path]");
                return;
            }

            var cHeaderPath = args[0];

            var generators = new (ICodeGenerator, string)[] {
                (new CSharpCodeGenerator(), "NativeMethods.cs"), 
                (new PythonCodeGenerator(), "nativemethods.py")
            };

            foreach (var (gen, filename) in generators)
            {
                var parser = new Parser(cHeaderPath);
                var writer = new CodeWriter(gen, filename);
                writer.Write(parser);
            }
        }
    }
}
