using System.Diagnostics;
using System.IO.Compression;

namespace archivetool
{
    internal static class Tool
    {
        private static string? _downloadPath;
        private static string? _exePath;
        private const string DOWNLOADER_EXE_URL = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";
        private const string EXE_NAME = "yt-dlp.exe";

        private static async Task Main(string[] args)
        {
            try
            {
                _exePath = Path.Combine(Directory.GetCurrentDirectory(), "yt-dlp.exe");
                _downloadPath = Path.Combine(Directory.GetCurrentDirectory(), "downloads");
                if (!Directory.Exists(_downloadPath))
                {
                    Directory.CreateDirectory(_downloadPath);
                    Console.WriteLine("Download folder created.");
                }
                Cleanup();
                if (!File.Exists(_exePath))
                {
                    Console.WriteLine("yt-dlp not found, downloading..");
                    using (var http = new HttpClient())
                    {
                        var bytes = await http.GetByteArrayAsync(DOWNLOADER_EXE_URL);
                        await File.WriteAllBytesAsync(_exePath, bytes);
                    }
                    Console.WriteLine("yt-dlp downloaded successfully.");
                }
                else
                {
                    Console.WriteLine("yt-dlp found, checking for updates.");
                    Process.Start(new ProcessStartInfo(EXE_NAME, "-U") { UseShellExecute = true });
                }
                Console.WriteLine("Input an URL to archive.");
                CheckInput();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Main failed: {ex.Message}");
                throw;
            }
            finally
            {
                Console.ReadKey();
            }
        }

        private static void Cleanup()
        {
            try
            {
                var files = Directory.GetFiles(_downloadPath);
                if (files.Length == 0) return;
                foreach (var file in files)
                {
                    File.Delete(file);
                }
                Console.WriteLine($"{files.Length} media files deleted.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cleanup faild: {ex.Message}");
            }
        }

        private static void CheckInput()
        {
            try
            {
                var url = Console.ReadLine();
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    Console.WriteLine("Invalid URL.");
                    CheckInput();
                    return;
                }
                Console.WriteLine("URL is valid. Starting.");
                StartDownload(url);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CheckInput failed: {ex.Message}");
            }
        }

        private static void Archive(string name)
        {
            try
            {
                ZipFile.CreateFromDirectory(_downloadPath, Path.Combine(Directory.GetCurrentDirectory(), $"{name.Split('/').Last()} {DateTime.Now.ToString("dMyyyy")}.zip"), CompressionLevel.SmallestSize, false);
                Console.WriteLine("Archive created.");
                Cleanup();
                Console.WriteLine("Input an URL to archive.");
                CheckInput();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Archiving failed: {ex.Message}");
            }
        }

        private static async void StartDownload(string url)
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo(EXE_NAME, $"{url} -P downloads") { UseShellExecute = true });
                // Not even needed?
                if (process?.Responding == true)
                {
                    await process?.WaitForExitAsync();
                }
                else
                {
                    Console.WriteLine("Error, process is not responding.");
                    return;
                }
                var files = Directory.GetFiles(_downloadPath);
                if (files.Length == 0)
                {
                    Console.WriteLine("No files downloaded.");
                    return;
                }
                Console.WriteLine($"Download completed, {files.Length} file(s) downloaded. Archiving.");
                Archive(url);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"StartDownload failed: {ex.Message}");
            }
        }
    }
}