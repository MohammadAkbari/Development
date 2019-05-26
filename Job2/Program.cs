using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Job2
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddJobScheduler<SimpleJob>();

                    services.AddTransient<ITrigger>(e => TriggerBuilder.Create()
                        .WithIdentity("Tasks", "Task1")
                        .WithSimpleSchedule(x => x
                            .WithIntervalInMinutes(1)
                            .RepeatForever())
                        .StartNow()
                        .Build());

                    services.AddSingleton<IHostedService, DaemonService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    //logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });

            await builder.RunConsoleAsync();
        }
    }

    public static class QuartzServicesUtilities
    {
        public static void StartJob<TJob>(IScheduler scheduler, TimeSpan runInterval)
            where TJob : IJob
        {
            var jobName = typeof(TJob).FullName;

            var job = JobBuilder.Create<TJob>()
                .WithIdentity(jobName)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{jobName}.trigger")
                .StartNow()
                .WithSimpleSchedule(scheduleBuilder =>
                    scheduleBuilder
                        .WithInterval(runInterval)
                        .RepeatForever())
                .Build();

            scheduler.ScheduleJob(job, trigger);
        }
    }

    internal class SimpleJob : IJob
    {
        private readonly ILogger<SimpleJob> _logger;

        public SimpleJob(ILogger<SimpleJob> logger)
        {
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"Start job at {Environment.CurrentDirectory}");

            await Task.CompletedTask;

            _logger.LogInformation($"End job at {Environment.CurrentDirectory}");
        }
    }

    internal static class JobSchedulerExtension
    {
        public static IServiceCollection AddJobScheduler<TJob>(this IServiceCollection services) where TJob : class, IJob
        {
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddTransient<IJobFactory, JobFactory>();
            services.AddTransient<IJobDetail>(e => JobBuilder.Create<IJob>().Build());
            services.AddTransient<IJob, TJob>();

            services.AddTransient<IScheduler>((provider) =>
            {
                var schedulerFactory = provider.GetService<ISchedulerFactory>();
                var jobfactory = provider.GetService<IJobFactory>();

                var scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
                scheduler.JobFactory = jobfactory;

                return scheduler;
            });

            return services;
        }
    }

    internal class JobFactory : IJobFactory
    {
        protected readonly IServiceProvider Container;

        public JobFactory(IServiceProvider container)
        {
            Container = container;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return Container.GetService(bundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job)
        {
            (job as IDisposable)?.Dispose();
        }
    }

    public class DaemonService : IHostedService, IDisposable
    {
        private readonly IScheduler _scheduler;
        private readonly IJobDetail _jobDetail;
        private readonly ITrigger _trigger;

        public DaemonService(IScheduler scheduler, IJobDetail jobDetail, ITrigger trigger)
        {
            _scheduler = scheduler;
            _jobDetail = jobDetail;
            _trigger = trigger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _scheduler.ScheduleJob(_jobDetail, _trigger);

            await _scheduler.Start();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //_logger.LogInformation("Stopping daemon.");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            //_logger.LogInformation("Disposing....");
        }
    }
}
