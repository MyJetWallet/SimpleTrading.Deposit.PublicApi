using System.Collections.Generic;
using Destructurama.Attributed;
using MyYamlParser;


namespace SimpleTrading.Deposit.PublicApi
{
    public class SettingsModel
    {
        [YamlProperty("DepositManagerWebView.DepositManagerGrpcHost")]
        public string DepositManagerGrpcHost { get; set; }

        [NotLogged]
        [YamlProperty("DepositManagerWebView.DbConnectionString")]
        public string DbConnectionString { get; set; }

        [NotLogged]
        [YamlProperty("DepositManagerWebView.DbReadConnectionString")]
        public string DbReadConnectionString { get; set; }

        [YamlProperty("DepositManagerWebView.FinancePciDssIntegrationService")]
        public string FinancePciDssIntegrationService { get; set; }

        [YamlProperty("DepositManagerWebView.SeqUrl")]
        public string SeqUrl { get; set; }

        [YamlProperty("DepositManagerWebView.CrmAuditlogGrpcServiceUrl")]
        public string CrmAuditlogGrpcServiceUrl { get; set; }

        [YamlProperty("DepositManagerWebView.PersonalDataGrpcService")]
        public string PersonalDataGrpcService { get; set; }

        [YamlProperty("DepositManagerWebView.ConvertServiceGrpcUrl")]
        public string ConvertServiceGrpcUrl { get; set; }

        [LogMasked(ShowFirst = 2, ShowLast = 1, PreserveLength = true)]
        [YamlProperty("DepositManagerWebView.RoyalPayUsername")]
        public string RoyalPayUsername { get; set; }

        [NotLogged]
        [YamlProperty("DepositManagerWebView.RoyalPayPassword")]
        public string RoyalPayPassword { get; set; }

        [LogMasked(ShowFirst = 2, ShowLast = 1, PreserveLength = true)]
        [YamlProperty("DepositManagerWebView.MonfexRoyalPayUsername")]
        public string MonfexRoyalPayUsername { get; set; }

        [NotLogged]
        [YamlProperty("DepositManagerWebView.MonfexRoyalPayPassword")]
        public string MonfexRoyalPayPassword { get; set; }

        [LogMasked(ShowFirst = 2, ShowLast = 1, PreserveLength = true)]
        [YamlProperty("DepositManagerWebView.AllianzmarketRoyalPayUsername")]
        public string AllianzmarketRoyalPayUsername { get; set; }

        [NotLogged]
        [YamlProperty("DepositManagerWebView.AllianzmarketRoyalPayPassword")]
        public string AllianzmarketRoyalPayPassword { get; set; }

        [LogMasked(ShowFirst = 2, ShowLast = 1, PreserveLength = true)]
        [YamlProperty("DepositManagerWebView.TexcentUserId")]
        public string TexcentUserId { get; set; }

        [LogMasked(ShowFirst = 2, ShowLast = 1, PreserveLength = true)]
        [YamlProperty("DepositManagerWebView.TexcentHandelProUserId")]
        public string TexcentHandelProUserId { get; set; }

        [YamlProperty("DepositManagerWebView.HandleProStUrl")]
        public string HandleProStUrl { get; set; }

        [YamlProperty("DepositManagerWebView.MonfexStUrl")]
        public string MonfexStUrl { get; set; }

        [YamlProperty("DepositManagerWebView.AllianzmarketStUrl")]
        public string AllianzmarketStUrl { get; set; }

        [YamlProperty("DepositManagerWebView.HandleProMtUrl")]
        public string HandleProMtUrl { get; set; }

        [YamlProperty("DepositManagerWebView.MonfexMtUrl")]
        public string MonfexMtUrl { get; set; }

        [YamlProperty("DepositManagerWebView.AllianzmarketMtUrl")]
        public string AllianzmarketMtUrl { get; set; }

        [YamlProperty("DepositManagerWebView.MonfexBrandDomains")]
        public string MonfexBrandDomains { get; set; }

        [YamlProperty("DepositManagerWebView.HandelProBrandDomains")]
        public string HandelProBrandDomains { get; set; }

        [YamlProperty("DepositManagerWebView.AllianzmarketBrandDomains")]
        public string AllianzmarketBrandDomains { get; set; }

        [YamlProperty("DepositManagerWebView.FinanceSwiffyIntegrationService")]
        public string FinanceSwiffyIntegrationService { get; set; }

        [YamlProperty("DepositManagerWebView.ProcessIdCleanerTimeout")]
        public string ProcessIdCleanerTimeout { get; set; }

        [YamlProperty("DepositManagerWebView.FinanceDirectaIntegrationService")]
        public string FinanceDirectaIntegrationService { get; set; }

        [YamlProperty("DepositManagerWebView.FinanceVoltIntegrationService")]
        public string FinanceVoltIntegrationService { get; set; }

        [YamlProperty("DepositManagerWebView.FinancePayRetailersIntegrationService")]
        public string FinancePayRetailersIntegrationService { get; set; }

        [YamlProperty("DepositManagerWebView.FinancePayopIntegrationService")]
        public string FinancePayopIntegrationService { get; set; }

        [YamlProperty("DepositManagerWebView.TraderExternalDataGrpcServiceUrl")]
        public string TraderExternalDataGrpcServiceUrl { get; set; }
        
        [YamlProperty("DepositManagerWebView.PricesMyNoSqlServerReader")]
        public string PricesMyNoSqlServerReader { get; set; }

        [YamlProperty("DepositManagerWebView.ABSplits")]
        public Dictionary<string, string> ABSplits { get; set; }
    }
}