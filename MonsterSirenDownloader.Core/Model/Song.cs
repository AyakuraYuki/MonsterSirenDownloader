using System.Text.Json.Serialization;

namespace MonsterSirenDownloader.Core.Model;

public class Song
{
    [JsonPropertyName("cid")] public string Cid { get; init; } = "";

    [JsonPropertyName("name")] public string Name { get; init; } = "";

    [JsonPropertyName("albumCid")] public string AlbumCid { get; init; } = null!;

    [JsonPropertyName("sourceUrl")] public string SourceUrl { get; init; } = null!;

    [JsonPropertyName("lyricUrl")] public string LyricUrl { get; init; } = null!;

    [JsonPropertyName("mvUrl")] public string MvUrl { get; init; } = null!;

    [JsonPropertyName("mvCoverUrl")] public string MvCoverUrl { get; init; } = null!;

    [JsonPropertyName("artists")] public string[]? Artists { get; init; }
    [JsonPropertyName("artistes")] public string[]? Artistes { get; init; }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Cid);
    }

    public string SafetyFilename()
    {
        var filename = string.Join('_', Name.Split(Path.GetInvalidFileNameChars()));
        return filename;
    }
}