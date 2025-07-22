using System.Text.Json.Serialization;
using MonsterSirenDownloader.Core.Helper;

namespace MonsterSirenDownloader.Core.Model;

public class Album
{
    [JsonPropertyName("cid")] public string Cid { get; init; } = "";

    [JsonPropertyName("name")] public string Name { get; init; } = "";

    [JsonPropertyName("intro")] public string Intro { get; init; } = null!;

    [JsonPropertyName("belong")] public string Belong { get; init; } = null!;

    [JsonPropertyName("coverUrl")] public string CoverUrl { get; init; } = null!;

    [JsonPropertyName("coverDeUrl")] public string CoverDeUrl { get; init; } = null!;

    [JsonPropertyName("artistes")] public string[]? Artistes { get; set; }

    [JsonPropertyName("songs")] public List<Song?> Songs { get; init; } = null!;

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Cid);
    }

    public string SafetyFilename()
    {
        var filename = string.Join('_', Name.Split(Path.GetInvalidFileNameChars()));
        filename = filename.Trim();
        filename = StringHelper.ReplaceTailDot(filename);
        return filename.Trim();
    }
}