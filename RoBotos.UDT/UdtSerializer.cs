using System.IO;
using System.Text;
using RoBotos.UDT.Fields;

namespace RoBotos.UDT;

public static class UdtSerializer
{
    public static Result<UserDefinedType, string> Deserialize(FileInfo fileInfo)
    {
        if (!fileInfo.Exists)
        {
            return $"Could not find {fileInfo}";
        }

        using var stream = fileInfo.OpenText();
        return Deserialize(stream, fileInfo.Directory!.FullName);
    }

    public static Result<UserDefinedType, string> Deserialize(string path)
    {
        if (!File.Exists(path))
        {
            return $"Could not find {path}";
        }

        using var stream = File.OpenText(path);
        return Deserialize(stream, Path.GetDirectoryName(path)!);
    }

    public static Result<UserDefinedType, string> Deserialize(StreamReader stream, string referenceDirectory)
    {
        var header = stream.ReadLine();

        if (header is null || !header.StartsWith("TYPE"))
        {
            return $"file is not a valid UDT type";
        }

        var span = header.AsSpan();

        var nameIndex = span.IndexOf('"') + 1;
        var nameEndIndex = span[nameIndex..].IndexOf('"') + nameIndex;
        var commentIndex = span.IndexOf("//");
        var name = span[nameIndex..nameEndIndex].ToString();
        if (name.Contains('.')) return $"'{name}': names cannot contain '.'";
        var comment = commentIndex > 0 ? span[commentIndex..].ToString() : string.Empty;

        var version = stream.ReadLine()!;
        if (!version.StartsWith("version", StringComparison.OrdinalIgnoreCase))
        {
            version = stream.ReadLine()!; // skip meta data
        }

        stream.ReadLine(); // skip STRUCT

        Result<IEnumerable<UdtField>, string> GetEntries()
        {
            var list = new List<UdtField>();
            while (stream.ReadLine() is string line)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var span = line.AsSpan().Trim();
                if (span.SequenceEqual("END_STRUCT;"))
                {
                    return list.AsReadOnly();
                }

                if (!ParseEntry(span).Branch(out var entry, out var error))
                {
                    return error;
                }

                list.Add(entry);
            }

            return "struct never ended";
        }

        if (!GetEntries().Branch(out var entries, out var error))
        {
            return error;
        }

        return new UserDefinedType(name, version[FirstDigitIndex(version)..].Trim(), entries, comment);

