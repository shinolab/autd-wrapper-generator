using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace autd_wrapper_generator.lib
{
    internal class CSharpCodeGenerator : ICodeGenerator
    {
        private static readonly string[] Reserved = {"out", "params"};

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
        private const string DllName = ""autd3capi"";
";
        }

        public string GetFileFooter()
        {
            return @"
    }
}
";
        }

        public string GetFunctionDefinition(Function func)
        {
            return $"        {GetHeader(func.ReturnTypeSignature, func.ArgumentsList)} {func.Name}({GetArgs(func.ArgumentsList)});";
        }

        private static string ReplaceReserved(string name)
        {
            if (Reserved.Contains(name))
                return "@" + name;
            return name;
        }

        private static string SnakeToLowerCamel(string snake)
        {
            if (string.IsNullOrEmpty(snake)) return snake;
            var camel = snake
                    .Split('_', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => char.ToUpperInvariant(s[0]) + s[1..])
                    .Aggregate(char.ToLowerInvariant(snake[0]).ToString(), (s1, s2) => s1 + s2)
                    .Remove(1, 1);
            return ReplaceReserved(camel);
        }

        private static string GetArgs(IEnumerable<Argument> args)
        {
            var sb = new StringBuilder();
            sb.Append(string.Join(", ", args.Select(arg =>
            {
                var annotation = arg.TypeSignature.Type == CType.Bool ? "[MarshalAs(UnmanagedType.U1)] " : "";
                return $"{annotation}{MapArgType(arg)} {SnakeToLowerCamel(arg.Name)}";
            })));
            return sb.ToString();
        }

        private static string GetHeader(TypeSignature ret, IEnumerable<(TypeSignature, string)> args)
        {
            var sb = new StringBuilder();
            sb.Append("[DllImport(DllName, ");
            if (args != null && args.Select(x => x.Item1.Type).Any(ty => ty == CType.Bool))
            {
                sb.Append("CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, ");
            }
            sb.Append("CallingConvention = CallingConvention.StdCall)] ");

            if (ret.Type == CType.Bool)
            {
                sb.Append("[return: MarshalAs(UnmanagedType.U1)] ");
            }
            sb.Append($"public static extern {MapRetType(ret)}");
            return sb.ToString();
        }

        private static string MapRetType(TypeSignature sig)
        {
            return sig.Type switch
            {
                CType.Char => sig.Ptr switch
                {
                    PtrOption.None => MapType(sig.Type),
                    PtrOption.Ptr => "string",
                    PtrOption.PtrPtr => throw new InvalidExpressionException(sig + " cannot to convert to C# type."),
                    _ => throw new InvalidExpressionException(sig + " cannot to convert to C# type.")
                },
                CType.Void => sig.Ptr switch
                {
                    PtrOption.None => MapType(sig.Type),
                    PtrOption.Ptr => "IntPtr",
                    PtrOption.PtrPtr => "IntPtr*",
                    _ => throw new InvalidExpressionException(sig + " cannot to convert to C# type.")
                },
                _ => sig.Ptr switch
                {
                    PtrOption.None => MapType(sig.Type),
                    PtrOption.Ptr => MapType(sig.Type) + "*",
                    PtrOption.PtrPtr => throw new InvalidExpressionException(sig + " cannot to convert to C# type."),
                    _ => throw new InvalidExpressionException(sig + " cannot to convert to C# type.")
                }
            };
        }

        private static string MapArgType(Argument arg)
        {
            return arg.TypeSignature.Type switch
            {
                CType.Char => arg.TypeSignature.Ptr switch
                {
                    PtrOption.None => MapType(arg.TypeSignature.Type),
                    PtrOption.Ptr => arg.IsConst ? "string" : "StringBuilder",
                    PtrOption.PtrPtr => "string*",
                    _ => throw new InvalidExpressionException(arg.TypeSignature + " cannot to convert to C# type.")
                },
                CType.Void => arg.TypeSignature.Ptr switch
                {
                    PtrOption.None => MapType(arg.TypeSignature.Type),
                    PtrOption.Ptr => "IntPtr",
                    PtrOption.PtrPtr => arg.IsConst ? "IntPtr*" : "out IntPtr",
                    _ => throw new InvalidExpressionException(arg.TypeSignature + " cannot to convert to C# type.")
                },
                _ => sig.Ptr switch
                {
                    PtrOption.None => MapType(sig.Type),
                    PtrOption.Ptr => MapType(sig.Type) + "*",
                    PtrOption.PtrPtr => "out " + MapType(sig.Type) + "*",
                    _ => throw new InvalidExpressionException(arg.TypeSignature + " cannot to convert to C# type.")
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
