using System.Collections.Frozen;

namespace RoBotos.UDT;

public sealed record UdtPrimitiveType(string Name, Type CSharpType, int BitSize)
{
    public int ByteSize { get; } = (int) float.Ceiling(BitSize / 8f);

    private static readonly FrozenDictionary<string, UdtPrimitiveType> _registry;

    static UdtPrimitiveType()
    {
        UdtPrimitiveType[] types = [
            new UdtPrimitiveType(ID.Bool, typeof(bool), 1),

            new UdtPrimitiveType(ID.Byte, typeof(byte), 8),
            new UdtPrimitiveType(ID.Word, typeof(ushort), 16),
            new UdtPrimitiveType(ID.DWord, typeof(uint), 32),
            new UdtPrimitiveType(ID.LWord, typeof(ulong), 64),

            new UdtPrimitiveType(ID.SInt, typeof(sbyte), 8),
            new UdtPrimitiveType(ID.Int, typeof(short), 16),
            new UdtPrimitiveType(ID.DInt, typeof(int), 32),
            new UdtPrimitiveType(ID.LInt, typeof(long), 64),

            new UdtPrimitiveType(ID.Real, typeof(float), 32), 
            new UdtPrimitiveType(ID.LReal, typeof(double), 64),

            new UdtPrimitiveType(ID.DateTime, typeof(DateTime), 64),
        ];

        _registry = types.ToFrozenDictionary(static t => t.Name, StringComparer.OrdinalIgnoreCase);
    }

    public static UdtPrimitiveType Get(ReadOnlySpan<char> name)
    {
        var lookup = _registry.GetAlternateLookup<ReadOnlySpan<char>>();
        if(lookup.TryGetValue(name, out var result))
        {
            return result;
        }

        throw new NotImplementedException($"UDT Type {name} is not implemented!");
    }

    public static class ID
    {
        public const string Bool = "BOOL";
        public const string Byte = "BYTE";
        public const string Word = "WORD";
        public const string DWord = "DWORD";
        public const string LWord = "LWORD";
        public const string SInt = "SINT";
        public const string Int = "INT";
        public const string DInt = "DINT";
        public const string USInt = "USINT";
        public const string UInt = "UINT";
        public const string UDInt = "UDINT";
        public const string LInt = "LINT";
        public const string ULInt = "ULINT";
        public const string Real = "REAL";
        public const string LReal = "LREAL";
        //public const string S5Time = "S5TIME";
        //public const string Time = "TIME";
        //public const string LTime = "LTIME";
        public const string Char = "CHAR";
        public const string WChar = "WCHAR";
        public const string String = "STRING";
        public const string WString = "WSTRING";
        //public const string Date = "DATE";
        //public const string Tod = "TOD";
        //public const string LTod = "LTOD";
        public const string DateTime = "DATE_AND_TIME";
        //public const string LDT = "LDT";
        //public const string DTL = "DTL";
        //public const string Variant = "VARIANT";
    }
}
