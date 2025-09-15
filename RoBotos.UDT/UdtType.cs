using RoBotos.UDT.Fields;

namespace RoBotos.UDT;

public sealed record UdtType(string Name, string Version, ImmutableArray<UdtField> Fields, string Comment) : StructField(Name, Fields, Comment)
{
    public UdtType(string name, string version, IEnumerable<UdtField> fields, string comment)
        : this(name, version, [.. fields], comment) { }
}