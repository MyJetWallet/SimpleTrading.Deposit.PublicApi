using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyDependencies;
using MyNoSqlServer.DataReader;
using MySettingsReader;
using SimpleTrading.Common.Helpers;
using SimpleTrading.Cryptography;
using SimpleTrading.ServiceStatusReporterConnector;

namespace SimpleTrading.Deposit.PublicApi
{
    public class Startup
    {
        private static readonly MyIoc Ioc = new MyIoc();
        private static readonly SettingsModel Settings = SettingsReader.GetSettings<SettingsModel>(".simple-trading");
        public IConfiguration Configuration { get; }
        private ILogger logger { get; set; }
        private static MyNoSqlTcpClient _connection;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            Ioc.BindSeqLogger(Settings);
            services.SetupSwagger();
            Ioc.BindGrpcServices(Settings);
            Ioc.BindDatabaseRepositories(Settings, Ioc.GetService<MyLogger.MyLogger>());
            Ioc.BindServices(Settings);
            _connection = Ioc.BindNoSqlReaders(Settings);

            ServiceLocator.Init(Ioc);
            services.AddHostedService<ProcessIdCleanerJob>();
            services.AddControllers().AddNewtonsoftJson();
            ServiceLocator.BindKeys();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<ExceptionLogMiddleware>();
            app.UseForwardedHeaders();
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseOpenApi();
            app.UseSwaggerUi3();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.BindIsAlive(GetEnvs());

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
            _connection.Start();
        }

        private static Dictionary<string, string> GetEnvs()
        {
            return new Dictionary<string, string>
            {
                {"SESSION_KEY", Environment.GetEnvironmentVariable("TOKEN_KEY")?.EncodeToSha1().ToHexString()}
            };
        }
    }
}