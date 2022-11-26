using Dapper;
using PortfolioApi.Model;
using System.Data;

namespace PortfolioApi.Infrastructure
{
    public class TaxFeeRepository : ITaxFeeRepository
    {
        private readonly IConfiguration _configuration;
        private string ConnectionString
        {
            get { return _configuration.GetConnectionString("DefaultConnection"); }
        }
        public TaxFeeRepository(IConfiguration configuration)
        {
            _configuration = configuration;            
        }
        public async Task<IEnumerable<TaxFeeDataModel>> GetListTaxFee()
        {
            try
            {
                using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
                {
                    const string findAllQuery = "SELECT * FROM \"TaxFee\"";
                    var results = await db.QueryAsync<TaxFeeDataModel>(findAllQuery);
                    return results;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
