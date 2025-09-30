namespace RoBotos.UDT.Fields;

public sealed record ArrayField(string Name, UdtPrimitiveType Type, Range Range, string Comment, string DefaultValue) : CompoundField(Name, Comment)
{
    public int StartIndex { get; } = !Range.Start.IsFromEnd ? Range.Start.Value : throw new ArgumentException("Cannot index from end with unknown length");
    public int EndIndex { get; } = !Range.End.IsFromEnd ? Range.End.Value + 1 : throw new ArgumentException("Cannot index from end with unknown length");
    public int Count => EndIndex - StartIndex;

    /// <summary>
    /// Enumerates all elements by creating a simple element with its index as name
    /// </summary>
    public override IEnumerable<UdtField> EnumerateFields() => FlattenImpl(static i => i.ToString());

    /// <summary>
    /// Flattens its elements by creating a simple field named Name.Index
    /// </summary>
    public override IEnumerable<AtomicField> FlattenFields(string? prefix = null, char separator = '.')
    {
        return string.IsNullOrWhiteSpace(prefix) ? FlattenImpl(i => $"{Name}{separator}{i}") : FlattenImpl(i => $"{prefix}{separator}{Name}{separator}{i}");
    }

    private IEnumerable<AtomicField> FlattenImpl(Func<int, string> nameSupplier)
    {
        for (var i = StartIndex; i < EndIndex; i++)
        {
            yield return new PrimitiveField(nameSupplier(i), Type, string.Empty, DefaultValue);
        }
    }

    public override int GetSize() => Type.ByteSize * Count;
}
