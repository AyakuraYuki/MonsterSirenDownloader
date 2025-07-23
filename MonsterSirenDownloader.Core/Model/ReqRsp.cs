using System.Text.Json.Serialization;

namespace MonsterSirenDownloader.Core.Model;

public class SongsRsp
{
    [JsonPropertyName("code")] public int Code { get; init; }
    [JsonPropertyName("msg")] public string Msg { get; init; } = "";
    [JsonPropertyName("data")] public SongsRspBody Data { get; init; } = new();
}

public class SongsRspBody
{
    [JsonPropertyName("list")] public Song[] Songs { get; init; } = [];
    [JsonPropertyName("autoplay")] public string Autoplay { get; init; } = "";
}

public class SongRsp
{
    [JsonPropertyName("code")] public int Code { get; init; } = 0;
    [JsonPropertyName("msg")] public string Msg { get; init; } = "";
    [JsonPropertyName("data")] public Song? Data { get; init; }
}

public class AlbumsRsp
{
    [JsonPropertyName("code")] public int Code { get; init; } = 0;
    [JsonPropertyName("msg")] public string Msg { get; init; } = "";
    [JsonPropertyName("data")] public Album[] Data { get; init; } = [];
}

public class AlbumRsp
{
    [JsonPropertyName("code")] public int Code { get; init; } = 0;
    [JsonPropertyName("msg")] public string Msg { get; init; } = "";
    [JsonPropertyName("data")] public Album? Data { get; init; }
}