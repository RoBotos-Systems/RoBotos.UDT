using System.Collections.Frozen;
using RoBotos.UDT.Fields;

namespace RoBotos.UDT;

public sealed record UserDefinedType(string Name, string Version, FrozenDictionary<string, UdtField> Fields, string Comment) : StructField(Name, Fields, Comment)
{
    public UserDefinedType(string name, string version, IEnumerable<UdtField> fields, string comment)
        : this(name, version, fields.ToFrozenDictionary(static f => f.Name), comment) { }
}