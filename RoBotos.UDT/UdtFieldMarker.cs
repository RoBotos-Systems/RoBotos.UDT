using System.Text.Json;
using System.Text.Json.Serialization;
using Ametrin.Optional.Parsing;

namespace RoBotos.UDT;

[GenerateISpanParsable]
public sealed partial class UdtFieldMarker(ImmutableList<string> ids) : IEquatable<UdtFieldMarker>, IOptionSpanParsable<UdtFieldMarker>
{
#if NET10_0_OR_GREATER
    public static UdtFieldMarker Self => field ??= new([]);
#else
    public static readonly UdtFieldMarker Self = new([]);
#endif
    public static UdtFieldMarker Single(string name) => new([name]);

    public ImmutableList<string> IDs { get; } = ids;
    public bool IsSelf => IDs.Count is 0;

    public UdtFieldMarker Nest(string fieldName) => new(IDs.Add(fieldName));
    public UdtFieldMarker PopRoot() => new(IDs.RemoveAt(0));

    public override string ToString() => ToString('.');
    public string ToString(char separator) => string.Join(separator, IDs); // already short circuits when Count == 0 or 1

    public bool Equals(UdtFieldMarker? other) => other is not null && IDs.SequenceEqual(other.IDs);
    public override bool Equals(object? obj) => Equals(obj as UdtFieldMarker);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        IDs.ForEach(hash.Add);
        return hash.ToHashCode();
    }

    public static Option<UdtFieldMarker> TryParse(ReadOnlySpan<char> span, IFormatProvider? provider = null)
    {
        if (span.IsEmpty) return Self;

        var ids = ImmutableList<string>.Empty.ToBuilder();
        foreach (var range in span.Split('.'))
        {
            ids.Add(new(span[range]));
        }
        if (ids.Count is 0) return default;
        return new UdtFieldMarker(ids.ToImmutable());
    }
}

public sealed class UdtFieldMarkerJsonConverter : JsonConverter<UdtFieldMarker>
{
    public static UdtFieldMarkerJsonConverter Instance { get; } = new();
    public override UdtFieldMarker? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return UdtFieldMarker.Parse(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, UdtFieldMarker value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

public readonly record struct Marked<T>(UdtFieldMarker Marker, T Field);