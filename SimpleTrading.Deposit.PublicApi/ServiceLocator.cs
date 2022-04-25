using System;
using System.Text;
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
using MyCrm.AuditLog.Grpc;
using MyDependencies;
using MySettingsReader;
using Serilog;
using SimpleTrading.Common.Abstractions.MyNoSQL;
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
    public class ServiceLocator
    {
        public static byte[] EncodeKey { get; set; }
        public static SettingsModel Settings => SettingsReader.GetSettings<SettingsModel>(".simple-trading");
        public static IDepositManagerGrpcService DepositManagerGrpcService { get; private set; }
        public static WalletRepository WalletRepository { get; private set; }
        public static DepositRepository DepositRepository { get; private set; }
        public static UtmPostgresRepository UtmRepository { get; private set; }
        public static IPersonalDataServiceGrpc PersonalDataServiceGrpc { get; private set; }
        public static IMyCrmAuditLogGrpcService AuditLogGrpcService { get; private set; }
        public static IFinancePciDssIntegrationGrpcService FinancePciDssIntegrationGrpcService { get; private set; }
        public static IConvertService ConvertService { get; private set; }
        public static IFinanceSwiffyIntegrationGrpcService FinanceSwiffyIntegrationGrpcService { get; private set; }
        public static IFinanceDirectaIntegrationGrpcService FinanceDirectaIntegrationGrpcService { get; private set; }
        public static ILogger Logger { get; private set; }
        public static IProcessIdService<MakeSwiffyDepositGrpcResponse> MakeSwiffyDepositProcessIdService { get; private set; }

        public static IProcessIdService<MakeDirectaDepositGrpcResponse> MakeDirectaDepositProcessIdService
        {
            get;
            private set;
        }

        public static IFinanceVoltIntegrationGrpcService FinanceVoltIntegrationGrpcService { get; set; }

        public static IProcessIdService<MakeVoltDepositGrpcResponse> MakeVoltDepositProcessIdService { get; set; }

        public static IProcessIdService<MakePayRetailersDepositGrpcResponse> MakePayRetailersDepositProcessIdService { get; set; }

        public static IFinancePayRetailersIntegrationGrpcService FinancePayRetailersIntegrationGrpcService { get; set; }

        public static IFinancePayopIntegrationGrpcService FinancePayopIntegrationGrpcService { get; set; }

        public static IProcessIdService<MakePayopDepositGrpcResponse> MakePayopDepositProcessIdService { get; set; }

        public static GrpcServiceClient<ITraderExternalDataGrpc> TraderExternalDataGrpcService { get; private set; }
        public static IMyNoSQLReaderLite<DepositRestrictionNoSqlEntity> DepositRestrictionsReader { get; set; }

        public static void Init(IServiceResolver sr)
        {
            DepositManagerGrpcService = sr.GetService<IDepositManagerGrpcService>();
            WalletRepository = sr.GetService<WalletRepository>();
            PersonalDataServiceGrpc = sr.GetService<IPersonalDataServiceGrpc>();
            DepositRepository = sr.GetService<DepositRepository>();
            UtmRepository = sr.GetService<UtmPostgresRepository>();
            AuditLogGrpcService = sr.GetService<IMyCrmAuditLogGrpcService>();
            FinancePciDssIntegrationGrpcService = sr.GetService<IFinancePciDssIntegrationGrpcService>();
            ConvertService = sr.GetService<IConvertService>();
            FinanceSwiffyIntegrationGrpcService = sr.GetService<IFinanceSwiffyIntegrationGrpcService>();
            FinanceDirectaIntegrationGrpcService = sr.GetService<IFinanceDirectaIntegrationGrpcService>();
            FinanceVoltIntegrationGrpcService = sr.GetService<IFinanceVoltIntegrationGrpcService>();
            FinancePayRetailersIntegrationGrpcService = sr.GetService<IFinancePayRetailersIntegrationGrpcService>();
            FinancePayopIntegrationGrpcService = sr.GetService<IFinancePayopIntegrationGrpcService>();
            Logger = sr.GetService<MyLogger.MyLogger>();
            MakeSwiffyDepositProcessIdService = sr.GetService<IProcessIdService<MakeSwiffyDepositGrpcResponse>>();
            MakeDirectaDepositProcessIdService = sr.GetService<IProcessIdService<MakeDirectaDepositGrpcResponse>>();
            MakeVoltDepositProcessIdService = sr.GetService<IProcessIdService<MakeVoltDepositGrpcResponse>>();
            MakePayRetailersDepositProcessIdService = sr.GetService<IProcessIdService<MakePayRetailersDepositGrpcResponse>>();
            MakePayopDepositProcessIdService = sr.GetService<IProcessIdService<MakePayopDepositGrpcResponse>>();
            TraderExternalDataGrpcService = sr.GetService<GrpcServiceClient<ITraderExternalDataGrpc>>();
            DepositRestrictionsReader = sr.GetService<IMyNoSQLReaderLite<DepositRestrictionNoSqlEntity>>();
        }


        public static void BindKeys()
        {
            var tokenKey = Environment.GetEnvironmentVariable("TOKEN_KEY");
            if (string.IsNullOrEmpty(tokenKey))
                throw new Exception("Please specify TOKEN_KEY environment variable");
            
            EncodeKey = Encoding.UTF8.GetBytes(tokenKey);
        }
    }
}