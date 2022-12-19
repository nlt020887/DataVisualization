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
    public class AssetController : ControllerBase
    {
        private readonly ILogger<AssetController> _logger;
        private readonly IAssetRepository _assetRepository;
                         
        public AssetController(ILogger<AssetController> logger,
            IAssetRepository assetRepository)
        {
            _logger = logger;
            _assetRepository = assetRepository;
        }


        [HttpPost]
        public async Task<ActionResult<Response>> GetAllAsset(SearchModel model)
        {
            try
            {
                var result = await _assetRepository.GetListAsset(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,ex.Message);
               return new Response { Status = "Error", Message = ex.Message, Data = null};
            }
        }

        [HttpGet]
        public async Task<ActionResult<Response>> GetFullListAsset()
        {
            try
            {
                var result = await _assetRepository.GetFullListAsset();
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> GetAllAssetPending(SearchModel model)
        {
            try
            {
                var result = await _assetRepository.GetListAssetPending(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpGet]
        public async Task<ActionResult<Response>> GetAssetPendingById(string AssetId)
        {
            try
            {
                var result = await _assetRepository.GetAssetPendingById(AssetId);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpGet]
        public async Task<ActionResult<Response>> GetAssetById(string AssetId)
        {
            try
            {
                var result = await _assetRepository.GetAssetById(AssetId);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> CreateAsset(AssetDataModel portfolioDataModel)
        {
            try
            {
                var userName = HttpContext.User.Identity.Name;
                portfolioDataModel.CreatedUser = userName;
                var result = await _assetRepository.CreateAsset(portfolioDataModel);
                return new Response { Status = "Success", Message = "Cập nhật thông tin danh mục tài sản thành công!", Data = JsonConvert.SerializeObject(result) };
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
                var result = await _assetRepository.Approve(portfolioApproveModel);
                return new Response { Status = "Success", Message = "Duyệt thông tin danh mục tài sản thành công!", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }
    }
}
