using System.Collections.Frozen;

namespace RoBotos.UDT;

public sealed record UdtPrimitiveType(string Name, int BitSize, Type CSharpType)
{
    public int ByteSize { get; } = (int) float.Ceiling(BitSize / 8f);

    private static readonly FrozenDictionary<string, UdtPrimitiveType> _registry;

    static UdtPrimitiveType()
    {
        // based on https://www.spshaus.ch/files/inc/Downloads/Lernumgebung/Downloads/Allgemein/TIA_Portal_Uebersicht_Datentypen.pdf
        UdtPrimitiveType[] types = [
            new UdtPrimitiveType(ID.Bool, BitSize: 1, typeof(bool)),

            new UdtPrimitiveType(ID.Byte, BitSize: 8, typeof(byte)),
            new UdtPrimitiveType(ID.Word, BitSize: 16, typeof(ushort)),
            new UdtPrimitiveType(ID.DWord, BitSize: 32, typeof(uint)),
            new UdtPrimitiveType(ID.LWord, BitSize: 64, typeof(ulong)),

            new UdtPrimitiveType(ID.SInt, BitSize: 8, typeof(sbyte)),
            new UdtPrimitiveType(ID.USInt, BitSize: 8, typeof(byte)),
            new UdtPrimitiveType(ID.Int, BitSize: 16, typeof(short)),
            new UdtPrimitiveType(ID.UInt, BitSize: 16, typeof(ushort)),
            new UdtPrimitiveType(ID.DInt, BitSize: 32, typeof(int)),
            new UdtPrimitiveType(ID.UDInt, BitSize: 32, typeof(uint)),
            new UdtPrimitiveType(ID.LInt, BitSize: 64, typeof(long)),
            new UdtPrimitiveType(ID.ULInt, BitSize: 64, typeof(ulong)),

            new UdtPrimitiveType(ID.Real, BitSize: 32, typeof(float)), 
            new UdtPrimitiveType(ID.LReal, BitSize: 64, typeof(double)),

            new UdtPrimitiveType(ID.Time, BitSize: 32, typeof(TimeSpan)),
            new UdtPrimitiveType(ID.Tod, BitSize: 32, typeof(TimeOnly)),
            new UdtPrimitiveType(ID.Date, BitSize: 16, typeof(DateOnly)),

            // .NET DateTime covers year 1 to 10000, way more than any S7 date time format
            new UdtPrimitiveType(ID.DateTime, BitSize: 64, typeof(DateTime)),
            new UdtPrimitiveType(ID.LDT, BitSize: 64, typeof(DateTime)),
            new UdtPrimitiveType(ID.DTL, BitSize: 96, typeof(DateTime)),
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

        throw new NotImplementedException($"unkown udt primitive {name}");
    }

    public static class ID
    {
        public const string Bool = "Bool";
        public const string Byte = "Byte";
        public const string Word = "Word";
        public const string DWord = "DWord";
        public const string LWord = "LWord";
        public const string SInt = "SInt";
        public const string Int = "Int";
        public const string DInt = "DInt";
        public const string USInt = "USInt";
        public const string UInt = "UInt";
        public const string UDInt = "UDInt";
        public const string LInt = "LInt";
        public const string ULInt = "ULInt";
        public const string Real = "Real";
        public const string LReal = "LReal";
        //public const string S5Time = "S5Time";
        public const string Time = "Time";
        //public const string LTime = "LTime";
        public const string Char = "Char";
        public const string WChar = "WChar";
        public const string String = "String";
        public const string WString = "WString";
        public const string Date = "Date";
        public const string Tod = "TOD";
        //public const string LTod = "LTOD";
        public const string DateTime = "Date_And_Time";
        public const string LDT = "LDT";
        public const string DTL = "DTL";
        //public const string Variant = "VARIANT";
    }
}
