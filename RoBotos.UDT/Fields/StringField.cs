namespace RoBotos.UDT.Fields;

/// <summary>
/// An atomic <see cref="UdtField"/> containing an ascii string
/// </summary>
public sealed record StringField(string Name, int Length, string Comment) : AtomicField(Name, Comment)
{
    public override int GetSize() => 2 + Length; // 1 byte maxLength + 1 byte actualLength + maxLength bytes ASCII characters
}
