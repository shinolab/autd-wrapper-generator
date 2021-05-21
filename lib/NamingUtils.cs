/*
 * File: NamingUtils.cs
 * Project: lib
 * Created Date: 29/12/2020
 * Author: Shun Suzuki
 * -----
 * Last Modified: 21/05/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2020 Hapis Lab. All rights reserved.
 * 
 */

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace autd_wrapper_generator.lib
{
    internal static class NamingUtils
    {
        private static readonly string[] Abbreviations = { "AUTD", "STM", "PCM", "SOEM", "TwinCAT" };
        private static readonly Regex Regex = new(string.Join('|', Abbreviations.Select(abbr => $@"(?<word>{abbr})")) + @"|(?<word>[A-Z][a-z]+)");

        internal static string SnakeToLowerCamel(string snake)
        {
            if (string.IsNullOrEmpty(snake))
            {
                return snake;
            }

            return snake
                .Split('_', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => char.ToUpperInvariant(s[0]) + s[1..])
                .Aggregate(char.ToLowerInvariant(snake[0]).ToString(), (s1, s2) => s1 + s2)
                .Remove(1, 1);
        }

        internal static string CamelToSnake(string camel)
        {
            return string.Join('_', Regex.Matches(camel).Select(match => match.Groups["word"].Value).Select(word => word.ToLower()));
        }

        internal static string ToSnake(string str)
        {
            return str.Replace('-', '_');
        }

    }
}
