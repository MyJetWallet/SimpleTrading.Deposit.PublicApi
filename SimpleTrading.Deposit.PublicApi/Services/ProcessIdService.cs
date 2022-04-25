using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleTrading.Deposit.PublicApi.Services
{
    public class ProcessIdService<TResult> : IProcessIdService<TResult>
    {
        private ConcurrentDictionary<string, Lazy<Task<TResult>>> _tasksByProcessId = new ConcurrentDictionary<string, Lazy<Task<TResult>>>();
        public Task<TResult> GetOrCreateAsync(string processId, Func<Task<TResult>> func)
        {
            if (string.IsNullOrEmpty(processId))
            {
                return func();
            }

            return _tasksByProcessId.GetOrAdd(processId, new Lazy<Task<TResult>>(func)).Value;
        }

        public Task<TResult> GetOrCreateAsync(string processId, Func<ValueTask<TResult>> func)
        {
            if (string.IsNullOrEmpty(processId))
            {
                return func().AsTask();
            }

            return _tasksByProcessId.GetOrAdd(processId, new Lazy<Task<TResult>>(func().AsTask)).Value;
        }

        public void Clear(int maxCount = 1000)
        {
            if (_tasksByProcessId.Count >= maxCount)
            {
                _tasksByProcessId.Clear();
            }
        }
    }

    public interface IProcessIdService<TResult>
    {
        Task<TResult> GetOrCreateAsync(string processId, Func<Task<TResult>> func);
        Task<TResult> GetOrCreateAsync(string processId, Func<ValueTask<TResult>> func);
        void Clear(int maxCount = 1000);
    }
}
