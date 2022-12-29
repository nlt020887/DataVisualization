using JwtAuthenticationManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PortfoliApi.Infrastructure;
using PortfolioApi.Infrastructure;
using PortfolioApi.Model;

namespace PortfolioApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class InvestorController : ControllerBase
    {
        private readonly ILogger<InvestorController> _logger;
        private readonly IInvestorRepository _InvestorRepository;

        public InvestorController(ILogger<InvestorController> logger,
            IInvestorRepository transactionRepository)
        {
            _logger = logger;
            _InvestorRepository = transactionRepository;
        }

        [HttpPost]
        public async Task<ActionResult<Response>> SearchListInvestor(SearchInvestorModel model)
        {
            try
            {
                var result = await _InvestorRepository.SearchListInvestor(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpGet]
        public async Task<ActionResult<Response>> GetInvestorByTransNo(string InvestorNo)
        {
            try
            {
                var result = await _InvestorRepository.GetInvestorByTransNo(InvestorNo);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> Approve(InvestorApproveModel model)
        {
            try
            {
               
                if(model==null ||  string.IsNullOrEmpty(model.InvestorNo))
                    return new Response { Status = "Error", Message = "Dữ liệu không hợp lệ!", Data = null };
                model.ApprovedUser = HttpContext.User.Identity.Name;
                var result = await _InvestorRepository.Approve(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> Delete(InvestorDeleteModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.InvestorNo))
                    return new Response { Status = "Error", Message = "Dữ liệu không hợp lệ!", Data = null };
                model.DeletedUser = HttpContext.User.Identity.Name;
                var result = await _InvestorRepository.Delete(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> CreateInvestor(InvestorDataModel model)
        {
            try
            {
                if(model==null)
                    return new Response { Status = "Error", Message = "Dữ liệu không hợp lệ!", Data = null };
                if (string.IsNullOrEmpty(model.InvestorUser))
                    return new Response { Status = "Error", Message = "Vui lòng chọn tài sản!", Data = null };
                if (string.IsNullOrEmpty(model.PortfolioId))
                    return new Response { Status = "Error", Message = "Vui lòng chọn danh mục đầu tư!", Data = null };
                if (model.CertPrice<=0)
                    return new Response { Status = "Error", Message = "Vui lòng nhập Số lượng giao dịch!", Data = null };
                if (model.NoOfCert <= 0)
                    return new Response { Status = "Error", Message = "Vui lòng nhập giá giao dịch!", Data = null };
                if (model.ValueDate==null || model.ValueDate < DateTime.Now)
                    return new Response { Status = "Error", Message = "Ngày giá trị phải lớn hơn bằng ngày hiện tại!", Data = null };
                var userName = HttpContext.User.Identity.Name;
                model.CreatedUser = userName;
                
                var result = await _InvestorRepository.CreatedInvestor(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpGet]
        public async Task<ActionResult<Response>> GetNewInvestorNo(string DealTypeName)
        {
            try
            {
                if (string.IsNullOrEmpty(DealTypeName))
                    return new Response { Status = "Error", Message = "Dữ liệu không hợp lệ!", Data = null };

                var result = await _InvestorRepository.GetNewInvestorNo(DealTypeName);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }
    }
}
