/*
 * File: Parser.cs
 * Project: lib
 * Created Date: 28/12/2020
 * Author: Shun Suzuki
 * -----
 * Last Modified: 21/05/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2020 Hapis Lab. All rights reserved.
 * 
 */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace autd_wrapper_generator.lib
{
    internal sealed class Parser
    {
        private const string ExportPrefix = "EXPORT_AUTD";

        private readonly Regex _rx;

        public Parser()
        {
            _rx = new Regex(@$"^{ExportPrefix}\s(?<ret>.+?)\s(?<name>.*?)\((?<args>.*)\)");
        }

        public IEnumerable<Function> EnumerateFunctions(string filePath)
        {
            var sr = new StreamReader(filePath);
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine()?.Trim();
                if (line == null) continue;

                if (!line.StartsWith(ExportPrefix)) continue;

                while (!line.EndsWith(';'))
                {
                    if (sr.EndOfStream) yield break;
                    line += sr.ReadLine()?.Trim();
                }

                yield return ParseFunction(line);
            }
        }

        private static List<Argument> ParseArgs(string str)
        {
            return str.Split(',')
                .Select(Argument.From)
                .ToList();
        }

        private Function ParseFunction(string line)
        {
            var matches = _rx.Matches(line);

            if (matches.Count <= 0) return new Function(TypeSignature.None, string.Empty, null);

            var match = matches[0];
            var ret = CTypeGen.From(match.Groups["ret"].Value);
            var name = match.Groups["name"].Value;
            var args = ParseArgs(match.Groups["args"].Value);
            return new Function(ret, name, args);
        }
    }
}
