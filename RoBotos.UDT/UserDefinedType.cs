using RoBotos.UDT.Fields;

namespace RoBotos.UDT;

public sealed record UserDefinedType(string Name, string Version, ImmutableArray<UdtField> Fields, string Comment) : StructField(Name, Fields, Comment)
{
    public UserDefinedType(string name, string version, IEnumerable<UdtField> fields, string comment)
        : this(name, version, [.. fields], comment) { }
}