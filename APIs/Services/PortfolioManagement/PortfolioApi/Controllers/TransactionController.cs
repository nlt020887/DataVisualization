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
    public class TransactionController : ControllerBase
    {
        private readonly ILogger<TransactionController> _logger;
        private readonly ITransactionRepository _TransactionRepository;

        public TransactionController(ILogger<TransactionController> logger,
            ITransactionRepository transactionRepository)
        {
            _logger = logger;
            _TransactionRepository = transactionRepository;
        }

        [HttpPost]
        public async Task<ActionResult<Response>> SearchListTransaction(SearchTransactionModel model)
        {
            try
            {
                var result = await _TransactionRepository.SearchListTransaction(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpGet]
        public async Task<ActionResult<Response>> GetTransactionByTransNo(string TransactionNo)
        {
            try
            {
                var result = await _TransactionRepository.GetTransactionByTransNo(TransactionNo);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> Approve(TransactionApproveModel model)
        {
            try
            {
               
                if(model==null ||  string.IsNullOrEmpty(model.TransactionNo))
                    return new Response { Status = "Error", Message = "Dữ liệu không hợp lệ!", Data = null };
                model.ApprovedUser = HttpContext.User.Identity.Name;
                var result = await _TransactionRepository.Approve(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> Delete(TransactionDeleteModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.TransactionNo))
                    return new Response { Status = "Error", Message = "Dữ liệu không hợp lệ!", Data = null };
                model.DeletedUser = HttpContext.User.Identity.Name;
                var result = await _TransactionRepository.Delete(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpPost]
        public async Task<ActionResult<Response>> CreateTransaction(TransactionDataModel model)
        {
            try
            {
                if(model==null)
                    return new Response { Status = "Error", Message = "Dữ liệu không hợp lệ!", Data = null };
                if (string.IsNullOrEmpty(model.AssetId))
                    return new Response { Status = "Error", Message = "Vui lòng chọn tài sản!", Data = null };
                if (string.IsNullOrEmpty(model.PortfolioId))
                    return new Response { Status = "Error", Message = "Vui lòng chọn danh mục đầu tư!", Data = null };
                if (string.IsNullOrEmpty(model.TaxFeeId))
                    return new Response { Status = "Error", Message = "Vui lòng chọn Loại thuế phí!", Data = null };
                if (model.TransactionAmount<=0)
                    return new Response { Status = "Error", Message = "Vui lòng nhập Số lượng giao dịch!", Data = null };
                if (model.TransactionPrice <= 0)
                    return new Response { Status = "Error", Message = "Vui lòng nhập giá giao dịch!", Data = null };
                if (model.ValueDate==null || model.ValueDate < DateTime.Now)
                    return new Response { Status = "Error", Message = "Ngày giá trị phải lớn hơn bằng ngày hiện tại!", Data = null };
                var userName = HttpContext.User.Identity.Name;
                model.CreatedUser = userName;
                
                var result = await _TransactionRepository.CreatedTransaction(model);
                return new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response { Status = "Error", Message = ex.Message, Data = null };
            }
        }

        [HttpGet]
        public async Task<ActionResult<Response>> GetNewTransactionNo(string DealTypeName)
        {
            try
            {
                if (string.IsNullOrEmpty(DealTypeName))
                    return new Response { Status = "Error", Message = "Dữ liệu không hợp lệ!", Data = null };

                var result = await _TransactionRepository.GetNewTransactionNo(DealTypeName);
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
