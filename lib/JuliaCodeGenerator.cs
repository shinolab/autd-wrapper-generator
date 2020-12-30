/*
 * File: JuliaCodeGenerator.cs
 * Project: lib
 * Created Date: 29/12/2020
 * Author: Shun Suzuki
 * -----
 * Last Modified: 29/12/2020
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2020 Hapis Lab. All rights reserved.
 * 
 */


using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace autd_wrapper_generator.lib
{
    internal class JuliaCodeGenerator : ICodeGenerator
    {
        private static readonly string[] Reserved = { "out", "params" };

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

const _dll_name = joinpath(@__DIR__, ""bin"", get_lib_prefix() * ""autd3capi"" * get_lib_ext())
";
        }

        public string GetFileFooter()
        {
            return string.Empty;
        }

        public string GetFunctionDefinition(Function func)
        {
            return $"{GetHeader(func)} = ccall((:{func.Name},  _dll_name), {MapRetType(func.ReturnTypeSignature)}, ({func.ArgumentsList.Select(arg => MapArgType(arg.TypeSignature) + ", ").Aggregate(string.Empty, (s, a) => s + a)}), {GetArgs(func)})";
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
            return type switch
            {
                CType.Char => sig.Ptr switch
                {
                    PtrOption.None => MapType(type),
                    PtrOption.Ptr => "Cstring",
                    PtrOption.ConstPtr => throw new InvalidExpressionException(sig + " cannot to convert to Julia type."),
                    _ => throw new InvalidExpressionException(sig + " cannot to convert to Julia type.")
                },
                _ => sig.Ptr switch
                {
                    PtrOption.None => MapType(type),
                    PtrOption.Ptr => "Array{" + MapType(type) + ", 1}",
                    PtrOption.ConstPtr => throw new InvalidExpressionException(sig + " cannot to convert to Julia type."),
                    _ => throw new InvalidExpressionException(sig + " cannot to convert to Julia type.")
                }
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
                    PtrOption.ConstPtr => "Cstring",
                    _ => throw new InvalidExpressionException(sig + " cannot to convert to Julia type.")
                },
                _ => sig.Ptr switch
                {
                    PtrOption.None => MapType(type),
                    PtrOption.ConstPtr => "Ptr{" + MapType(type) + "}",
                    PtrOption.Ptr => "Ref{" + MapType(type) + "}",
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
                CType.VoidPtr => "Ptr{Cvoid}",
                CType.Bool => "Bool",
                CType.Char => "UInt8",
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
