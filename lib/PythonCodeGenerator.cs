using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace autd_wrapper_generator.lib
{
    internal class PythonCodeGenerator : ICodeGenerator
    {
        public string GetCommentPrefix()
        {
            return "#";
        }

        public string GetFileHeader()
        {
            return @"
import threading
import ctypes
from ctypes import c_void_p, c_bool, c_int, POINTER, c_float, c_long, c_char_p, c_ubyte, c_uint, c_ulong, c_ushort

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
    dll = None

    def init_dll(self, dlllocation):
        self.dll = ctypes.CDLL(dlllocation)
";
        }

        public string GetFileFooter()
        {
            return string.Empty;
        }

        public string GetFunctionDefinition(Function func)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"        self.dll.{func.Name}.argtypes = [{GetArgs(func.ArgumentsList)}]");
            sb.AppendLine($"        self.dll.{func.Name}.restypes = [{MapType(func.ReturnTypeSignature)}]");
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
                    PtrOption.Ptr or PtrOption.ConstPtr => "c_char_p",
                    _ => throw new InvalidExpressionException(sig + " cannot to convert to C# type.")
                },
                _ => sig.Ptr switch
                {
                    PtrOption.None => MapType(type),
                    PtrOption.Ptr or PtrOption.ConstPtr => $"POINTER({MapType(type)})",
                    _ => throw new InvalidExpressionException(sig + " cannot to convert to C# type.")
                }
            };
        }

        private static string MapType(CType type)
        {
            return type switch
            {
                CType.None => throw new InvalidExpressionException(type + " cannot to convert to C# type."),
                CType.Void => "None",
                CType.VoidPtr => "c_void_p",
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
                CType.Int8 => "c_byte",
                CType.Float64 => "c_double",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}
