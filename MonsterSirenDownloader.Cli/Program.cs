// See https://aka.ms/new-console-template for more information

using MonsterSirenDownloader.Core;

var m = new MonsterSiren();
await m.DownloadTracks();
m.Dispose();