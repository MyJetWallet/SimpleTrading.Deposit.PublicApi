using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SimpleTrading.Deposit.PublicApi
{
    public class ExceptionLogMiddleware
    {
        private readonly RequestDelegate _next;
 
        public ExceptionLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }
 
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                ServiceLocator.Logger.Error(ex, ex.Message);
                throw;
            }
        }
    }
}