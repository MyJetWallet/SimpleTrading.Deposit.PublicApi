using Finance.PciDssIntegration.GrpcContracts;
using Grpc.Net.Client;
using MyCrm.AuditLog.Grpc;
using MyDependencies;
using MyNoSqlServer.DataReader;
using ProtoBuf.Grpc.Client;
using Serilog;
using SimpleTrading.Common.Abstractions.MyNoSQL;
using SimpleTrading.Common.MyNoSql;
using SimpleTrading.Common.MyNoSql.DepositRestrictions;
using SimpleTrading.ConvertService.Grpc;
using SimpleTrading.Deposit.Grpc;
using SimpleTrading.Deposit.Postgresql.Repositories;
using SimpleTrading.Deposit.PublicApi.Services;
using SimpleTrading.GrpcTemplate;
using SimpleTrading.PersonalData.Grpc;
using SimpleTrading.TraderExternalData.Grpc;
using SimpleTrading.Utm.Postgres;

namespace SimpleTrading.Deposit.PublicApi
{
    public static class ServiceBinder
    {
        private const string AppName = "DepositManagerView";

        public static void BindGrpcServices(this IServiceRegistrator sr, SettingsModel settingsModel)
        {
            sr.Register(
                GrpcChannel.ForAddress(settingsModel.DepositManagerGrpcHost)
                    .CreateGrpcService<IDepositManagerGrpcService>());
            
            sr.Register(
                GrpcChannel.ForAddress(settingsModel.FinancePciDssIntegrationService)
                    .CreateGrpcService<IFinancePciDssIntegrationGrpcService>()
            );
            
            sr.Register(
                GrpcChannel.ForAddress(settingsModel.CrmAuditlogGrpcServiceUrl)
                    .CreateGrpcService<IMyCrmAuditLogGrpcService>()
            );            
            
            sr.Register(
                GrpcChannel.ForAddress(settingsModel.PersonalDataGrpcService)
                    .CreateGrpcService<IPersonalDataServiceGrpc>()
            );

            sr.Register(
                GrpcChannel.ForAddress(settingsModel.ConvertServiceGrpcUrl)
                    .CreateGrpcService<IConvertService>()
            );

            sr.Register(
                new GrpcServiceClient<ITraderExternalDataGrpc>(
                    () => settingsModel.TraderExternalDataGrpcServiceUrl));
        }
        public static void BindSeqLogger(this IServiceRegistrator sr, SettingsModel settingsModel)
        {
            var log = new MyLogger.MyLogger("DepositManagerView", settingsModel.SeqUrl);
            sr.Register(log);
        }
        public static void BindDatabaseRepositories(this IServiceRegistrator sr, SettingsModel settingsModel, ILogger logger)
        {
            sr.Register(new WalletRepository(settingsModel.DbConnectionString, 
                settingsModel.DbReadConnectionString, AppName));
            sr.Register(new DepositRepository(settingsModel.DbConnectionString, 
                settingsModel.DbReadConnectionString, AppName));
            sr.Register(new UtmPostgresRepository(settingsModel.DbConnectionString, 
                settingsModel.DbReadConnectionString, AppName, logger));
        }

        public static void BindServices(this IServiceRegistrator sr, SettingsModel settingsModel)
        {
            //sr.Register<IProcessIdService<MakePayopDepositGrpcResponse>>(
            //    new ProcessIdService<MakePayopDepositGrpcResponse>());
        }
        
        public static MyNoSqlTcpClient BindNoSqlReaders(this IServiceRegistrator sr, SettingsModel settingsModel)
        {
            var tcpConnection = new MyNoSqlTcpClient(() => settingsModel.PricesMyNoSqlServerReader, AppName);

            sr.Register<IMyNoSQLReaderLite<DepositRestrictionNoSqlEntity>>(tcpConnection.CreateDepositRestrictionsNoSqlReader());

            return tcpConnection;
        }
    }
}