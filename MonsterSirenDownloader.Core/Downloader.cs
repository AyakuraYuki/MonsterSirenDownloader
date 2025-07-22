using System.Collections.Concurrent;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using MonsterSirenDownloader.Core.Model;

namespace MonsterSirenDownloader.Core;

public partial class MonsterSiren
{
    private const string SaveTo = "./monster-siren";

    private readonly int _maxConcurrency;
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentQueue<DownloadTask> _queue;

    private async Task<Album?> _fetchAlbumSongs(Album? album)
    {
        if (album == null || !album.IsValid()) return null;
        if (album.Songs.Count == 0) return null;

        for (var i = 0; i < album.Songs.Count; i++)
        {
            var cid = album.Songs[i]?.Cid ?? "";
            album.Songs[i] = await Song(cid);
        }

        return album;
    }

    private async Task _downloadAlbum(Album album, List<DownloadTask> tasks)
    {
        var completionTasks = new List<Task<bool>>();

        _queue.Clear();

        foreach (var task in tasks)
        {
            _queue.Enqueue(task);
            completionTasks.Add(task.CompletionSource.Task);
        }

        _ = Task.Run(_processTasksAsync);

        await Task.WhenAll(completionTasks);
    }

    private async Task _processTasksAsync()
    {
        var activeTasks = new List<Task>();

        while (_queue.TryDequeue(out var downloadTask) || activeTasks.Count > 0)
        {
            var task = _executeDownloadTaskAsync(downloadTask);
            activeTasks.Add(task);
            if (activeTasks.Count < _maxConcurrency) continue;

            if (activeTasks.Count <= 0) continue;
            var completedTask = await Task.WhenAny(activeTasks);
            activeTasks.Remove(completedTask);
        }
    }

    private async Task _executeDownloadTaskAsync(DownloadTask? downloadTask)
    {
        if (downloadTask == null) return;

        await _semaphore.WaitAsync();

        try
        {
            await _downloadSongsAsync(downloadTask.Song, downloadTask.TrackNo, downloadTask.Path);
            downloadTask.CompletionSource.SetResult(true);
        }
        catch (Exception e)
        {
            _logger.LogWarning("[_executeDownloadTaskAsync] error when executing download task: {Err}", e.Message);
            downloadTask.CompletionSource.SetResult(false);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task _downloadSongsAsync(Song song, int trackNo, string path)
    {
        var ext = Path.GetExtension(song.SourceUrl);
        var name = song.SafetyFilename();
        var songName = $"{trackNo:00}.{name}{ext}";
        var lyricName = $"{trackNo:00}.{name}.lrc";
        if (!string.IsNullOrEmpty(song.SourceUrl)) await _downloadAsync(song.SourceUrl, path, songName);
        if (!string.IsNullOrEmpty(song.LyricUrl)) await _downloadAsync(song.LyricUrl, path, lyricName);
    }

    // private async Task _downloadAsync(string link, string path, string name)
    // {
    //     var dst = Path.GetFullPath(Path.Combine(path, name));
    //     if (Path.Exists(dst)) return;
    //
    //     var tempDst = $"{dst}.tmp";
    //     File.Delete(tempDst);
    //
    //     await using var stream = await _sharedClient.GetStreamAsync(link);
    //     await using var fileStream = new FileStream(tempDst, FileMode.Create);
    //     var buffer = new byte[64 * 1024];
    //     int byteRead;
    //     while ((byteRead = stream.Read(buffer)) > 0)
    //         fileStream.Write(buffer, 0, byteRead);
    //
    //     File.Move(sourceFileName: tempDst, destFileName: dst);
    // }

    private async Task _downloadAsync(string link, string path, string name)
    {
        var dst = Path.GetFullPath(Path.Combine(path, name));
        if (Path.Exists(dst)) return;

        var tempDst = $"{dst}.tmp";
        long pos = 0;
        var fileInfo = new FileInfo(tempDst);
        if (fileInfo.Exists)
        {
            pos = fileInfo.Length;
            if (pos >= _getFileSize(link)) return;
        }

        await using var fileStream = new FileStream(tempDst, pos == 0 ? FileMode.Create : FileMode.Append);

        var request = new HttpRequestMessage(HttpMethod.Get, link);
        request.Headers.Range = new RangeHeaderValue(pos, null);

        using var response = await _sharedClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        var contentLength = response.Content.Headers.ContentLength;
        if (!contentLength.HasValue)
        {
            _logger.LogWarning("[_downloadAsync] link {Link} did not provide the Content-Length.", link);
            return;
        }

        var total = contentLength.Value + pos;

        await using var stream = await response.Content.ReadAsStreamAsync();
        await stream.CopyToAsync(fileStream);

        if (fileStream.Length != total)
        {
            fileInfo.Refresh();
            if (fileInfo.Length < total)
            {
                _logger.LogWarning("[_downloadAsync] file {TempDst} was not downloaded correctly.", tempDst);
                return;
            }
        }

        File.Move(sourceFileName: tempDst, destFileName: dst);
    }

    /// <summary>
    /// 透传路径参数的同时，保证路径目录一定存在
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static string _assureDirectory(string path)
    {
        path = Path.GetFullPath(path);
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>
    /// 保存专辑信息
    /// </summary>
    /// <param name="album">专辑对象</param>
    /// <param name="saveTo">保存路径</param>
    private static void _saveInfoFile(Album album, string saveTo)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"专辑名称：{album.Name}");
        builder.AppendLine($"专辑属于：{album.Belong}");
        builder.AppendLine($"专辑作者：{string.Join("、", album.Artistes ?? [])}");
        builder.AppendLine("专辑介绍：");
        builder.AppendLine(album.Intro);
        builder.AppendLine("");
        builder.AppendLine("");

        foreach (var (index, song) in album.Songs?.Select((s, i) => (i, s)) ?? [])
        {
            if (song == null || !song.IsValid())
            {
                builder.AppendLine($"- {index + 1:00}. <unknown: missing data>");
                continue;
            }

            builder.AppendLine($"- {index + 1:00}. {song.Name}");

            if ((song.Artists?.Length ?? 0) > 0)
                builder.AppendLine($"  作者：{string.Join("、", song.Artists ?? [])}");
            else
                builder.AppendLine($"  作者：{string.Join("、", song.Artistes ?? [])}");
        }

        File.WriteAllText(saveTo, builder.ToString().Trim());
    }

    /// <summary>
    /// 获取链接关联文件的大小，取自 Content-Length
    /// </summary>
    /// <param name="link"></param>
    /// <returns></returns>
    private long _getFileSize(string link)
    {
        using var response = _sharedClient.Send(new HttpRequestMessage(HttpMethod.Head, link));
        var contentLength = response.Content.Headers.ContentLength;
        return contentLength ?? 0;
    }
}

internal class DownloadTask(Song song, int trackNo, string path)
{
    public Song Song { get; } = song;
    public int TrackNo { get; } = trackNo;
    public string Path { get; } = path;
    public TaskCompletionSource<bool> CompletionSource { get; } = new();
}