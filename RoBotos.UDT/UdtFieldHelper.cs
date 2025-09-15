using RoBotos.UDT.Structure;

namespace RoBotos.UDT;

public static class UdtFieldHelper
{
    public static IEnumerable<AtomicField> Flatten(this IEnumerable<UdtField> entries, string prefix, char separator = '_')
        => entries.SelectMany(entry => entry switch
        {
            AtomicField primitive => [primitive with { Name = $"{prefix}{separator}{primitive.Name}" }],
            CompoundField nested => nested.FlattenFields(prefix, separator),
            _ => throw new UnreachableException(),
        });

    public static IEnumerable<AtomicField> Flatten(this IEnumerable<UdtField> entries, char separator = '_')
        => entries.SelectMany(entry => entry switch
        {
            AtomicField primitive => [primitive],
            CompoundField nested => nested.FlattenFields(separator: separator),
            _ => throw new UnreachableException(),
        });

    public static IEnumerable<AtomicField> EnumerateFlat(this IEnumerable<UdtField> entries)
        => entries.SelectMany(entry => entry switch
        {
            AtomicField primitive => [primitive],
            ArrayField array => array.EnumerateFields().Cast<AtomicField>(),
            StructField @struct => @struct.Fields.EnumerateFlat(),
            _ => throw new UnreachableException(),
        });

    public static string GetSqlExpressDefinition(this AtomicField field)
        => $"{field.Name} {field.GetSqlExpressType()}";

    public static string GetSqlExpressType(this AtomicField field) => field switch
    {
        StringField sf => $"nvarchar({sf.Length})",
        PrimitiveField pf => pf.Type.Name switch
        {
            UdtPrimitiveType.ID.Bool => "BIT",
            UdtPrimitiveType.ID.Byte => "BINARY(1)",
            UdtPrimitiveType.ID.Word => "BINARY(2)",
            UdtPrimitiveType.ID.DWord => "BINARY(4)",
            UdtPrimitiveType.ID.LWord => "BINARY(8)",
            UdtPrimitiveType.ID.SInt => "TINYINT",
            UdtPrimitiveType.ID.Int => "SMALLINT",
            UdtPrimitiveType.ID.DInt => "INT",
            UdtPrimitiveType.ID.LInt => "BIGINT",
            UdtPrimitiveType.ID.Real => "FLOAT(24)", // FLOAT(24) is the same as REAL
            UdtPrimitiveType.ID.LReal => "FLOAT(53)",
            UdtPrimitiveType.ID.DateTime => "DATETIME2", // DATETIME is obsolete
            _ => throw new UnreachableException($"SQL Express type for S7 {pf.Type.Name} unknown"),
        },
        _ => throw new UnreachableException(),
    };
}
