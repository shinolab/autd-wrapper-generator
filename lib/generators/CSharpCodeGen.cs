/*
 * File: CSharpCodeGen.cs
 * Project: generators
 * Created Date: 21/05/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 21/05/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace autd_wrapper_generator.lib.generators
{
    internal class CSharpCodeGen : ICodeGenerator
    {
        private static readonly string[] Reserved = { "out", "params" };

        public string GetCommentPrefix()
        {
            return "//";
        }

        public string GetFileHeader()
        {
            return @"
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AUTD3Sharp
{
    internal static unsafe class NativeMethods
    {
";
        }

        public string GetFileFooter()
        {
            return @"
    }
}
";
        }

        public string GetFunctionDefinition(Function func, string libName)
        {
            return $"        {GetHeader(libName, func.ReturnTypeSignature, func.ArgumentsList)} {func.Name}({GetArgs(func.ArgumentsList)});";
        }

        private static string ReplaceReserved(string name)
        {
            if (Reserved.Contains(name)) return "@" + name;
            return name;
        }

        private static string GetArgs(IEnumerable<Argument> args)
        {
            var sb = new StringBuilder();
            sb.Append(string.Join(", ", args.Select(arg =>
            {
                var annotation = arg.TypeSignature.Type == CType.Bool ? "[MarshalAs(UnmanagedType.U1)] " : "";
                var argName = ReplaceReserved(NamingUtils.SnakeToLowerCamel(arg.Name));
                return $"{annotation}{MapArgType(arg.TypeSignature)} {argName}";
            })));
            return sb.ToString();
        }

        private static string GetHeader(string libName, TypeSignature ret, IEnumerable<Argument> args)
        {
            var sb = new StringBuilder();
            sb.Append($"[DllImport(\"{libName}\", ");
            if (args != null && args.Select(x => x.TypeSignature.Type).Any(ty => ty == CType.Bool))
                sb.Append("CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, ");
            sb.Append("CallingConvention = CallingConvention.StdCall)] ");

            if (ret.Type == CType.Bool)
                sb.Append("[return: MarshalAs(UnmanagedType.U1)] ");
            sb.Append($"public static extern {MapRetType(ret)}");
            return sb.ToString();
        }

        private static string MapRetType(TypeSignature sig) => sig.Ptr == PtrOption.None ? MapType(sig.Type) : throw new InvalidExpressionException(sig + " cannot to convert to C# type.");

        private static string MapArgType(TypeSignature sig)
        {
            var type = sig.Type;
            return type switch
            {
                CType.Void => sig.Ptr switch
                {
                    PtrOption.None => throw new InvalidExpressionException(sig + " cannot to convert to C# type."),
                    PtrOption.Ptr => "IntPtr",
                    PtrOption.PtrPtr => "out IntPtr",
                    _ => throw new InvalidExpressionException(sig + " cannot to convert to C# type.")
                },
                CType.Char => sig.Ptr switch
                {
                    PtrOption.None => MapType(type),
                    PtrOption.Ptr => "StringBuilder?",
                    PtrOption.PtrPtr => throw new InvalidExpressionException(sig + " cannot to convert to C# type."),
                    _ => throw new InvalidExpressionException(sig + " cannot to convert to C# type.")
                },
                _ => sig.Ptr switch
                {
                    PtrOption.None => MapType(type),
                    PtrOption.Ptr => MapType(type) + "*",
                    PtrOption.PtrPtr => throw new InvalidExpressionException(sig + " cannot to convert to C# type."),
                    _ => throw new InvalidExpressionException(sig + " cannot to convert to C# type.")
                }
            };
        }

        private static string MapType(CType type)
        {
            return type switch
            {
                CType.None => throw new InvalidExpressionException(type + " cannot to convert to C# type."),
                CType.Void => "void",
                CType.Bool => "bool",
                CType.Char => "char",
                CType.String => "string",
                CType.Int8 => "sbyte",
                CType.Uint8 => "byte",
                CType.Int16 => "short",
                CType.Uint16 => "ushort",
                CType.Int32 => "int",
                CType.Int64 => "long",
                CType.UInt32 => "uint",
                CType.UInt64 => "ulong",
                CType.Float32 => "float",
                CType.Float64 => "double",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}
