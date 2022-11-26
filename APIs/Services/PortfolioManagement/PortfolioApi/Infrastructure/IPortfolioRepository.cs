using PortfolioApi.Model;

namespace PortfolioApi.Infrastructure
{
    public interface IPortfolioRepository
    {
        public Task<IEnumerable<PortfolioDataModel>> GetAllPortfolio();
        public Task<int>CreatePortfolio(PortfolioDataModel portfolioDataModel);        
        public Task<IEnumerable<PortfolioDataModel>> GetAllPortfolioPending();
        public Task<PortfolioPendingDataModel> GetPortfolioPendingById(string PortfolioId);
        public Task<PortfolioDataModel> GetPortfolioById(string PortfolioId);
        public Task<int> Approve(PortfolioApproveModel portfolioApproveModel);
    }
}
