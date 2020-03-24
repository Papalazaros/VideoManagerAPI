using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VideoManager.Services
{
    public abstract class BackgroundTaskService : BackgroundService
    {
        // Dependencies
        private readonly ILogger<BackgroundTaskService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;        

        // Polling
        private const int _defaultPollingInterval = 15;
        private const int _maxPollingInterval = 300;
        private const int _pollingIntervalIncreaseRate = 2;
        private static int _pollingInterval = _defaultPollingInterval;

        protected BackgroundTaskService(ILogger<BackgroundTaskService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            TimeSpan _timeToCheckInterval = TimeSpan.FromSeconds(_pollingInterval);

            while (!cancellationToken.IsCancellationRequested)
            {
                bool workCompleted = await DoWork(_logger, _scopeFactory);

                if (workCompleted)
                {
                    _pollingInterval = _defaultPollingInterval;
                }
                else
                {
                    _logger.LogInformation(
                        "No tasks to completed. Sleeping for {_pollingInterval} seconds.", _timeToCheckInterval.TotalSeconds);

                    _pollingInterval = Math.Min(_maxPollingInterval, _pollingInterval * _pollingIntervalIncreaseRate);

                    await Task.Delay(_timeToCheckInterval);
                }
            }
        }

        public abstract Task<bool> DoWork(ILogger<BackgroundTaskService> logger, IServiceScopeFactory scopeFactory);
    }
}
