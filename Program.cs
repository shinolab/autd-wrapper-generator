/*
 * File: Program.cs
 * Project: autd-wrapper-generator
 * Created Date: 28/12/2020
 * Author: Shun Suzuki
 * -----
 * Last Modified: 21/05/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2020 Hapis Lab. All rights reserved.
 * 
 */

using autd_wrapper_generator.lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using autd_wrapper_generator.lib.generators;

namespace autd_wrapper_generator
{
    internal static class Program
    {
        private static List<(string, string)> EnumerateHeaders(string path)
        {
            var res = new List<(string, string)>();

            var rx = new Regex(@"^add_library\((?<name>.+?) SHARED");
            foreach (var dir in Directory.EnumerateDirectories(path, "*.*", SearchOption.AllDirectories))
            {
                var cmakePath = Path.Join(dir, "CMakeLists.txt");
                if (!File.Exists(cmakePath)) continue;
                var sr = new StreamReader(cmakePath);
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var m = rx.Matches(line ?? string.Empty);
                    if (m.Count <= 0) continue;
                    var name = m[0].Groups["name"].Value;
                    res.AddRange(Directory.EnumerateFiles(dir, "*.h").Select(header => (header, name)));
                }
            }

            return res;
        }

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: awg.exe [capi folder]");
                return;
            }

            var cAPIPath = args[0];
            var headers = EnumerateHeaders(cAPIPath);

            var generators = new (ICodeGenerator, string)[] {
                (new CSharpCodeGen(), "NativeMethods.cs"),
                (new PythonCodeGen(), "native_methods.py"),
                (new JuliaCodeGen(), "NativeMethods.jl"),
            };

            foreach (var (gen, filename) in generators)
            {
                var writer = new CodeWriter(gen, filename);
                foreach (var (header, lib) in headers) writer.Write(header, lib);
                writer.Close();
            }
        }
    }
}
