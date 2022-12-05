
using JwtAuthenticationManager.Models;
using PortfolioApi.Model;

namespace PortfoliApi.Infrastructure
{
    public interface IAssetMarketPriceRepository
    {
        public Task<PagingResponseModel<List<AssetMarketPriceDataModel>>> SearchAssetMarketPrice(SearchModel model);
        public Task<int>CreateAssetMarketPrice(AssetMarketPriceDataModel assetDataModel);
        public Task<PagingResponseModel<List<AssetMarketPricePendingDataModel>>> SearchAssetMarketPricePending(SearchModel model);
        public Task<AssetMarketPricePendingDataModel> GetAssetMarketPricePendingById(string AssetMarketPriceId);
        public Task<AssetMarketPriceDataModel> GetAssetMarketPriceById(string AssetMarketPriceId);
        public Task<int> Approve(AssetApproveModel AssetMarketPriceApproveModel);
    }
}
