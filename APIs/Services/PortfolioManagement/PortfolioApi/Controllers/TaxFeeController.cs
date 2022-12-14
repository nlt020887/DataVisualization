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
    public class TaxFeeController : ControllerBase
    {
        private readonly ILogger<TaxFeeController> _logger;
        private readonly ITaxFeeRepository _taxFeeRepository;

        public TaxFeeController(ILogger<TaxFeeController> logger,
            ITaxFeeRepository taxFeeRepository)
        {
            _logger = logger;
            _taxFeeRepository = taxFeeRepository;
        }


        [HttpGet]
        public async Task<ActionResult<Response>> GetListTaxFee()
        {
            try
            {
                var result = await _taxFeeRepository.GetListTaxFee();
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result.ToList()) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }


        [HttpPost]
        public async Task<ActionResult<Response>> SearchListTaxFee(SearchModel model)
        {
            try
            {
                var result = await _taxFeeRepository.SearchListTaxFee(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> GetListTaxFeePending(SearchModel model)
        {
            try
            {
                var result = await _taxFeeRepository.GetListTaxFeePending(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpGet]
        public async Task<ActionResult<Response>> GetTaxFeePendingById(string TaxFeeId)
        {
            try
            {
                var result = await _taxFeeRepository.GetTaxFeePendingById(TaxFeeId);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpGet]
        public async Task<ActionResult<Response>> GetTaxFeeById(string TaxFeeId)
        {
            try
            {
                var result = await _taxFeeRepository.GetTaxFeeById(TaxFeeId);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> CreateTaxFee(TaxFeeDataModel taxFeeDataModel)
        {
            try
            {
                var userName = HttpContext.User.Identity.Name;
                if (taxFeeDataModel == null || string.IsNullOrEmpty(userName))
                    return new Response { Status = "Error", Message = "Dữ liệu không hợp lệ!", Data = null };
                if (string.IsNullOrEmpty(taxFeeDataModel.TaxFeeId) || string.IsNullOrEmpty(taxFeeDataModel.TaxFeeName))
                    return new Response { Status = "Error", Message = "Mã thuế phí và tên tên thuế phí không được trống!", Data = null };
                taxFeeDataModel.CreatedUser = userName;
                var result = await _taxFeeRepository.CreateTaxFee(taxFeeDataModel);
                return new Response { Status = "Success", Message = "Cập nhật thông tin phí thuế thành công!", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> Approve(TaxFeeApproveModel taxFeeApproveModel)
        {
            try
            {
                var userName = HttpContext.User.Identity.Name;
                if (taxFeeApproveModel == null || string.IsNullOrEmpty(userName)||string.IsNullOrEmpty(taxFeeApproveModel.TaxFeeId))
                    return new Response { Status = "Error", Message = "Dữ liệu không hợp lệ!", Data = null };
                taxFeeApproveModel.ApprovedUser = userName;
                var result = await _taxFeeRepository.Approve(taxFeeApproveModel);
                return new Response { Status = "Success", Message = "Duyệt thông tin phí thuế thành công!", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }
    }
}
