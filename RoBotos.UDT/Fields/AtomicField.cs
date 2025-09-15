namespace RoBotos.UDT.Fields;

/// <summary>
/// A <see cref="UdtField"/> that can be stored in a single database cell
/// </summary>
public abstract record AtomicField(string Name, string Comment) : UdtField(Name, Comment);
