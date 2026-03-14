using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace IgnaviorLauncher.Services;

public class DownloadService
{
    private readonly HttpClient client;

    public DownloadService()
    {
        client = new();
    }

    public async Task DownloadFileAsync(string url, string dest)
    {
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using FileStream fileStream = new(dest, FileMode.Create, FileAccess.Write, FileShare.None);
        await response.Content.CopyToAsync(fileStream);
    }
}