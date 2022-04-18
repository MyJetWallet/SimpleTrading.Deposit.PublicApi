using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SimpleTrading.Deposit.PublicApi.Services
{
    public interface ILocalCacheService<TResult>
    {
        Task<TResult> GetOrAddAsync(string key, Task<TResult> loader);
    }
}
