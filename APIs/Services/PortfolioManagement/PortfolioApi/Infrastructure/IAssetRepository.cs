
using JwtAuthenticationManager.Models;
using PortfolioApi.Model;

namespace PortfoliApi.Infrastructure
{
    public interface IAssetRepository
    {
        public Task<PagingResponseModel<List<AssetDataModel>>> GetListAsset(SearchModel model);
        public Task<int>CreateAsset(AssetDataModel assetDataModel);
        public Task<PagingResponseModel<List<AssetPendingDataModel>>> GetListAssetPending(SearchModel model);
        public Task<AssetPendingDataModel> GetAssetPendingById(string AssetId);
        public Task<AssetDataModel> GetAssetById(string AssetId);
        public Task<int> Approve(AssetApproveModel AssetApproveModel);
    }
}
