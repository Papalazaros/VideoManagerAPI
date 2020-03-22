using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace VideoManager.Services
{
    public interface IFileService
    {
        public Task Create(Stream data, string filePath);
        public bool Move(string sourceFilePath, string targetFilePath);
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

        public bool Move(string sourceFilePath, string targetFilePath)
        {
            try
            {
                File.Copy(sourceFilePath, targetFilePath, true);
                Delete(sourceFilePath);
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
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
