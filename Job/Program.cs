using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Serilog;
using System;
using System.Threading.Tasks;
using Topshelf;

namespace Job
{
    class Program
    {
        static void Main(string[] args)
        {
            var provider = RegisterServices();

            HostFactory.Run(cfg =>
            {
                cfg.Service<SimpleTask>(s =>
                {
                    s.ConstructUsing(() => provider.GetService<SimpleTask>());
                    s.WhenStarted(async f => await f.Start());
                    s.WhenStopped(f => f.Stop());
                });

                cfg.RunAsLocalSystem();
                cfg.SetDescription("Job.Simple");
                cfg.SetDisplayName("Job.Simple");
                cfg.SetServiceName("Job.Simple");
            });
        }

        public static ServiceProvider RegisterServices()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddTransient<SimpleTask>();

            services.AddJobScheduler<SimpleJob>();

            services.AddTransient<ITrigger>(e => TriggerBuilder.Create()
                .WithIdentity("Tasks", "Task1")
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(1)
                    .RepeatForever())
                .StartNow()
                .Build());

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(@"C:\Temp\log2.txt")
                .CreateLogger();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddSerilog();
            });

            var provider = services.BuildServiceProvider();

            return provider;
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

    internal class SimpleTask
    {
        private readonly IScheduler _scheduler;
        private readonly IJobDetail _jobDetail;
        private readonly ITrigger _trigger;

        public SimpleTask(IScheduler scheduler, IJobDetail jobDetail, ITrigger trigger)
        {
            _scheduler = scheduler;
            _jobDetail = jobDetail;
            _trigger = trigger;
        }

        public async Task Start()
        {
            await _scheduler.ScheduleJob(_jobDetail, _trigger);

            await _scheduler.Start();
        }

        public bool Stop()
        {
            return true;
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
            _logger.LogInformation("Hi");

            await Task.CompletedTask;
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
}
