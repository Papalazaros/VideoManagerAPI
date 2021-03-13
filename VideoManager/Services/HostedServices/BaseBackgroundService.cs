using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace VideoManager.Services
{
    /// <summary>
    /// Executes tasks with an increasing delay if no work is found
    /// </summary>
    public abstract class BackgroundTaskService : BackgroundService
    {
        // Dependencies
        protected readonly ILogger _logger;
        protected readonly IServiceScopeFactory _scopeFactory;

        // Polling
        private const int _defaultPollingInterval = 15;
        private const int _maxPollingInterval = 300;
        private const int _pollingIntervalIncreaseRate = 2;
        private int _pollingInterval = _defaultPollingInterval;

        protected BackgroundTaskService(ILogger logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                bool workCompleted = await DoWork();

                if (workCompleted && _pollingInterval != _defaultPollingInterval)
                {
                    _pollingInterval = _defaultPollingInterval;
                }
                else if (!workCompleted)
                {
                    TimeSpan _timeToCheckInterval = TimeSpan.FromSeconds(_pollingInterval);

                    _logger.LogInformation(
                        "No tasks found. Sleeping for {_pollingInterval} seconds.", _timeToCheckInterval.TotalSeconds);

                    await Task.Delay(_timeToCheckInterval, cancellationToken);

                    _pollingInterval = Math.Min(_maxPollingInterval, _pollingInterval * _pollingIntervalIncreaseRate);
                }
            }
        }

        public abstract Task<bool> DoWork();
    }
}
