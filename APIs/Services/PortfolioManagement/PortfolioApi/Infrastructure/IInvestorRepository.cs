using JwtAuthenticationManager.Models;
using PortfolioApi.Model;

namespace PortfolioApi.Infrastructure
{
    public interface IInvestorRepository
    {
        public Task<PagingResponseModel<List<InvestorDataModel>>> SearchListInvestor(SearchInvestorModel model);
        public Task<CreatedInvestorResponseModel> CreatedInvestor(InvestorDataModel transactionDataModel);        
        public Task<InvestorDataModel> GetInvestorByTransNo(string InvestorNo);
        public Task<int> Approve(InvestorApproveModel input);
        public Task<String> GetNewInvestorNo(string DealTypeName);
        public Task<int> Delete(InvestorDeleteModel input);
    }
}
