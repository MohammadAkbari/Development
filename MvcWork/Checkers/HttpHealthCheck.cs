using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MvcWork.Checkers
{
    public class HttpHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync("https://github.com");
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception("Url not responding with 200 OK");
                    }
                }
                catch (Exception)
                {
                    return await Task.FromResult(HealthCheckResult.Unhealthy());
                }
            }
            return await Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
