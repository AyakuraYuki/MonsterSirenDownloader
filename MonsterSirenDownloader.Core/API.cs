using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using MonsterSirenDownloader.Core.Model;

namespace MonsterSirenDownloader.Core;

public partial class MonsterSiren
{
    /// <summary>
    /// 查询所有歌曲
    /// </summary>
    /// <returns>按时间从新到旧排序的所有歌曲的列表</returns>
    public async Task<(Song[], string)> Songs()
    {
        Song[] songs = [];
        var autoplay = "";

        using var response = await _sharedClient.GetAsync("/api/songs");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("[Songs] error status code: {ResponseStatusCode}", response.StatusCode);
            return (songs, autoplay);
        }

        var rsp = await response.Content.ReadFromJsonAsync<SongsRsp>();
        songs = rsp?.Data?.Songs ?? [];
        autoplay = rsp?.Data?.Autoplay ?? "";

        return (songs, autoplay);
    }

    /// <summary>
    /// 查询歌曲信息
    /// </summary>
    /// <param name="cid">歌曲ID，来自歌曲列表接口返回的cid参数</param>
    /// <returns>包含临时下载链接的歌曲信息</returns>
    public async Task<Song?> Song(string cid)
    {
        if (string.IsNullOrEmpty(cid)) return null;

        using var response = await _sharedClient.GetAsync($"/api/song/{cid}");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("[Song] error status code: {ResponseStatusCode}", response.StatusCode);
            return null;
        }

        var rsp = await response.Content.ReadFromJsonAsync<SongRsp>();
        return rsp?.Data ?? null;
    }

    /// <summary>
    /// 查询专辑列表
    /// </summary>
    /// <returns>按时间从新到旧排序的所有专辑的列表</returns>
    public async Task<Album[]> Albums()
    {
        using var response = await _sharedClient.GetAsync("/api/albums");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("[Albums] error status code: {ResponseStatusCode}", response.StatusCode);
            return [];
        }

        var rsp = await response.Content.ReadFromJsonAsync<AlbumsRsp>();
        return rsp?.Data ?? [];
    }

    /// <summary>
    /// 查询专辑信息
    /// </summary>
    /// <param name="cid">专辑ID，来自专辑列表接口返回的cid参数</param>
    /// <returns></returns>
    public async Task<Album?> Album(string cid)
    {
        if (string.IsNullOrEmpty(cid)) return null;

        using var response = await _sharedClient.GetAsync($"/api/album/{cid}/data");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("[Album] error status code: {ResponseStatusCode}", response.StatusCode);
            return null;
        }

        var rsp = await response.Content.ReadFromJsonAsync<AlbumRsp>();
        return rsp?.Data ?? null;
    }

    /// <summary>
    /// 查询专辑信息，包含歌曲列表
    /// </summary>
    /// <param name="cid">专辑ID，来自专辑列表接口返回的cid参数</param>
    /// <returns></returns>
    public async Task<Album?> AlbumWithSongs(string cid)
    {
        if (string.IsNullOrEmpty(cid)) return null;

        using var response = await _sharedClient.GetAsync($"/api/album/{cid}/detail");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("[Album] error status code: {ResponseStatusCode}", response.StatusCode);
            return null;
        }

        var rsp = await response.Content.ReadFromJsonAsync<AlbumRsp>();
        return rsp?.Data ?? null;
    }
}