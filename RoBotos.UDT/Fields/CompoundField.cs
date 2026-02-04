namespace RoBotos.UDT.Fields;

/// <summary>
/// Represents an <see cref="UdtField"/> composited out of other types.
/// <para/>
/// <see cref="StructField"/>
/// <see cref="ArrayField"/>
/// </summary>
public abstract record CompoundField(string Name, string Comment) : UdtField(Name, Comment)
{
    /// <summary>
    /// When implemented should enumerate all direct children
    /// </summary>
    public abstract IEnumerable<UdtField> EnumerateFields();

    /// <summary>
    /// When implemented should enumerate all children with their name changed to (prefix_)parentName_childName
    /// </summary>
    public abstract IEnumerable<Marked<AtomicField>> FlattenFields(UdtFieldMarker? prefix = null);
}
