namespace RoBotos.UDT.Fields;

public sealed record ArrayField(string Name, UdtPrimitiveType Type, Range Range, string Comment, string DefaultValue) : CompoundField(Name, Comment)
{
    public int StartIndex { get; } = !Range.Start.IsFromEnd ? Range.Start.Value : throw new ArgumentException("Cannot index from end with unknown length");
    public int EndIndex { get; } = !Range.End.IsFromEnd ? Range.End.Value + 1 : throw new ArgumentException("Cannot index from end with unknown length");
    public int Count => EndIndex - StartIndex;

    /// <summary>
    /// Enumerates all elements by creating a simple element with its index as name
    /// </summary>
    public override IEnumerable<UdtField> EnumerateFields()
    {
        for (var i = StartIndex; i < EndIndex; i++)
        {
            yield return new PrimitiveField(i.ToString(), Type, string.Empty, DefaultValue);
        }
    }

    /// <summary>
    /// Flattens its elements by creating <see cref="PrimitiveField"/>s named Index and resturns a <see cref="MarkedAtomicField"/>
    /// </summary>
    public override IEnumerable<Marked<AtomicField>> FlattenFields(UdtFieldMarker? prefix = null)
    {
        var root = prefix?.Nest(Name) ?? new([Name]);
        return EnumerateFields().Select(field => new Marked<AtomicField>(root.Nest(field.Name), (AtomicField)field));
    }

    public override int GetSize() => Type.ByteSize * Count;
}
