
using JwtAuthenticationManager.Models;
using PortfolioApi.Model;

namespace PortfolioApi.Infrastructure
{
    public interface IPortfolioRepository
    {
        public Task<PagingResponseModel<List<PortfolioDataModel>>> GetListPortfolio(SearchModel model);
        public Task<List<PortfolioTaxFeeModel>> GetListPortfolioTaxFeeByUser(string username);
        public Task<int>CreatePortfolio(PortfolioDataModel portfolioDataModel);
        public Task<PagingResponseModel<List<PortfolioPendingDataModel>>> GetListPortfolioPending(SearchModel model);
        public Task<PortfolioPendingDataModel> GetPortfolioPendingById(string PortfolioId);
        public Task<PortfolioDataModel> GetPortfolioById(string PortfolioId);
        public Task<int> Approve(PortfolioApproveModel portfolioApproveModel);
    }
}
