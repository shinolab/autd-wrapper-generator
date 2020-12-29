/*
 * File: Program.cs
 * Project: autd-wrapper-generator
 * Created Date: 28/12/2020
 * Author: Shun Suzuki
 * -----
 * Last Modified: 28/12/2020
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2020 Hapis Lab. All rights reserved.
 * 
 */

using autd_wrapper_generator.lib;
using System;

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
                (new PythonCodeGenerator(), "nativemethods.py"),
                (new JuliaCodeGenerator(), "NativeMethods.jl")
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
