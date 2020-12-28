using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace autd_wrapper_generator.lib
{
    internal sealed class Parser : IEnumerable<Function>, IDisposable
    {
        private const string ExportPrefix = "EXPORT_AUTD_DLL";

        private readonly StreamReader _sr;
        private readonly Regex _rx;

        public Parser(string fileName)
        {
            _sr = new StreamReader(fileName);
            _rx = new Regex(@$"^{ExportPrefix}\s(?<ret>.+?)\s(?<name>.*?)\((?<args>.*)\)");
        }

        private static List<Argument> ParseArgs(string str)
        {
            return str.Split(',')
                .Select(Argument.From)
                .ToList();

        }

        private Function Parse(string line)
        {
            var matches = _rx.Matches(line);

            if (matches.Count <= 0) return new Function(TypeSignature.None, string.Empty, null);

            var match = matches[0];
            var ret = CTypeGen.From(match.Groups["ret"].Value);
            var name = match.Groups["name"].Value;
            var args = ParseArgs(match.Groups["args"].Value);
            return new Function(ret, name, args);
        }

        public IEnumerator<Function> GetEnumerator()
        {
            while (!_sr.EndOfStream)
            {
                var line = _sr.ReadLine()?.Trim();
                if (line == null) continue;

                if (!line.StartsWith(ExportPrefix)) continue;

                while (!line.EndsWith(';'))
                {
                    if (_sr.EndOfStream) yield break;
                    line += _sr.ReadLine()?.Trim();
                }

                yield return Parse(line);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _sr.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
