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
    }
}
