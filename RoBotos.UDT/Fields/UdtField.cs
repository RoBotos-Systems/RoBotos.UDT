namespace RoBotos.UDT.Fields;

public abstract record UdtField(string Name, string Comment)
{
    public abstract int GetSize();
}
