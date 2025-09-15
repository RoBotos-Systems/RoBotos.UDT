namespace RoBotos.UDT.Structure;

public abstract record UdtField(string Name, string Comment)
{
    public abstract int GetSize();
}
