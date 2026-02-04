using RoBotos.UDT.Fields;

namespace RoBotos.UDT;

public static class UdtFieldHelper
{
    public static IEnumerable<Marked<AtomicField>> Flatten(this IEnumerable<UdtField> entries, UdtFieldMarker prefix)
        => entries.SelectMany(entry => entry switch
        {
            AtomicField atomic => [new(prefix.Nest(atomic.Name), atomic)],
            CompoundField nested => nested.FlattenFields(prefix.Nest(nested.Name)),
            _ => throw new UnreachableException(),
        });

    public static IEnumerable<Marked<AtomicField>> Flatten(this IEnumerable<UdtField> entries)
        => entries.Flatten(UdtFieldMarker.Self);

    public static IEnumerable<AtomicField> EnumerateFlat(this IEnumerable<UdtField> entries)
        => entries.SelectMany(entry => entry switch
        {
            AtomicField primitive => [primitive],
            ArrayField array => array.EnumerateFields().Cast<AtomicField>(),
            StructField @struct => @struct.Fields.EnumerateFlat(),
            _ => throw new UnreachableException(),
        });

    public static string GetSqlExpressDefinition(this AtomicField field)
        => $"[{field.Name}] {field.GetSqlExpressType()}";

    public static string GetSqlExpressDefinition(this Marked<AtomicField> field, char separator = '.')
        => $"[{field.Marker.ToString(separator)}] {field.Field.GetSqlExpressType()}";

    public static string GetSqlExpressType(this AtomicField field) => field switch
    {
        StringField sf => $"nvarchar({sf.Length})",
        PrimitiveField pf => pf.Type.GetSqlExpressType(),
        _ => throw new UnreachableException(),
    };

    public static string GetSqlExpressType(this UdtPrimitiveType type) => type.Name switch
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
        UdtPrimitiveType.ID.Real => "FLOAT(24)", // same as REAL
        UdtPrimitiveType.ID.LReal => "FLOAT(53)",
        UdtPrimitiveType.ID.DateTime => "DATETIME2", // DATETIME is obsolete
        _ => throw new UnreachableException($"SQL Express type for S7 {type.Name} unknown"),
    };
}
