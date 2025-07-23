using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace MonsterSirenDownloader.Core;

public partial class MonsterSiren
{
    private const string BaseUrl = "https://monster-siren.hypergryph.com";
    private const string Referer = "https://monster-siren.hypergryph.com/";
    private readonly ILogger<MonsterSiren> _logger;

    private readonly HttpClient _sharedClient;

    public MonsterSiren()
    {
        _sharedClient = new HttpClient();
        _sharedClient.BaseAddress = new Uri(BaseUrl);
        _sharedClient.Timeout = TimeSpan.FromMinutes(20);
        _sharedClient.DefaultRequestHeaders.Referrer = new Uri(Referer);
        _sharedClient.DefaultRequestHeaders.Add("Accept", "*/*");
        _sharedClient.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9,ja;q=0.8,en;q=0.7,en-GB;q=0.6,en-US;q=0.5");
        _sharedClient.DefaultRequestHeaders.Add("User-Agent", $".Net/{Environment.Version} MonsterSirenDownloader/1.0.0");

        _maxConcurrency = 3;
        _semaphore = new SemaphoreSlim(_maxConcurrency, _maxConcurrency);
        _queue = new ConcurrentQueue<DownloadTask>();

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
        }));
        _logger = loggerFactory.CreateLogger<MonsterSiren>();
    }

    /// <summary>
    ///     核心逻辑入口
    /// </summary>
    public async Task DownloadTracks()
    {
        var pwd = Directory.GetCurrentDirectory();
        var saveTo = _assureDirectory(Path.Combine(pwd, SaveTo));

        var albums = await Albums();

        foreach (var (index, value) in albums.Select((a, i) => (i, a)))
        {
            var album = await AlbumWithSongs(value.Cid);
            album = await _fetchAlbumSongs(album);
            if (album == null) continue;
            if ((album.Artistes?.Length ?? 0) == 0) album.Artistes = value.Artistes;

            var albumNo = albums.Length - index;
            var albumSaveTo = _assureDirectory(Path.Combine(saveTo, $"{albumNo:000} - {album.SafetyFilename()}"));

            // 保存专辑信息
            _saveInfoFile(album, Path.GetFullPath(Path.Combine(albumSaveTo, "info.txt")));

            var tasks = new List<DownloadTask>();
            for (var i = 0; i < album.Songs.Count; i++)
            {
                if (album.Songs[i] == null || !album.Songs[i]!.IsValid()) continue;
                tasks.Add(new DownloadTask(album.Songs[i]!, i + 1, albumSaveTo));
            }

            await _downloadAlbum(album, tasks);

            if (!string.IsNullOrEmpty(album.CoverUrl))
            {
                var ext = Path.GetExtension(album.CoverUrl);
                await _downloadAsync(album.CoverUrl, albumSaveTo, $"专辑封面{ext}");
            }

            if (!string.IsNullOrEmpty(album.CoverDeUrl))
            {
                var ext = Path.GetExtension(album.CoverDeUrl);
                await _downloadAsync(album.CoverDeUrl, albumSaveTo, $"封面{ext}");
            }
        }
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public void Dispose()
    {
        _sharedClient.Dispose();
        _semaphore.Dispose();
        _queue.Clear();
    }
}