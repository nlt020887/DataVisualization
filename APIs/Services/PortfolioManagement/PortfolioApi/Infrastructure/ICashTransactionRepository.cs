using JwtAuthenticationManager.Models;
using PortfolioApi.Model;

namespace PortfolioApi.Infrastructure
{
    public interface ICashTransactionRepository
    {
        public Task<PagingResponseModel<List<CashTransactionDataModel>>> SearchListCashTransaction(SearchCashTransactionModel model);
        public Task<CreatedCashTransacionResponseModel> CreatedCashTransaction(CashTransactionDataModel transactionDataModel);        
        public Task<CashTransactionDataModel> GetCashTransactionByTransNo(string TransactionNo);
        public Task<int> Approve(CashTransactionApproveModel input);
        public Task<String> GetNewCashTransactionNo(string DealTypeName);
        public Task<int> Delete(CashTransactionDeleteModel input);
    }
}
