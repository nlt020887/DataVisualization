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
	public class AssetRepository : IAssetRepository
    {
		private readonly IConfiguration _configuration;        
        private string ConnectionString
		{
			get {return _configuration.GetConnectionString("DefaultConnection"); }
		}
        public AssetRepository(IConfiguration configuration)
		{
			_configuration = configuration;                     
         }
			
        public async Task<int> Approve(AssetApproveModel AssetApproveModel)
		{
            int resultCode = -1;
			using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
			{
				db.Open();
				var trans = db.BeginTransaction();
				try
				{
					const string StoreProcedure_approve = "\"approveasset\"";
					var param = new
					{
                        v_assetid = AssetApproveModel.AssetId,
                        v_confirmstatus = AssetApproveModel.ConfirmStatus,
                        v_approveduser = AssetApproveModel.ApprovedUser
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

		public async Task<int> CreateAsset(AssetDataModel AssetDataModel)
		{
			int resultCode = -1;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
				db.Open();
                var trans = db.BeginTransaction();
                try
				{
					const string StoreProcedure_createAsset = "\"insertasset\"";
					var param = new
					{
						v_assetid = AssetDataModel.AssetId,
						v_assetname = AssetDataModel.AssetName,
                        v_assettype = AssetDataModel.AssetType,
                        v_unitprice = AssetDataModel.UnitPrice.Value,
						v_createduser = AssetDataModel.CreatedUser,
                        v_isactive = AssetDataModel.IsActive
					};

					var dynamicParameters = new DynamicParameters();
					dynamicParameters.AddDynamicParams(param);
					dynamicParameters.Add("@resultcode", dbType: DbType.Int32, direction: ParameterDirection.Output);
					var results = await db.ExecuteAsync(StoreProcedure_createAsset, dynamicParameters, trans, null, CommandType.StoredProcedure);
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

		

		public async Task<PagingResponseModel<List<AssetDataModel>>> GetListAsset(SearchModel model)
		{


            int maxPagSize = 50;
            model.PageSize = (model.PageSize > 0 && model.PageSize <= maxPagSize) ? model.PageSize : maxPagSize;

            int skip = (model.PageNumber - 1) * model.PageSize;
            int take = model.PageSize;

            const string findAllQuery = @"select count(*) 
                            from Asset 
                            where (1=1) and (@keyword is null or assetId=@keyword or assetname like '%'||@keyword||'%');

                        select * from Asset 
                        where (1=1) and (@keyword is null or assetid=@keyword or assetName like '%'||@keyword||'%')
                        offset @Skip limit @Take;"
            ;
            try
            {
                using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
				{
                    var reader = await db.QueryMultipleAsync(findAllQuery, new { Skip = skip, Take = take, keyword = model.Keyword });

                    int count = reader.Read<int>().FirstOrDefault();

                    List<AssetDataModel> AssetDataModelList = reader.Read<AssetDataModel>().ToList();

                    var result = new PagingResponseModel<List<AssetDataModel>>(AssetDataModelList, count, model.PageNumber, model.PageSize);
                    return result;

				}
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		public async Task<PagingResponseModel<List<AssetPendingDataModel>>> GetListAssetPending(SearchModel model)
        {
            int maxPagSize = 50;
            model.PageSize = (model.PageSize > 0 && model.PageSize <= maxPagSize) ? model.PageSize : maxPagSize;
            int skip = (model.PageNumber - 1) * model.PageSize;
            int take = model.PageSize;

            const string findAllQuery = @"select count(*) 
                            from AssetPending 
                            where (1=1) and (@keyword is null or assetId=@keyword or assetname like '%'||@keyword||'%');

                        select * from AssetPending 
                        where (1=1) and (@keyword is null or assetid=@keyword or assetName like '%'||@keyword||'%')
                        offset @Skip limit @Take;"
            ;
            try
            {
                using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
                {
                    var reader = await db.QueryMultipleAsync(findAllQuery, new { Skip = skip, Take = take, keyword=model.Keyword });

                    int count = reader.Read<int>().FirstOrDefault();

                    List<AssetPendingDataModel> AssetPendingDataModelList = reader.Read<AssetPendingDataModel>().ToList();
                    var result = new PagingResponseModel<List<AssetPendingDataModel>>(AssetPendingDataModelList, count, model.PageNumber, model.PageSize);
                    return result;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<AssetDataModel> GetAssetById(string AssetId)
		{
            AssetDataModel result = null;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                const string findQueryById = @"SELECT trim(assetid) AssetId, assetname, assettype, createduser, createdate, updateuser,
                    updatedate, status, recordno, approveduser, approveddate, isactive, isdeleted, unitprice
	                FROM public.asset
                    where assetId = @Assetid";
                var parameters = new { Assetid = AssetId};
                var results = await db.QuerySingleOrDefaultAsync<AssetDataModel>(findQueryById, parameters);
               // if (results!=null && results.Any())
                    result = results;
            }
          
            return result;
        }

		

		public async Task<AssetPendingDataModel> GetAssetPendingById(string AssetId)
		{
            AssetPendingDataModel result = null;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                const string findQueryById = @"SELECT trim(assetid) assetid, assetname, 
assettype, createduser, createdate, updateuser, 
updatedate, status, recordno, approveduser, approveddate, isactive, isdeleted, unitprice
	FROM public.assetpending where AssetId=@Assetid";
                var parameters = new { Assetid = AssetId };
                var results = await db.QuerySingleOrDefaultAsync<AssetPendingDataModel>(findQueryById, parameters);
                result = results;
            }
           
            return result;
        }

        public async Task<List<AssetDataModel>> GetFullListAsset()
        {
            List<AssetDataModel> result = null;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                const string findQueryById = "SELECT AssetId,AssetName, AssetType, unitprice FROM Asset where IsActive = true";
                
                var results = await db.QueryAsync<AssetDataModel>(findQueryById);
                result = results.ToList();
            }

            return result;
        }
    }
}
