﻿/*
 * File: Function.cs
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
using System.Linq;
using System.Text;

namespace autd_wrapper_generator.lib
{

    internal record Argument
    {
        internal TypeSignature TypeSignature { get; }
        internal string Name { get; }

        internal static Argument From(string arg)
        {
            arg = arg.Trim();
            var name = arg.Split(' ').Last();
            var type = CTypeGen.From(arg[..^name.Length]);
            return new Argument(type, name);
        }

        private Argument(TypeSignature typeSignature, string name)
        {
            (TypeSignature, Name) = (typeSignature, name);
        }
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
            foreach (var argument in ArgumentsList) sb.Append($"({argument.TypeSignature}:{argument.Name})");
            return sb.ToString();
        }
    }
}
