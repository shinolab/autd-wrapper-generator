/*
 * File: JuliaCodeGen.cs
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
    internal class JuliaCodeGen : ICodeGenerator
    {
        private readonly HashSet<string> _libs = new();

        public string GetCommentPrefix()
        {
            return "#";
        }

        public string GetFileHeader()
        {
            return @"
function get_lib_ext()
    if Sys.iswindows()
        return "".dll""
    elseif Sys.isapple()
        return "".dylib""
    elseif Sys.islinux()
        return "".so""
    end
end

function get_lib_prefix()
    if Sys.iswindows()
        return """"
    else 
        return ""lib""
    end
end

";
        }

        public string GetFileFooter()
        {
            return string.Empty;
        }

        public string GetFunctionDefinition(Function func, string libName)
        {
            var sb = new StringBuilder();
            if (!_libs.Contains(libName))
            {
                _libs.Add(libName);
                sb.AppendLine($"const _{NamingUtils.ToSnake(libName)} = joinpath(@__DIR__, \"bin\", get_lib_prefix() * \"{libName}\" * get_lib_ext())");
            }

            sb.AppendLine($"{GetHeader(func)} = ccall((:{func.Name},  _{NamingUtils.ToSnake(libName)}), {MapRetType(func.ReturnTypeSignature)}, ({func.ArgumentsList.Select(arg => MapArgType(arg.TypeSignature) + ", ").Aggregate(string.Empty, (s, a) => s + a)}), {GetArgs(func)})");
            return sb.ToString();
        }

        private static string GetArgs(Function func)
        {
            return string.Join(',', func.ArgumentsList.Select(arg => arg.Name));
        }

        private static string GetHeader(Function func)
        {
            return $"{NamingUtils.CamelToSnake(func.Name)}({GetArgs(func)})";
        }

        private static string MapRetType(TypeSignature sig)
        {
            var type = sig.Type;
            return sig.Ptr switch
            {
                PtrOption.None => MapType(type),
                PtrOption.Ptr => "Array{" + MapType(type) + ", 1}",
                PtrOption.PtrPtr or _ => throw new InvalidExpressionException(sig + " cannot to convert to Julia type.")
            };
        }

        private static string MapArgType(TypeSignature sig)
        {
            var type = sig.Type;
            return type switch
            {
                CType.Char => sig.Ptr switch
                {
                    PtrOption.None => MapType(type),
                    PtrOption.Ptr => "Ref{UInt8}",
                    PtrOption.PtrPtr or _ => throw new InvalidExpressionException(sig + " cannot to convert to Julia type.")
                },
                _ => sig.Ptr switch
                {
                    PtrOption.None => MapType(type),
                    PtrOption.Ptr => "Ptr{" + MapType(type) + "}",
                    PtrOption.PtrPtr => "Ref{Ptr{" + MapType(type) + "}}",
                    _ => throw new InvalidExpressionException(sig + " cannot to convert to Julia type.")
                }
            };
        }

        private static string MapType(CType type)
        {
            return type switch
            {
                CType.None => throw new InvalidExpressionException(type + " cannot to convert to Julia type."),
                CType.Void => "Cvoid",
                CType.Bool => "Bool",
                CType.Char => "UInt8",
                CType.String => "Cstring",
                CType.Int8 => "Int8",
                CType.Uint8 => "UInt8",
                CType.Int16 => "Int16",
                CType.Uint16 => "UInt16",
                CType.Int32 => "Int32",
                CType.Int64 => "Int64",
                CType.UInt32 => "UInt32",
                CType.UInt64 => "UInt64",
                CType.Float32 => "Float32",
                CType.Float64 => "Float64",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}