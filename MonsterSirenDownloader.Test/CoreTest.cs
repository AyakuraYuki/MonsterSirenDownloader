using System.Text.Json;
using Microsoft.Extensions.Logging;
using MonsterSirenDownloader.Core;
using MonsterSirenDownloader.Core.Helper;

namespace MonsterSirenDownloader.Test;

public class CoreTest
{
    private ILogger<CoreTest> logger;
    private MonsterSiren instance;

    [SetUp]
    public void Setup()
    {
        instance = new MonsterSiren();

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
        }));
        logger = loggerFactory.CreateLogger<CoreTest>();
    }

    [Test]
    public async Task TestApiSongs()
    {
        var (songs, autoplay) = await instance.Songs();
        using (logger.BeginScope("[TestApiSongs]"))
        {
            logger.LogInformation("autoplay: {Autoplay}", autoplay);
            logger.LogInformation("songs: {Songs}", JsonSerializer.Serialize(songs, Json.Pretty));
        }
    }

    [Test]
    public async Task TestApiSong()
    {
        var song = await instance.Song("697699"); // Grow on My Time
        using (logger.BeginScope("[TestApiSong]"))
        {
            logger.LogInformation("song: {Song}", JsonSerializer.Serialize(song, Json.Pretty));
        }
    }

    [Test]
    public async Task TestApiAlbums()
    {
        var albums = await instance.Albums();
        using (logger.BeginScope("[TestApiAlbums]"))
        {
            logger.LogInformation("albums: {Albums}", JsonSerializer.Serialize(albums, Json.Pretty));
        }
    }

    [Test]
    public async Task TestApiAlbum()
    {
        var album = await instance.Album("1010"); // Grow on My Time
        using (logger.BeginScope("[TestApiAlbum]"))
        {
            logger.LogInformation("album: {Album}", JsonSerializer.Serialize(album, Json.Pretty));
        }
    }

    [Test]
    public async Task TestApiAlbumWithSongs()
    {
        var album = await instance.AlbumWithSongs("6660"); // Warm and Small Light
        using (logger.BeginScope("[TestApiAlbumWithSongs]"))
        {
            logger.LogInformation("album: {Album}", JsonSerializer.Serialize(album, Json.Pretty));
        }
    }

    [Test]
    public async Task TestDownloadTracks()
    {
        await instance.DownloadTracks();
    }
}