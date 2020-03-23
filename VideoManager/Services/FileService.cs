using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public interface IFileService
    {
        public Task Create(Stream data, string filePath);
        public bool Move(string sourceFilePath, Video video);
        public void Delete(string filePath);
    }

    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;

        public FileService(ILogger<FileService> logger)
        {
            _logger = logger;
        }

        public async Task Create(Stream data, string filePath)
        {
            using (data)
            {
                using FileStream createdFile = File.Create(filePath);
                await data.CopyToAsync(createdFile);
            }
        }

        public bool Move(string sourceFilePath, Video video)
        {
            try
            {
                File.Copy(sourceFilePath, video.GetEncodedFilePath(), true);
                Delete(sourceFilePath);

                if (video.OriginalType != video.EncodedType) Delete(video.GetOriginalFilePath());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failure moving video.");
                return false;
            }

            return true;
        }

        public void Delete(string filePath)
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }
}
