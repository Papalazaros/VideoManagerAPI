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
    }

    public class Encoder : IEncoder
    {
        private readonly ILogger<Encoder> _logger;
        private readonly IFileService _fileService;

        public Encoder(ILogger<Encoder> logger, IFileService fileService)
        {
            _logger = logger;
            _fileService = fileService;
        }

        public async Task<EncodeResult> Encode(Video video)
        {
            EncodeResult encodeResult = new EncodeResult();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            _logger.LogInformation(
                "Starting encode of {video}.", video);

            string encodedFilePath = Path.GetTempPath() + Guid.NewGuid().ToString() + video.EncodedType;

            try
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = "ffmpeg.exe",
                        Arguments = $"-loglevel error -i {video.GetOriginalFilePath()} -c:v libx264 -filter_complex \"scale = iw * min(1\\, min(1920 / iw\\, 1280 / ih)):-2\" -b:v 2M -maxrate 2M -bufsize 1M -c:a copy -preset medium -crf 28 -y -threads 1 {encodedFilePath}",
                        UseShellExecute = false,
                        RedirectStandardError = true
                    }
                };

                process.Start();
                string standardError = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

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

            _logger.LogInformation(
                "Finished encode of {video} in {elapsedMilliseconds} milliseconds.", video, stopWatch.ElapsedMilliseconds);

            return encodeResult;
        }
    }
}
