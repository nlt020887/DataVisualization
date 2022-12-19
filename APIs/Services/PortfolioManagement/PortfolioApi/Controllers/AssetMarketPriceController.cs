using JwtAuthenticationManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PortfolioApi.Infrastructure;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PortfolioApi.Model;
using PortfoliApi.Infrastructure;

namespace PortfolioApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class AssetMarketPriceController : ControllerBase
    {
        private readonly ILogger<AssetMarketPriceController> _logger;
        private readonly IAssetMarketPriceRepository _assetMarketPriceRepository;
                         
        public AssetMarketPriceController(ILogger<AssetMarketPriceController> logger,
            IAssetMarketPriceRepository assetMarketPriceRepository)
        {
            _logger = logger;
            _assetMarketPriceRepository = assetMarketPriceRepository;
        }


        [HttpPost]
        public async Task<ActionResult<Response>> SearchAssetMarketPrice(SearchModel model)
        {
            try
            {
                var result = await _assetMarketPriceRepository.SearchAssetMarketPrice(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,ex.Message);
               return new Response { Status = "Error", Message = ex.Message, Data = null};
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> SearchAssetMarketPricePending(SearchModel model)
        {
            try
            {
                var result = await _assetMarketPriceRepository.SearchAssetMarketPricePending(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpGet]
        public async Task<ActionResult<Response>> GetAssetMarketPricePendingById(string AssetId)
        {
            try
            {
                var result = await _assetMarketPriceRepository.GetAssetMarketPricePendingById(AssetId);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpGet]
        public async Task<ActionResult<Response>> GetAssetMarketPriceById(string AssetId)
        {
            try
            {
                var result = await _assetMarketPriceRepository.GetAssetMarketPriceById(AssetId);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> CreateAssetMarketPrice(AssetMarketPriceDataModel portfolioDataModel)
        {
            try
            {
                var userName = HttpContext.User.Identity.Name;
                portfolioDataModel.CreatedUser = userName;
                var result = await _assetMarketPriceRepository.CreateAssetMarketPrice(portfolioDataModel);
                return new Response { Status = "Success", Message = "Cập nhật thông tin giá trị tài sản thành công!", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> Approve(AssetApproveModel portfolioApproveModel)
        {
            try
            {
                var userName = HttpContext.User.Identity.Name;
                portfolioApproveModel.ApprovedUser = userName;
                var result = await _assetMarketPriceRepository.Approve(portfolioApproveModel);
                return new Response { Status = "Success", Message = "Duyệt thông tin giá trị tài sản thành công!", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }
    }
}
