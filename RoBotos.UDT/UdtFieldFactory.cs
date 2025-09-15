using RoBotos.UDT.Fields;

namespace RoBotos.UDT;

public static class UdtFieldFactory
{
    public static PrimitiveField CreateSimple(ReadOnlySpan<char> name, ReadOnlySpan<char> typeName, ReadOnlySpan<char> comment, ReadOnlySpan<char> defaultValue)
        => CreateSimple(name.Trim().ToString(), UdtPrimitiveType.Get(typeName), comment.Trim().ToString(), defaultValue.Trim().ToString());
    public static PrimitiveField CreateSimple(string name, UdtPrimitiveType type, string comment = "", string defaultValue = "")
    {
        return type.Name switch
        {
            UdtPrimitiveType.ID.String => throw new InvalidOperationException("Cannot create StringEntry without Length"),
            _ => new PrimitiveField(name, type, comment, defaultValue),
        };
    }

    public static StringField CreateString(ReadOnlySpan<char> name, int length, ReadOnlySpan<char> comment)
        => CreateString(name.Trim().ToString(), length, comment.Trim().ToString());

    public static StringField CreateString(string name, int length, string comment = "")
        => new(name, length, comment);

    public static ArrayField CreateArray(ReadOnlySpan<char> name, ReadOnlySpan<char> typeName, ReadOnlySpan<char> range, ReadOnlySpan<char> comment, ReadOnlySpan<char> defaultValue)
        => CreateArray(name.Trim().ToString(), UdtPrimitiveType.Get(typeName.Trim()), ParseRange(range), defaultValue.Trim().ToString(), comment.Trim().ToString());
    public static ArrayField CreateArray(string name, UdtPrimitiveType dataType, Range range, string comment = "", string defaultValue = "")
        => new(name, dataType, range, comment, defaultValue);



    static Range ParseRange(ReadOnlySpan<char> range)
    {
        range = range.Trim();
        if (range.IsEmpty)
        {
            throw new ArgumentException("Range cannot be empty", nameof(range));
        }

        int delimiterIndex = range.IndexOf("..");
        if (delimiterIndex == -1)
        {
            throw new FormatException("Range must be in format 'start..end'");
        }

        var startPart = range[..delimiterIndex];
        var endPart = range[(delimiterIndex + 2)..];

        if (!int.TryParse(startPart, out var start))
        {
            throw new FormatException("Start of the range is not a valid integer");
        }

        if (!int.TryParse(endPart, out var end))
        {
            throw new FormatException("End of the range is not a valid integer");
        }

        return new Range(start, end);
    }
}