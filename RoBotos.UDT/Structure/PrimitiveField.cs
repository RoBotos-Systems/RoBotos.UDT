namespace RoBotos.UDT.Structure;

public record PrimitiveField(string Name, UdtPrimitiveType Type, string Comment, string DefaultValue = "") : AtomicField(Name, Comment)
{
    public override int GetSize() => Type.ByteSize;
}
