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
    public class CashTransactionController : ControllerBase
    {
        private readonly ILogger<CashTransactionController> _logger;
        private readonly ICashTransactionRepository _CashTransactionRepository;

        public CashTransactionController(ILogger<CashTransactionController> logger,
            ICashTransactionRepository transactionRepository)
        {
            _logger = logger;
            _CashTransactionRepository = transactionRepository;
        }

        [HttpPost]
        public async Task<ActionResult<Response>> SearchListCashTransaction(SearchCashTransactionModel model)
        {
            try
            {
                var result = await _CashTransactionRepository.SearchListCashTransaction(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpGet]
        public async Task<ActionResult<Response>> GetCashTransactionByTransNo(string TransactionNo)
        {
            try
            {
                var result = await _CashTransactionRepository.GetCashTransactionByTransNo(TransactionNo);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> Approve(CashTransactionApproveModel model)
        {
            try
            {
               
                if(model==null ||  string.IsNullOrEmpty(model.TransactionNo))
                    return new Response { Status = "Error", Message = "Dữ liệu không hợp lệ!", Data = null };
                model.ApprovedUser = HttpContext.User.Identity.Name;
                var result = await _CashTransactionRepository.Approve(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> Delete(CashTransactionDeleteModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.TransactionNo))
                    return new Response { Status = "Error", Message = "Dữ liệu không hợp lệ!", Data = null };
                model.DeletedUser = HttpContext.User.Identity.Name;
                var result = await _CashTransactionRepository.Delete(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> CreateCashTransaction(CashTransactionDataModel model)
        {
            try
            {
                if(model==null)
                    return new Response { Status = "Error", Message = "Dữ liệu không hợp lệ!", Data = null };                
                if (string.IsNullOrEmpty(model.PortfolioId))
                    return new Response { Status = "Error", Message = "Vui lòng chọn danh mục đầu tư!", Data = null };
                if (model.Amount<=0)
                    return new Response { Status = "Error", Message = "Vui lòng nhập số lượng!", Data = null };
                if (model.ExchangeRate<=0)
                    return new Response { Status = "Error", Message = "Tỷ giá phải lớn hơn 0!", Data = null };
                if (string.IsNullOrEmpty(model.Currency))
                    return new Response { Status = "Error", Message = "Vui lòng chọn loại tiền!", Data = null };
                if (model.ValueDate==null || model.ValueDate < DateTime.Now)
                    return new Response { Status = "Error", Message = "Ngày giá trị phải lớn hơn bằng ngày hiện tại!", Data = null };
                var userName = HttpContext.User.Identity.Name;
                model.CreatedUser = userName;
                
                var result = await _CashTransactionRepository.CreatedCashTransaction(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpGet]
        public async Task<ActionResult<Response>> GetNewCashTransactionNo(string DealTypeName)
        {
            try
            {
                if (string.IsNullOrEmpty(DealTypeName))
                    return new Response { Status = "Error", Message = "Dữ liệu không hợp lệ!", Data = null };

                var result = await _CashTransactionRepository.GetNewCashTransactionNo(DealTypeName);
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
