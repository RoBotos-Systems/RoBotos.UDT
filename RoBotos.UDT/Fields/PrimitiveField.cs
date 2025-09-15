namespace RoBotos.UDT.Fields;

public record PrimitiveField(string Name, UdtPrimitiveType Type, string Comment, string DefaultValue = "") : AtomicField(Name, Comment)
{
    public override int GetSize() => Type.ByteSize;
}
