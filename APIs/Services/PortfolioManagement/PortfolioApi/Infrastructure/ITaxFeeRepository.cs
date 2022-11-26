using PortfolioApi.Model;

namespace PortfolioApi.Infrastructure
{
    public interface ITaxFeeRepository
    {
        public Task<IEnumerable<TaxFeeDataModel>> GetListTaxFee();
    }
}
