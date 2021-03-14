using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public interface IEncoder
    {
        Task<EncodeResult> Encode(Video video);
        Task<int?> GetVideoDurationInSeconds(string path);
        Task<string?> CreateThumbnail(Video video);
    }

    public class EncodeService : IEncoder
    {
        private readonly ILogger<EncodeService> _logger;
        private readonly IFileService _fileService;

        public EncodeService(ILogger<EncodeService> logger, IFileService fileService)
        {
            _logger = logger;
            _fileService = fileService;
        }

        private async Task<(string?, string?)> RunCommandAsync(string arguments, string fileName = "ffmpeg.exe")
        {
            if (!File.Exists(fileName)) throw new Exception($"{fileName} not found.");

            string? standardOutput = null;
            string? standardError = null;

            try
            {
                using Process process = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = fileName,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    }
                };

                process.Start();

                while (!process.HasExited)
                {
                    await Task.Delay(10000);
                }

                standardOutput = await process.StandardOutput.ReadToEndAsync();
                standardError = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failure executing process.");
            }

            return (standardOutput, standardError);
        }

        public async Task<EncodeResult> Encode(Video video)
        {
            EncodeResult encodeResult = new();
            Stopwatch stopWatch = new();
            stopWatch.Start();

            string encodedFilePath = Path.GetTempPath() + Guid.NewGuid().ToString() + video.EncodedType;

            try
            {
                string arguments = $"-loglevel error -i {video.GetOriginalFilePath()} -codec:v libx264 -filter_complex \"scale = iw * min(1\\, min(1920 / iw\\, 1280 / ih)):-2\" -b:v 2M -maxrate 2M -bufsize 1M -c:a copy -preset medium -crf 26 -y -threads 1 {encodedFilePath}";
                (string? standardOutput, string? standardError) = await RunCommandAsync(arguments);

                if (string.IsNullOrEmpty(standardError)) encodeResult.Success = true;
                else _logger.LogError("Error in ffmpeg process {ErrorMessage}", standardError);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failure starting video encoding process.");
            }

            if (encodeResult.Success)
            {
                encodeResult.Success = _fileService.Move(encodedFilePath, video);
                if (encodeResult.Success) encodeResult.EncodedFileLength = new FileInfo(video.GetEncodedFilePath()).Length;
            }

            stopWatch.Stop();

            encodeResult.EncodeTime = stopWatch.Elapsed;

            return encodeResult;
        }

        public async Task<int?> GetVideoDurationInSeconds(string path)
        {
            if (File.Exists(path))
            {
                string command = $"-v quiet -print_format compact=print_section=0:nokey=1:escape=csv -show_entries format=duration \"{path}\"";
                (string? standardOutput, string? standardError) = await RunCommandAsync(command, "ffprobe.exe");

                if (string.IsNullOrEmpty(standardError) && double.TryParse(standardOutput, out double parsedValue)) return (int)parsedValue;
            }

            return null;
        }

        public async Task<string?> CreateThumbnail(Video video)
        {
            string encodedFilePath = video.GetEncodedFilePath();
            string? encodedFileDirectory = Path.GetDirectoryName(encodedFilePath);

            if (File.Exists(encodedFilePath) && !string.IsNullOrEmpty(encodedFileDirectory))
            {
                string outputThumbnailPath = Path.Combine(encodedFileDirectory, video.VideoId + ".gif");
                string command = $"-loglevel error -t 2.5 -i {encodedFilePath} -filter_complex \"[0:v] fps = 10,scale = 480:-1,split[a][b];[a] palettegen[p];[b][p] paletteuse\" {outputThumbnailPath}";

                (string? _, string? standardError) = await RunCommandAsync(command);

                if (string.IsNullOrEmpty(standardError)) return outputThumbnailPath;
            }

            return null;
        }

        //public async Task<string?> CreateThumbnail(Video video)
        //{
        //    string encodedFilePath = video.GetEncodedFilePath();
        //    string? encodedFileDirectory = Path.GetDirectoryName(encodedFilePath);

        //    if (File.Exists(encodedFilePath) && !string.IsNullOrEmpty(encodedFileDirectory))
        //    {
        //        string outputThumbnailPath = Path.Combine(encodedFileDirectory, video.VideoId + ".jpg");
        //        string command = $"-loglevel error -i {encodedFilePath} -vf \"thumbnail,scale = 320:-2\" -frames:v 1 {outputThumbnailPath}";

        //        (string? _, string? standardError) = await RunCommandAsync(command);

        //        if (string.IsNullOrEmpty(standardError)) return outputThumbnailPath;
        //    }

        //    return null;
        //}
    }
}