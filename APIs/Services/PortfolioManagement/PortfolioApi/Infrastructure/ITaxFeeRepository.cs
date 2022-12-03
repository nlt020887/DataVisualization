using JwtAuthenticationManager.Models;
using PortfolioApi.Model;

namespace PortfolioApi.Infrastructure
{
    public interface ITaxFeeRepository
    {
        public Task<IEnumerable<TaxFeeDataModel>> GetListTaxFee();
        public Task<PagingResponseModel<List<TaxFeeDataModel>>> SearchListTaxFee(SearchModel model);
        public Task<int> CreateTaxFee(TaxFeeDataModel assetDataModel);
        public Task<PagingResponseModel<List<TaxFeePendingDataModel>>> GetListTaxFeePending(SearchModel model);
        public Task<TaxFeePendingDataModel> GetTaxFeePendingById(string TaxFeeId);
        public Task<TaxFeeDataModel> GetTaxFeeById(string TaxFeeId);
        public Task<int> Approve(TaxFeeApproveModel TaxFeeApproveModel);
    }
}
