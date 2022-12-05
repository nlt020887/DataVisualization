using Dapper;
using PortfolioApi.Model;
using System.Data;
using JwtAuthenticationManager.Models;
using Microsoft.Extensions.Configuration;
using static System.Net.Mime.MediaTypeNames;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PortfoliApi.Infrastructure;

namespace PortfolioApi.Infrastructure
{
	public class AssetMarketPriceRepository : IAssetMarketPriceRepository
    {
		private readonly IConfiguration _configuration;        
        private string ConnectionString
		{
			get {return _configuration.GetConnectionString("DefaultConnection"); }
		}
        public AssetMarketPriceRepository(IConfiguration configuration)
		{
			_configuration = configuration;                     
         }
			
        public async Task<int> Approve(AssetApproveModel approveModel)
		{
            int resultCode = -1;
			using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
			{
				db.Open();
				var trans = db.BeginTransaction();
				try
				{
					const string StoreProcedure_approve = "\"approveassetmarketprice\"";
					var param = new
					{
                        v_assetid = approveModel.AssetId,
                        v_confirmstatus = approveModel.ConfirmStatus,
                        v_approveduser = approveModel.ApprovedUser
					};

					var dynamicParameters = new DynamicParameters();
					dynamicParameters.AddDynamicParams(param);
					dynamicParameters.Add("@resultcode", dbType: DbType.Int32, direction: ParameterDirection.Output);
					var results = await db.ExecuteAsync(StoreProcedure_approve, dynamicParameters, trans, null, CommandType.StoredProcedure);
					resultCode = dynamicParameters.Get<int>("@resultcode");
                    if (resultCode == 0)
                        trans.Commit();
                    else
                        trans.Rollback();
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

		public async Task<int> CreateAssetMarketPrice(AssetMarketPriceDataModel assetMarketPriceDataModel)
		{
			int resultCode = -1;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
				db.Open();
                var trans = db.BeginTransaction();
                try
				{
					const string StoreProcedure_createAssetMarketPrice = "\"insertassetmarketprice\"";
					var param = new
					{
						v_assetid = assetMarketPriceDataModel.AssetId,
                        v_valuedate = assetMarketPriceDataModel.ValueDate,
                        v_assetmarketprice = assetMarketPriceDataModel.AssetMarketPrice,                     
						v_createduser = assetMarketPriceDataModel.CreatedUser,
                        v_isactive = assetMarketPriceDataModel.IsActive
					};

					var dynamicParameters = new DynamicParameters();
					dynamicParameters.AddDynamicParams(param);
					dynamicParameters.Add("@resultcode", dbType: DbType.Int32, direction: ParameterDirection.Output);
					var results = await db.ExecuteAsync(StoreProcedure_createAssetMarketPrice, dynamicParameters, trans, null, CommandType.StoredProcedure);
                    resultCode = dynamicParameters.Get<int>("@resultcode");

                    if (resultCode == 0)
                    {
                        trans.Commit();
                    }
                    else
                        trans.Rollback();
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

		

		public async Task<PagingResponseModel<List<AssetMarketPriceDataModel>>> SearchAssetMarketPrice(SearchModel model)
		{


            int maxPagSize = 50;
            model.PageSize = (model.PageSize > 0 && model.PageSize <= maxPagSize) ? model.PageSize : maxPagSize;

            int skip = (model.PageNumber - 1) * model.PageSize;
            int take = model.PageSize;

            const string findAllQuery = @"select count(*) 
                        from AssetMarketPrice
                        join asset on asset.assetid =AssetMarketPrice.assetid
                        where (@keyword is null or AssetMarketPrice.assetId=@keyword or asset.assetname like '%'||@keyword||'%');

                        select * from AssetMarketPrice
                        join asset on asset.assetid =AssetMarketPrice.assetid
                        where (@keyword is null or AssetMarketPrice.assetId=@keyword or asset.assetname like '%'||@keyword||'%')
                        offset @Skip limit @Take;"
            ;
            try
            {
                using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
				{
                    var reader = await db.QueryMultipleAsync(findAllQuery, new { Skip = skip, Take = take, keyword = model.Keyword });

                    int count = reader.Read<int>().FirstOrDefault();

                    List<AssetMarketPriceDataModel> AssetMarketPriceDataModelList = reader.Read<AssetMarketPriceDataModel>().ToList();

                    var result = new PagingResponseModel<List<AssetMarketPriceDataModel>>(AssetMarketPriceDataModelList, count, model.PageNumber, model.PageSize);
                    return result;

				}
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		public async Task<PagingResponseModel<List<AssetMarketPricePendingDataModel>>> SearchAssetMarketPricePending(SearchModel model)
        {
            int maxPagSize = 50;
            model.PageSize = (model.PageSize > 0 && model.PageSize <= maxPagSize) ? model.PageSize : maxPagSize;
            int skip = (model.PageNumber - 1) * model.PageSize;
            int take = model.PageSize;

            const string findAllQuery = @"select count(*) 
                            from AssetMarketPricePending
                            join asset on asset.assetid =AssetMarketPricePending.assetid
                            where (@keyword is null or AssetMarketPricePending.assetId=@keyword or asset.assetname like '%'||@keyword||'%');

                        select * from AssetMarketPricePending
                        join asset on asset.assetid =AssetMarketPricePending.assetid
                        where (@keyword is null or AssetMarketPricePending.assetId=@keyword or asset.assetname like '%'||@keyword||'%')
                        offset @Skip limit @Take;"
            ;
            try
            {
                using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
                {
                    var reader = await db.QueryMultipleAsync(findAllQuery, new { Skip = skip, Take = take, keyword=model.Keyword });

                    int count = reader.Read<int>().FirstOrDefault();

                    List<AssetMarketPricePendingDataModel> assetMarketPricePendingDataModelList = reader.Read<AssetMarketPricePendingDataModel>().ToList();
                    var result = new PagingResponseModel<List<AssetMarketPricePendingDataModel>>(assetMarketPricePendingDataModelList, count, model.PageNumber, model.PageSize);
                    return result;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<AssetMarketPriceDataModel> GetAssetMarketPriceById(string AssetId)
		{
            AssetMarketPriceDataModel result = null;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                const string findQueryById = "SELECT * FROM AssetMarketPrice where assetId=@Assetid";
                var parameters = new { Assetid = AssetId};
                var results = await db.QuerySingleOrDefaultAsync<AssetMarketPriceDataModel>(findQueryById, parameters);
               // if (results!=null && results.Any())
                    result = results;
            }
          
            return result;
        }

		

		public async Task<AssetMarketPricePendingDataModel> GetAssetMarketPricePendingById(string AssetId)
		{
            AssetMarketPricePendingDataModel result = null;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                const string findQueryById = "SELECT * FROM AssetMarketPricePending where AssetId=@Assetid";
                var parameters = new { Assetid = AssetId };
                var results = await db.QuerySingleOrDefaultAsync<AssetMarketPricePendingDataModel>(findQueryById, parameters);
                result = results;
            }
           
            return result;
        }

     
    }
}
