using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace MvcWork.Checkers
{
    public class SqlServerHealthcheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(@"Data Source=.;Initial Catalog=ConsoleDb;Integrated Security=true;TrustServerCertificate=True;MultipleActiveResultSets=true"))
            {
                try
                {
                    await connection.OpenAsync();
                }
                catch (Exception)
                {
                    return HealthCheckResult.Unhealthy();
                }
                return HealthCheckResult.Healthy();
            }
        }
    }
}
