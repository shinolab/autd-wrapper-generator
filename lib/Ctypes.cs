namespace autd_wrapper_generator.lib
{
    internal enum CType
    {
        None,
        Void,
        Bool,
        Char,
        Int8,
        Uint8,
        Int16,
        Uint16,
        Int32,
        Int64,
        UInt32,
        UInt64,
        Float32,
        Float64
    }

    internal enum PtrOption
    {
        None,
        Ptr,
        PtrPtr
    }

    internal record TypeSignature
    {
        internal CType Type { get; }
        internal PtrOption Ptr { get; }

        internal TypeSignature(CType type, PtrOption ptr) => (Type, Ptr) = (type, ptr);

        public override string ToString()
        {
            var ptr = Ptr == PtrOption.None ? "" : Ptr.ToString();
            return $"{Type}{ptr}";
        }

        internal static readonly TypeSignature None = new(CType.Void, PtrOption.None);
    }

    internal static class CTypeGen
    {
        internal static TypeSignature From(string str)
        {
            str = str.Trim();
            var baseStr = str.TrimEnd('*');
            var ptrAttrs = str.Length - baseStr.Length;

            var ptrOpt = ptrAttrs switch
            {
                1 => PtrOption.Ptr,
                2 => PtrOption.PtrPtr,
                _ => PtrOption.None
            };

            var type = baseStr switch
            {
                "void" => CType.Void,
                "bool" => CType.Bool,
                "char" => CType.Char,
                "int8_t" => CType.Int8,
                "uint8_t" => CType.Uint8,
                "int16_t" => CType.Int16,
                "uint16_t" => CType.Uint16,
                "int32_t" => CType.Int32,
                "int64_t" => CType.Int64,
                "uint32_t" => CType.UInt32,
                "uint64_t" => CType.UInt64,
                "float" => CType.Float32,
                "double" => CType.Float64,
                _ => CType.None
            };

            return new TypeSignature(type, ptrOpt);
        }
    }

}
