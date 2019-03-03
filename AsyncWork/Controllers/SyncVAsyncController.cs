using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Threading;
using Dapper;

namespace AsyncWork.Controllers
{
    public class SyncVsAsyncController : Controller
    {
        #region Ctor

        private readonly string _connectionString;
        public SyncVsAsyncController()
        {
            _connectionString = "Data Source=.;Initial Catalog=ConsoleDb;Integrated Security=true;TrustServerCertificate=True;MultipleActiveResultSets=true";
        }

        #endregion

        [HttpGet("sync")]
        public IActionResult SyncGet()
        {
            Query();
            Query();

            return Ok(GetThreadInfo());
        }

        [HttpGet("async")]
        public async Task<IActionResult> AsyncGet()
        {
            await QueryAsync();
            await QueryAsync();

            return Ok(GetThreadInfo());
        }

        [HttpGet("parallel")]
        public async Task<IActionResult> ParallelGet()
        {
            var task1 = QueryAsync();
            var task2 = QueryAsync();

            await task1;
            await task2;

            return Ok(GetThreadInfo());
        }

        #region Private Methods

        private void Query()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Execute("WAITFOR DELAY '00:00:02';");
            }
        }

        private async Task QueryAsync()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("WAITFOR DELAY '00:00:02';");
            }
        }

        private dynamic GetThreadInfo()
        {
            ThreadPool.GetAvailableThreads(out int availableWorkerThreads, out int availableAsyncIOThreads);
            return new
            {
                AvailableAsyncIOThreads = availableAsyncIOThreads,
                AvailableWorkerThreads = availableWorkerThreads
            };
        }

        #endregion
    }
}
