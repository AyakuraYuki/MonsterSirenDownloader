using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace MonsterSirenDownloader.Core.Helper;

public class Json
{
    public static readonly JsonSerializerOptions Compressed = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public static readonly JsonSerializerOptions Pretty = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        WriteIndented = true,
        IndentSize = 4
    };
}