using System.Text.Json;
using Microsoft.Extensions.Logging;
using MonsterSirenDownloader.Core;
using MonsterSirenDownloader.Core.Helper;

namespace MonsterSirenDownloader.Test;

public class CoreTest
{
    private MonsterSiren _instance;
    private ILogger<CoreTest> _logger;

    [SetUp]
    public void Setup()
    {
        _instance = new MonsterSiren();

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
        }));
        _logger = loggerFactory.CreateLogger<CoreTest>();
    }

    [Test]
    public async Task TestApiSongs()
    {
        var (songs, autoplay) = await _instance.Songs();
        _logger.LogInformation("autoplay: {Autoplay}", autoplay);
        _logger.LogInformation("songs: {Songs}", JsonSerializer.Serialize(songs, Json.Pretty));
    }

    [Test]
    public async Task TestApiSong()
    {
        var song = await _instance.Song("697699"); // Grow on My Time
        _logger.LogInformation("song: {Song}", JsonSerializer.Serialize(song, Json.Pretty));

        song = await _instance.Song("1111"); // <unknown>
        _logger.LogInformation("song: {Song}", JsonSerializer.Serialize(song, Json.Pretty));
    }

    [Test]
    public async Task TestApiAlbums()
    {
        var albums = await _instance.Albums();
        _logger.LogInformation("albums: {Albums}", JsonSerializer.Serialize(albums, Json.Pretty));
    }

    [Test]
    public async Task TestApiAlbum()
    {
        var album = await _instance.Album("1010"); // Grow on My Time
        _logger.LogInformation("album: {Album}", JsonSerializer.Serialize(album, Json.Pretty));
    }

    [Test]
    public async Task TestApiAlbumWithSongs()
    {
        var album = await _instance.AlbumWithSongs("6660"); // Warm and Small Light
        _logger.LogInformation("album: {Album}", JsonSerializer.Serialize(album, Json.Pretty));
    }

    // [Test]
    // public async Task TestDownloadTracks()
    // {
    //     await instance.DownloadTracks();
    // }
}