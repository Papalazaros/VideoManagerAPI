using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public interface IEncoder
    {
        Task<EncodeResult> Encode(Video video);
        Task<int?> GetVideoDurationInSeconds(string path);
        Task<string?> CreateThumbnail(Video video);
        Task<string?> CreatePreview(Video video);
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

        private static async Task<(string output, string error)> RunCommandAsync(string arguments, string fileName = "ffmpeg")
        {
            StringBuilder standardOutput = new();
            StringBuilder standardError = new();

            using Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };

            process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    standardOutput.AppendLine(e.Data);
                }
            });

            process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    standardError.AppendLine(e.Data);
                }
            });

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            return (standardOutput.ToString(), standardError.ToString());
        }

        public async Task<EncodeResult> Encode(Video video)
        {
            EncodeResult encodeResult = new();
            Stopwatch stopWatch = new();
            stopWatch.Start();

            string encodedFilePath = Path.GetTempPath() + Guid.NewGuid().ToString() + video.EncodedType;

            try
            {
                string arguments = $"-loglevel error -i {video.GetOriginalFilePath()} -codec:v libx264 -filter_complex \"scale = iw * min(1\\, min(1280 / iw\\, 720 / ih)):-2\" -maxrate 2M -bufsize 2M -c:a copy -crf 28 -y -threads 1 {encodedFilePath}";
                (string standardOutput, string standardError) = await RunCommandAsync(arguments);

                if (string.IsNullOrEmpty(standardError))
                {
                    encodeResult.Success = true;
                }
                else
                {
                    _logger.LogError("Error in ffmpeg process {ErrorMessage}", standardError);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failure starting video encoding process.");
            }

            if (encodeResult.Success)
            {
                encodeResult.Success = _fileService.Move(encodedFilePath, video);
                if (encodeResult.Success)
                {
                    encodeResult.EncodedFileLength = new FileInfo(video.GetEncodedFilePath()).Length;
                }
            }

            stopWatch.Stop();

            encodeResult.EncodeTime = stopWatch.Elapsed;

            return encodeResult;
        }

        public async Task<int?> GetVideoDurationInSeconds(string path)
        {
            string command = $"-v quiet -print_format compact=print_section=0:nokey=1:escape=csv -show_entries format=duration \"{path}\"";
            (string? standardOutput, string? standardError) = await RunCommandAsync(command, "ffprobe.exe");

            if (string.IsNullOrEmpty(standardError) && double.TryParse(standardOutput, out double parsedValue))
            {
                return (int)parsedValue;
            }

            return null;
        }

        public async Task<string?> CreatePreview(Video video)
        {
            string encodedFilePath = video.GetEncodedFilePath();
            string? encodedFileDirectory = Path.GetDirectoryName(encodedFilePath);

            if (!string.IsNullOrEmpty(encodedFileDirectory))
            {
                string outputThumbnailPath = Path.Combine(encodedFileDirectory, video.VideoId + "_preview.webp");

                string command = $"-loglevel error -i {encodedFilePath} -vf \"scale = 320:-2\" -t 00:00:03 {outputThumbnailPath}";

                (string? _, string? standardError) = await RunCommandAsync(command);

                if (string.IsNullOrEmpty(standardError))
                {
                    return outputThumbnailPath;
                }
            }

            return null;
        }

        public async Task<string?> CreateThumbnail(Video video)
        {
            string encodedFilePath = video.GetEncodedFilePath();
            string? encodedFileDirectory = Path.GetDirectoryName(encodedFilePath);

            if (!string.IsNullOrEmpty(encodedFileDirectory))
            {
                string outputThumbnailPath = Path.Combine(encodedFileDirectory, video.VideoId + "_thumbnail.webp");

                string command = $"-loglevel error -i {encodedFilePath} -vf \"thumbnail,scale = 320:-2\" -frames:v 1 {outputThumbnailPath}";

                (string? _, string? standardError) = await RunCommandAsync(command);

                if (string.IsNullOrEmpty(standardError))
                {
                    return outputThumbnailPath;
                }
            }

            return null;
        }
    }
}