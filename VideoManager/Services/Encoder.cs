using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public interface IEncoder
    {
        Task<bool> Encode(Video video);
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

        public async Task<bool> Encode(Video video)
        {
            bool success = false;
            string encodedFilePath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".mp4";

            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                        FileName = @"C:\Users\Jacob\Desktop\ffmpeg\ffmpeg.exe",
                        Arguments = $"-loglevel error -i {video.AssignedName} -c:v libx264 -filter_complex \"scale = iw * min(1\\, min(1920 / iw\\, 1280 / ih)):-2\" -b:v 2M -maxrate 2M -bufsize 1M -c:a copy -preset medium -crf 28 -y -threads 1 {encodedFilePath}",
                        UseShellExecute = false,
                        RedirectStandardError = true
                    }
                };

                process.Start();
                string standardError = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                if (string.IsNullOrEmpty(standardError))
                {
                    success = true;
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

            if (success)
            {
                success = _fileService.Move(encodedFilePath, video.AssignedName);
            }

            return success;
        }
    }
}
