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

    public async Task<string> DownloadFileAsync(string url, string dest)
    {
        Directory.CreateDirectory(dest);
        string baseFileName = Path.GetFileName(new Uri(url).LocalPath);
        string extension = Path.GetExtension(baseFileName);
        if (string.IsNullOrEmpty(extension))
        {
            extension = ".tmp";
        }

        string fileName = Guid.NewGuid().ToString() + extension;
        string destPath = Path.Combine(dest, fileName);

        const int maxRetries = 3;
        int attempt = 0;
        while (attempt < maxRetries)
        {
            try
            {
                using var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                using FileStream fileStream = new(dest, FileMode.Create, FileAccess.Write, FileShare.None,
                    bufferSize: 8192, useAsync: true);
                await response.Content.CopyToAsync(fileStream);
                return destPath;
            }
            catch (UnauthorizedAccessException)
            when (attempt < maxRetries - 1)
            {
                await Task.Delay(1000 * (attempt + 1));
                attempt++;
            }
            catch (IOException)
            when (attempt < maxRetries - 1)
            {
                await Task.Delay(1000 * (attempt + 1));
                attempt++;
            }
        }

        throw new Exception($"Download failed (source {url}) after {maxRetries} attempts");
    }
}