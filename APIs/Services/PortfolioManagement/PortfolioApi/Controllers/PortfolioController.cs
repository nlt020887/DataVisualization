using JwtAuthenticationManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PortfolioApi.Infrastructure;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PortfolioApi.Model;

namespace PortfolioApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class PortfolioController : ControllerBase
    {
        private readonly ILogger<PortfolioController> _logger;
        private readonly IPortfolioRepository _portfolioRepository;
        
        public PortfolioController(ILogger<PortfolioController> logger,
            IPortfolioRepository portfolioRepository)
        {
            _logger = logger;
            _portfolioRepository = portfolioRepository;
        }


        [HttpPost]
        public async Task<ActionResult<Response>> GetAllPortfolio(SearchModel model)
        {
            try
            {
                var result = await _portfolioRepository.GetListPortfolio(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,ex.Message);
               return new Response { Status = "Error", Message = ex.Message, Data = null};
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> GetAllPortfolioPending(SearchModel model)
        {
            try
            {
                var result = await _portfolioRepository.GetListPortfolioPending(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpGet]
        public async Task<ActionResult<Response>> GetPortfolioPendingById(string PortfolioId)
        {
            try
            {
                var result = await _portfolioRepository.GetPortfolioPendingById(PortfolioId);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpGet]
        public async Task<ActionResult<Response>> GetPortfolioById(string PortfolioId)
        {
            try
            {
                var result = await _portfolioRepository.GetPortfolioById(PortfolioId);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> CreatePortfolio(PortfolioDataModel portfolioDataModel)
        {
            try
            {
                var userName = HttpContext.User.Identity.Name;
                if (portfolioDataModel == null || string.IsNullOrEmpty(userName))
                    return new Response { Status = "Error", Message = "Dữ liệu không hợp lệ!", Data = null };
                if (string.IsNullOrEmpty(portfolioDataModel.PortfolioId) || string.IsNullOrEmpty(portfolioDataModel.PortfolioName))
                    return new Response { Status = "Error", Message = "Mã danh mục và tên danh mục không được trống!", Data = null };
                portfolioDataModel.CreatedUser = userName;
                var result = await _portfolioRepository.CreatePortfolio(portfolioDataModel);
                return new Response { Status = "Success", Message = "Cập nhật thông tin danh mục tài sản thành công!", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> Approve(PortfolioApproveModel portfolioApproveModel)
        {
            try
            {
                var userName = HttpContext.User.Identity.Name;
                portfolioApproveModel.ApprovedUser = userName;
                var result = await _portfolioRepository.Approve(portfolioApproveModel);
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
