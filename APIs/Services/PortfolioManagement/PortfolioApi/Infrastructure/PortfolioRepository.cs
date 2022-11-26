using Dapper;
using PortfolioApi.Model;
using System.Data;
using Microsoft.Extensions.Configuration;
using static System.Net.Mime.MediaTypeNames;

namespace PortfolioApi.Infrastructure
{
	public class PortfolioRepository : IPortfolioRepository
	{
		private readonly IConfiguration _configuration;        
        private string ConnectionString
		{
			get {return _configuration.GetConnectionString("DefaultConnection"); }
		}
        public PortfolioRepository(IConfiguration configuration)
		{
			_configuration = configuration;                     
         }
			
        public async Task<int> Approve(PortfolioApproveModel portfolioApproveModel)
		{
            int resultCode = -1;
			using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
			{
				db.Open();
				var trans = db.BeginTransaction();
				try
				{
					const string StoreProcedure_approve = "\"approveportfolio\"";
					var param = new
					{
						portfolioid = portfolioApproveModel.PortfolioId,
						confirmstatus = portfolioApproveModel.ConfirmStatus,
						approveduser = portfolioApproveModel.ApprovedUser
					};

					var dynamicParameters = new DynamicParameters();
					dynamicParameters.AddDynamicParams(param);
					dynamicParameters.Add("@resultcode", dbType: DbType.Int32, direction: ParameterDirection.Output);
					var results = await db.ExecuteAsync(StoreProcedure_approve, dynamicParameters, trans, null, CommandType.StoredProcedure);
					resultCode = dynamicParameters.Get<int>("@resultcode");
					trans.Commit();
				}
                catch (Exception exx)
                {
                    trans.Rollback();
                    resultCode = -1;
                    throw exx;
                }
                finally
                {
                    db.Close();
                }
            }
            return resultCode;
        }

		public async Task<int> CreatePortfolio(PortfolioDataModel portfolioDataModel)
		{
			int resultCode = -1;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
				db.Open();
                var trans = db.BeginTransaction();
                try
				{
					const string StoreProcedure_createportfolio = "\"createportfolio\"";
					var param = new
					{
						portfolioid = portfolioDataModel.PortfolioId,
						portfolioname = portfolioDataModel.PortfolioName,
						taxfeeid = portfolioDataModel.TaxFeeId,
						createduser = portfolioDataModel.CreatedUser,
                        isactive = portfolioDataModel.IsActive
					};

					var dynamicParameters = new DynamicParameters();
					dynamicParameters.AddDynamicParams(param);
					dynamicParameters.Add("@resultcode", dbType: DbType.Int32, direction: ParameterDirection.Output);
					var results = await db.ExecuteAsync(StoreProcedure_createportfolio, dynamicParameters, trans, null, CommandType.StoredProcedure);
                    resultCode = dynamicParameters.Get<int>("@resultcode");

					if(resultCode==0)
					{
                        const string StoreProcedure_insertportfoliousers = "\"insertportfoliousers\"";
						if (portfolioDataModel.PortfolioUsers != null && portfolioDataModel.PortfolioUsers.Count > 0)
						{
							foreach (var item in portfolioDataModel.PortfolioUsers)
                            {
								var paramuser = new
								{
                                    portfolioid = portfolioDataModel.PortfolioId,
                                    userid = item.UserId,
                                    roletype = item.RoleType,
                                    recordno = portfolioDataModel.RecordNo
                                };

                                var dynamicParameters1 = new DynamicParameters();
                                dynamicParameters1.AddDynamicParams(paramuser);
                                dynamicParameters1.Add("@resultcode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                results = await db.ExecuteAsync(StoreProcedure_insertportfoliousers, dynamicParameters1, trans, null, CommandType.StoredProcedure);
                                resultCode = dynamicParameters.Get<int>("@resultcode");
                            }
						}
                    }	

                    trans.Commit();					
				}
				catch (Exception exx)
				{
					trans.Rollback();
					resultCode = -1;
					throw exx;
				}
				finally
				{
                    db.Close();
                }

          
            }
            return resultCode;
        }

		

		public async Task<IEnumerable<PortfolioDataModel>> GetAllPortfolio()
		{

			try
			{
				using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
				{
					const string findAllQuery = "SELECT * FROM \"Portfolio\"";
					var results = await db.QueryAsync<PortfolioDataModel>(findAllQuery);
					return results;
				}
			}
			catch (Exception ex)
			{

				throw ex;
			}

		}

		public async Task<IEnumerable<PortfolioDataModel>> GetAllPortfolioPending()
		{
            try
            {
                using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
                {
                    const string findAllQuery = "SELECT * FROM \"PortfolioPending\"";
                    var results = await db.QueryAsync<PortfolioDataModel>(findAllQuery);
                    return results;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public async Task<PortfolioDataModel> GetPortfolioById(string PortfolioId)
		{
            PortfolioDataModel result = null;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                const string findQueryById = "SELECT * FROM \"Portfolio\" where \"PortfolioId\"=@portfolioid";
                var parameters = new { portfolioid = PortfolioId};
                var results = await db.QuerySingleOrDefaultAsync<PortfolioDataModel>(findQueryById, parameters);
               // if (results!=null && results.Any())
                    result = results;
            }
            if (result != null)
            {
                var PortfolioUserList = await GetListPortfolioUserByPortfolioId(PortfolioId);
                {
                    result.PortfolioUsers = PortfolioUserList?.ToList();
                }
            }
            return result;
        }

		

		public async Task<PortfolioPendingDataModel> GetPortfolioPendingById(string PortfolioId)
		{
            PortfolioPendingDataModel result = null;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                const string findQueryById = "SELECT * FROM \"PortfolioPending\" where \"PortfolioId\"=@portfolioid";
                var parameters = new { portfolioid = PortfolioId };
                var results = await db.QuerySingleOrDefaultAsync<PortfolioPendingDataModel>(findQueryById, parameters);
                result = results;
            }
            if (result != null)
            {
                var PortfolioUserPendingList = await GetListPortfolioUserPendingByPortfolioId(PortfolioId);
                {
                    result.PortfolioUserPending = PortfolioUserPendingList?.ToList();
                }
            }
            return result;
        }


        async Task<IEnumerable<PortfolioUserDataModel>> GetListPortfolioUserByPortfolioId(string PortfolioId)
        {
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                const string findQueryById = "SELECT * FROM \"PortfolioUsers\" where \"PortfolioId\"=@portfolioid";
                var parameters = new { portfolioid = PortfolioId };
                var results = await db.QueryAsync<PortfolioUserDataModel>(findQueryById, parameters);
                return results;
            }
        }

        async Task<IEnumerable<PortfolioUsersPendingDataModel>> GetListPortfolioUserPendingByPortfolioId(string PortfolioId)
        {
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                const string findQueryById = "SELECT * FROM \"PortfolioUsersPending\" where \"PortfolioId\"=@portfolioid";
                var parameters = new { portfolioid = PortfolioId };
                var results = await db.QueryAsync<PortfolioUsersPendingDataModel>(findQueryById, parameters);
                return results;
            }
        }

    }
}
