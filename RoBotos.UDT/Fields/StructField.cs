namespace RoBotos.UDT.Fields;

public record StructField(string Name, ImmutableArray<UdtField> Fields, string Comment) : CompoundField(Name, Comment), IEnumerable<UdtField>
{
    public StructField(string name, IEnumerable<UdtField> entries, string comment) : this(name, [.. entries], comment) { }

    public override IEnumerable<UdtField> EnumerateFields() => Fields;

    public override IEnumerable<Marked<AtomicField>> FlattenFields(UdtFieldMarker? prefix = null)
        => prefix is null
            ? Fields.Flatten(UdtFieldMarker.Single(Name))
            : Fields.Flatten(prefix.Nest(Name));

    public override int GetSize() => Fields.Sum(entry => entry.GetSize());

    /// <summary>
    /// calculates the total size of the struct in an S7 data stream 
    /// </summary>
    public int GetS7Size()
    {
        var sum = 0;
        var booleanCount = 0;

        foreach (var entry in Fields)
        {
            if (entry is PrimitiveField simpleEntry && simpleEntry.Type.Name == UdtPrimitiveType.ID.Bool)
            {
                booleanCount++;
            }
            else
            {
                AddBooleans();
                sum += entry is StructField structEntry ? structEntry.GetS7Size() : entry.GetSize();
            }
        }

        AddBooleans();

        return sum;

        void AddBooleans()
        {
            if (booleanCount <= 0)
            {
                return;
            }

            sum += (int)MathF.Ceiling(booleanCount / 8f) + 1;
            booleanCount = 0;
        }
    }

    public IEnumerator<UdtField> GetEnumerator() => Fields.AsEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
