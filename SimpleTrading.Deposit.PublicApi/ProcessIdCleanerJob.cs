using Finance.SwiffyIntegration.GrpcContracts.Contracts;
using Microsoft.Extensions.Hosting;
using Serilog;
using SimpleTrading.Deposit.PublicApi.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleTrading.Deposit.PublicApi
{
    public class ProcessIdCleanerJob : IHostedService
    {
        private Timer _timer;
        private SettingsModel SettingsModel => ServiceLocator.Settings;
        private ILogger Logger => ServiceLocator.Logger;
        private IProcessIdService<MakeSwiffyDepositGrpcResponse> MakeSwiffyDepositProcessIdService => ServiceLocator.MakeSwiffyDepositProcessIdService;
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = TimerFactory();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            return Task.CompletedTask;
        }

        private Timer TimerFactory()
        {
            var period = TimeSpan.TryParse(SettingsModel.ProcessIdCleanerTimeout, out var timeSpan) ? timeSpan : TimeSpan.FromMinutes(30);
            return new Timer(Collect, null, TimeSpan.Zero, period);
        }

        private void Collect(object? state)
        {
            Activity activity = new Activity(nameof(ProcessIdCleanerJob)).SetIdFormat(ActivityIdFormat.W3C).Start();
            try
            {
                Logger.Information("ProcessIdCleanerJob start clear");
                MakeSwiffyDepositProcessIdService.Clear();
            }
            finally
            {
                activity.Stop();
            }
        }
    }
}
