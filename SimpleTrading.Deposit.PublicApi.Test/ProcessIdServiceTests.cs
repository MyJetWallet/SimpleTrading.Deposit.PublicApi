using NUnit.Framework;
using SimpleTrading.Deposit.PublicApi.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleTrading.Deposit.PublicApi.Test
{
    public class ProcessIdServiceTests
    {
        [Test]
        public async Task GetOrCreateAsync_Task_ShouldReturnDifferentResult_WhenProcessIdDifferent()
        {
            var processService = new ProcessIdService<string>();
            string processId1 = nameof(processId1);
            var result1 = await processService.GetOrCreateAsync(processId1, () => GetResultAsync(1));
            string processId2 = nameof(processId2);
            var result2 = await processService.GetOrCreateAsync(processId2, () => GetResultAsync(2));
            var result3 = await processService.GetOrCreateAsync(processId2, () => GetResultAsync(3));
            var result4 = await processService.GetOrCreateAsync(processId1, () => GetResultAsync(1));

            Assert.AreEqual(nameof(result1), result1);
            Assert.AreEqual(nameof(result2), result2);
            Assert.AreEqual(result2, result3);
            Assert.AreEqual(result4, result1);
        }

        [Test]
        public async Task GetOrCreateAsync_ValueTask_ShouldReturnDifferentResult_WhenProcessIdDifferent()
        {
            var processService = new ProcessIdService<string>();
            string processId1 = nameof(processId1);
            var result1 = await processService.GetOrCreateAsync(processId1, () => GetResultValueTaskAsync(1));
            string processId2 = nameof(processId2);
            var result2 = await processService.GetOrCreateAsync(processId2, () => GetResultValueTaskAsync(2));
            var result3 = await processService.GetOrCreateAsync(processId2, () => GetResultValueTaskAsync(3));
            var result4 = await processService.GetOrCreateAsync(processId1, () => GetResultValueTaskAsync(1));

            Assert.AreEqual(nameof(result1), result1);
            Assert.AreEqual(nameof(result2), result2);
            Assert.AreEqual(result2, result3);
            Assert.AreEqual(result4, result1);
        }

        [Test]
        public async Task GetOrCreateAsync_Task_ShouldReturnResultInSameTime_WhenProcessIdSame()
        {
            var processService = new ProcessIdService<string>();
            string processId1 = nameof(processId1);
            List<Task<string>> tasks = new List<Task<string>>();
            var stopWatch = Stopwatch.StartNew();
            tasks.Add( processService.GetOrCreateAsync(processId1, () => GetResultValueTaskAsync(1, 3000)));
            tasks.Add( processService.GetOrCreateAsync(processId1, () => GetResultValueTaskAsync(1, 3000)));
            tasks.Add( processService.GetOrCreateAsync(processId1, () => GetResultValueTaskAsync(1, 3000)));
            tasks.Add( processService.GetOrCreateAsync(processId1, () => GetResultValueTaskAsync(1, 3000)));
            tasks.Add( processService.GetOrCreateAsync(processId1, () => GetResultValueTaskAsync(1, 3000)));
            tasks.Add( processService.GetOrCreateAsync(processId1, () => GetResultValueTaskAsync(1, 3000)));
            tasks.Add( processService.GetOrCreateAsync(processId1, () => GetResultValueTaskAsync(1, 3000)));

            var result = await Task.WhenAll(tasks);
            stopWatch.Stop();
            Assert.IsTrue(result.All(x=>x.Equals("result1")));
            Assert.LessOrEqual(stopWatch.ElapsedMilliseconds, 4000);
        }

        private async Task<string> GetResultAsync(int i, int? delayMs = null)
        {
            if (delayMs is not null)
            {
                await Task.Delay((int)delayMs);
            }
            return $"result{i}";
        }

        private async ValueTask<string> GetResultValueTaskAsync(int i, int? delayMs = null)
        {
            if (delayMs is not null)
            {
                await Task.Delay((int) delayMs);
            }
            return $"result{i}";
        }
    }
}