        Result<UdtField, string> ParseEntry(ReadOnlySpan<char> span)
        {
            var settingsStartIndex = span.IndexOf('{');
            var settingsEndIndex = span.IndexOf('}');

            int separatorIndex;
            int lineEndIndex;
            if (settingsEndIndex > 0)
            {
                separatorIndex = span[settingsEndIndex..].IndexOf(':') + settingsEndIndex;
                lineEndIndex = span[settingsEndIndex..].IndexOf(';') + settingsEndIndex;
            }
            else
            {
                separatorIndex = span.IndexOf(':');
                lineEndIndex = span.IndexOf(';');
            }

            var nameEndIndex = settingsStartIndex > 0 ? settingsStartIndex : separatorIndex;

            var commentIndex = span.IndexOf("//");
            if (lineEndIndex == -1)
            {
                lineEndIndex = commentIndex > 0 ? commentIndex : span.Length;
            }
            var name = span[..nameEndIndex].Trim();
            if (name.Contains('.')) return $"'{name}': names cannot contain '.'";
            var type = span[(separatorIndex + 1)..lineEndIndex].Trim();

            var defaultValueIndex = type.IndexOf(":=");
            var defaultValue = defaultValueIndex > 0 ? type[(defaultValueIndex + 2)..].Trim() : [];
            type = defaultValueIndex > 0 ? type[..defaultValueIndex].Trim() : type;

            var comment = commentIndex > 0 ? span[(commentIndex + 2)..].Trim() : [];

            if (type.StartsWith('"'))
            {
                type = type[1..^1].Trim();
                // earlier those files ended in .awl, so if there is no udt we look for an awl, but if that does not exist we use the udt path, to trigger the correct error message
                var udt = Path.Join(referenceDirectory, $"{type}.udt");
                if (!Path.Exists(udt))
                {
                    var awl = Path.Join(referenceDirectory, $"{type}.awl");
                    if (Path.Exists(awl))
                    {
                        udt = awl;
                    }
                }
                return Deserialize(udt).Require<UdtField>(error: "unreachable: UserDefinedType inherits from UdtField");
            }
            else if (type.Equals("struct", StringComparison.OrdinalIgnoreCase))
            {
                if (!GetEntries().Branch(out var entries, out var error))
                {
                    return error;
                }
                return new StructField(name.ToString(), entries, comment.ToString());
            }
            else if (type.StartsWith("array", StringComparison.OrdinalIgnoreCase))
            {
                var rangeStartIndex = type.IndexOf('[') + 1;
                var rangeEndIndex = type.IndexOf(']');
                var range = type[rangeStartIndex..rangeEndIndex];

                var typeStartIndex = type.IndexOf(" of ", StringComparison.OrdinalIgnoreCase) + 4;
                var typeEndIndex = type[typeStartIndex..].IndexOf(' ');
                type = typeEndIndex > 0 ? type[typeStartIndex..(typeEndIndex + typeStartIndex)] : type[typeStartIndex..];
                return UdtFieldFactory.CreateArray(name, type, range, defaultValue, comment);
            }
            else if (type.StartsWith("string", StringComparison.OrdinalIgnoreCase))
            {
                return UdtFieldFactory.CreateString(name, int.Parse(type[(type.IndexOf('[') + 1)..type.IndexOf(']')]), comment);
            }
            else
            {
                return UdtFieldFactory.CreateSimple(name, type, comment, defaultValue);
            }
        }
    }


    public static string Serialize(UserDefinedType structure)
    {
        var sb = new StringBuilder();
        sb.Append($"TYPE \"{structure.Name}\"");
        AppendComment(structure.Comment).AppendLine();

        sb.AppendLine($"VERSION : {structure.Version}");
        sb.AppendLine("\tSTRUCT");
        AppendEntries(structure.Fields, 2);

        sb.Append("\tEND_STRUCT;\n\nEND_TYPE\n");

        return sb.ToString();

        void AppendEntries(IEnumerable<UdtField> entries, int tabs)
        {
            foreach (var entry in entries)
            {
                Indent(tabs);

                _ = entry switch
                {
                    PrimitiveField simpleEntry => AppendSimpleEntry(simpleEntry),
                    StringField stringEntry => AppendStringEntry(stringEntry),
                    StructField structEntry => AppendStructEntry(structEntry, tabs),
                    ArrayField arrayEntry => AppendArrayEntry(arrayEntry),
                    _ => throw new NotImplementedException($"Cannot serialize {entry.GetType().Name} to UDT"),
                };
            }
        }

        StringBuilder AppendSimpleEntry(PrimitiveField simpleEntry)
        {
            sb.Append($"{simpleEntry.Name} : ");
            AppendType(simpleEntry.Type, simpleEntry.DefaultValue);
            return AppendComment(simpleEntry.Comment).AppendLine();
        }

        StringBuilder AppendStringEntry(StringField stringEntry)
        {
            sb.Append($"{stringEntry.Name} : STRING [{stringEntry.Length}];");
            return AppendComment(stringEntry.Comment).AppendLine();
        }

        StringBuilder AppendStructEntry(StructField structEntry, int tabs)
        {
            sb.Append($"{structEntry.Name} : Struct");
            AppendComment(structEntry.Comment).AppendLine();
            AppendEntries(structEntry.Fields, tabs + 1);
            return Indent(tabs).AppendLine("END_STRUCT;");
        }

        StringBuilder AppendArrayEntry(ArrayField arrayEntry)
        {
            sb.Append($"{arrayEntry.Name} : Array[{arrayEntry.Range}] of ");
            AppendType(arrayEntry.Type, arrayEntry.DefaultValue);
            return AppendComment(arrayEntry.Comment).AppendLine();
        }

        StringBuilder AppendType(UdtPrimitiveType type, string defaultValue = "")
        {
            if (string.IsNullOrWhiteSpace(defaultValue))
            {
                sb.Append($"{type.Name};");
            }
            else
            {
                sb.Append($"{type.Name} := {defaultValue};");
            }

            return sb;
        }


        StringBuilder AppendComment(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
                return sb;
            return sb.Append("   // ").Append(comment);
        }

        StringBuilder Indent(int tabs) => sb.AppendRepeated("    ", tabs);
    }

    private static int FirstDigitIndex(ReadOnlySpan<char> s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            if (char.IsDigit(s[i]))
            {
                return i;
            }
        }
        return s.Length;
    }

    private static StringBuilder AppendRepeated(this StringBuilder builder, string value, int count)
    {
        for (int i = 0; i < count; i++)
        {
            builder.Append(value);
        }
        return builder;
    }
}
