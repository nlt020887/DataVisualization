using JwtAuthenticationManager.Models;
using PortfolioApi.Model;

namespace PortfolioApi.Infrastructure
{
    public interface ITransaction
    {
        public Task<PagingResponseModel<List<TransactionDataModel>>> SearchListTransaction(SearchTransactionModel model);
        public Task<CreatedTransacionResponseModel> CreatedTransaction(TransactionDataModel transactionDataModel);        
        public Task<TransactionDataModel> GetTransactionByTransNo(string TransactionNo);
        public Task<int> Approve(TransactionApproveModel input);
        public Task<String> GetNewTransactionNo(string DealTypeName);
        public Task<int> Delete(TransactionDeleteModel input);
    }
}
