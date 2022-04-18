using Finance.DirectaIntegration.GrpcContracts;
using Finance.DirectaIntegration.GrpcContracts.Contracts;
using Finance.PayopIntegration.GrpcContracts;
using Finance.PayopIntegration.GrpcContracts.Contracts;
using Finance.PayRetailersIntegration.GrpcContracts;
using Finance.PayRetailersIntegration.GrpcContracts.Contracts;
using Finance.PciDssIntegration.GrpcContracts;
using Finance.SwiffyIntegration.GrpcContracts;
using Finance.SwiffyIntegration.GrpcContracts.Contracts;
using Finance.VoltIntegration.GrpcContracts;
using Finance.VoltIntegration.GrpcContracts.Contracts;
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
                GrpcChannel.ForAddress(settingsModel.FinanceSwiffyIntegrationService)
                    .CreateGrpcService<IFinanceSwiffyIntegrationGrpcService>()
            );

            sr.Register(
                GrpcChannel.ForAddress(settingsModel.FinanceDirectaIntegrationService)
                    .CreateGrpcService<IFinanceDirectaIntegrationGrpcService>()
            );

            sr.Register(
                GrpcChannel.ForAddress(settingsModel.FinanceVoltIntegrationService)
                    .CreateGrpcService<IFinanceVoltIntegrationGrpcService>()
            );

            sr.Register(
                GrpcChannel.ForAddress(settingsModel.FinancePayRetailersIntegrationService)
                    .CreateGrpcService<IFinancePayRetailersIntegrationGrpcService>()
            );

            sr.Register(
                GrpcChannel.ForAddress(settingsModel.FinancePayopIntegrationService)
                    .CreateGrpcService<IFinancePayopIntegrationGrpcService>()
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
            sr.Register<IProcessIdService<MakeSwiffyDepositGrpcResponse>>(
                new ProcessIdService<MakeSwiffyDepositGrpcResponse>());
            sr.Register<IProcessIdService<MakeVoltDepositGrpcResponse>>(
                new ProcessIdService<MakeVoltDepositGrpcResponse>());
            sr.Register<IProcessIdService<MakeDirectaDepositGrpcResponse>>(
                new ProcessIdService<MakeDirectaDepositGrpcResponse>());
            sr.Register<IProcessIdService<MakePayRetailersDepositGrpcResponse>>(
                new ProcessIdService<MakePayRetailersDepositGrpcResponse>());
            sr.Register<IProcessIdService<MakePayopDepositGrpcResponse>>(
                new ProcessIdService<MakePayopDepositGrpcResponse>());
        }
        
        public static MyNoSqlTcpClient BindNoSqlReaders(this IServiceRegistrator sr, SettingsModel settingsModel)
        {
            var tcpConnection = new MyNoSqlTcpClient(() => settingsModel.PricesMyNoSqlServerReader, AppName);

            sr.Register<IMyNoSQLReaderLite<DepositRestrictionNoSqlEntity>>(tcpConnection.CreateDepositRestrictionsNoSqlReader());

            return tcpConnection;
        }
    }
}