/*
 * File: PythonCodeGen.cs
 * Project: generators
 * Created Date: 21/05/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 23/05/2021
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
    internal class PythonCodeGen : ICodeGenerator
    {
        private readonly HashSet<string> _libs = new();

        public string GetCommentPrefix()
        {
            return "#";
        }

        public string GetFileHeader()
        {
            return @"
import threading
import ctypes
import os
from ctypes import c_void_p, c_bool, c_int, POINTER, c_double, c_char_p, c_ubyte, c_uint, c_ulong, c_ushort

class Singleton(type):
    _instances = {}
    _lock = threading.Lock()

    def __call__(cls, *args, **kwargs):
        if cls not in cls._instances:
            with cls._lock:
                if cls not in cls._instances:
                    cls._instances[cls] = super(Singleton, cls).__call__(*args, **kwargs)
        return cls._instances[cls]


class Nativemethods(metaclass=Singleton):
    def init_dll(self, bin_location, bin_ext):
        self._bin_location = bin_location
        self._bin_ext = bin_ext
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
                sb.AppendLine($"    def init_{NamingUtils.ToSnake(libName)}(self):");
                sb.AppendLine($"        if hasattr(self, '{NamingUtils.ToSnake(libName)}'):");
                sb.AppendLine($"            return");
                sb.AppendLine($"        self.{NamingUtils.ToSnake(libName)} = ctypes.CDLL(os.path.join(self._bin_location, self._bin_prefix + \"{libName.Replace("autd3capi", $"autd3capi_{Constants.Version}")}\" + self._bin_ext))");
            }

            sb.AppendLine($"        self.{NamingUtils.ToSnake(libName)}.{func.Name}.argtypes = [{GetArgs(func.ArgumentsList)}]");
            sb.AppendLine($"        self.{NamingUtils.ToSnake(libName)}.{func.Name}.restypes = [{MapType(func.ReturnTypeSignature)}]");
            return sb.ToString();
        }

        private static string GetArgs(IEnumerable<Argument> args)
        {
            return string.Join(", ", args.Select(arg => MapType(arg.TypeSignature)));
        }

        private static string MapType(TypeSignature sig)
        {
            var type = sig.Type;
            return type switch
            {
                CType.Char => sig.Ptr switch
                {
                    PtrOption.None => MapType(type),
                    PtrOption.Ptr => "c_char_p",
                    PtrOption.PtrPtr or _ => throw new InvalidExpressionException(sig + " cannot to convert to python type."),
                },
                CType.Void => sig.Ptr switch
                {
                    PtrOption.None => MapType(type),
                    PtrOption.Ptr => "c_void_p",
                    PtrOption.PtrPtr => $"POINTER(c_void_p)",
                    _ => throw new InvalidExpressionException(sig + " cannot to convert to python type.")
                },
                _ => sig.Ptr switch
                {
                    PtrOption.None => MapType(type),
                    PtrOption.Ptr => $"POINTER({MapType(type)})",
                    PtrOption.PtrPtr or _ => throw new InvalidExpressionException(sig + " cannot to convert to python type.")
                }
            };
        }

        private static string MapType(CType type)
        {
            return type switch
            {
                CType.None => throw new InvalidExpressionException(type + " cannot to convert to python type."),
                CType.Void => "None",
                CType.Bool => "c_bool",
                CType.Uint8 => "c_ubyte",
                CType.Int16 => "c_int",
                CType.Uint16 => "c_ushort",
                CType.Int32 => "c_int",
                CType.Int64 => "c_long",
                CType.UInt32 => "c_uint",
                CType.UInt64 => "c_ulong",
                CType.Float32 => "c_float",
                CType.Char => "c_char",
                CType.String => "c_char_p",
                CType.Int8 => "c_byte",
                CType.Float64 => "c_double",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}