using System;
using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace AsyncWork
{
    public class Program
    {
        public static void Main(string[] args)
        {
            int logicalProcessorCount = Environment.ProcessorCount;
            bool success = ThreadPool.SetMaxThreads(workerThreads: logicalProcessorCount, completionPortThreads: logicalProcessorCount);

            ThreadPool.SetMaxThreads(4, 4);

            //ThreadPool.GetAvailableThreads(out int wt, out int iot);

            //for (int i = 0; i < 10; i++)
            //{
            //    ThreadPool.QueueUserWorkItem(PooledProc);
            //}

            //ThreadPool.GetAvailableThreads(out wt, out iot);

            //ThreadPool.GetAvailableThreads(out wt, out iot);

            //ThreadPool.QueueUserWorkItem(new WaitCallback(PooledProc));

            //ThreadPool.GetMinThreads(out int minimumWorkerThreadCount, out int minimumIOCThreadCount);
            //ThreadPool.GetMaxThreads(out int maximumWorkerThreadCount, out int maximumIOCThreadCount);

            CreateWebHostBuilder(args).Build().Run();
        }

        static void PooledProc(object stateInfo)
        {
            Thread.Sleep(300000);

            Console.WriteLine("Pooled Proc");
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
