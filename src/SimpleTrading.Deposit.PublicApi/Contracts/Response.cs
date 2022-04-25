using Finance.PciDssIntegration.GrpcContracts.Contracts;

namespace SimpleTrading.Deposit.PublicApi.Contracts
{
    public class DepositResponse<TResponseData> where TResponseData: class
    {
        public DepositRequestStatus Status { get; set; }
        public TResponseData Data { get; set; }

        public DepositResponse(TResponseData data, DepositRequestStatus status)
        {
            Data = data;
            Status = status;
        }

        public static DepositResponse<TResponseData> Success(TResponseData data)
        {
            return new DepositResponse<TResponseData>(data, DepositRequestStatus.Success);
        }

        public static DepositResponse<TResponseData> Create(TResponseData data, DepositRequestStatus requestStatus )
        {
            return new DepositResponse<TResponseData>(data, requestStatus);
        }
    }
}
