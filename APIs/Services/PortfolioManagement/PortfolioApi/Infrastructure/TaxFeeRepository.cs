using Dapper;
using JwtAuthenticationManager.Models;
using PortfolioApi.Model;
using System;
using System.Data;
using static System.Net.Mime.MediaTypeNames;

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
                    const string findAllQuery = @"SELECT trim(taxfeeid) taxfeeid, taxfeename, valuedate, taxrate, feerate, createduser, 
createdate, updateuser, updatedate, status, recordno, approveduser, approveddate, isactive 
FROM TaxFee;";
                    var results = await db.QueryAsync<TaxFeeDataModel>(findAllQuery);
                    return results;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<PagingResponseModel<List<TaxFeeDataModel>>> SearchListTaxFee(SearchModel model)
        {
            int maxPagSize = 50;
            model.PageSize = (model.PageSize > 0 && model.PageSize <= maxPagSize) ? model.PageSize : maxPagSize;

            int skip = (model.PageNumber - 1) * model.PageSize;
            int take = model.PageSize;

            const string findAllQuery = @"select count(*) 
                            from TaxFee 
                            where (1=1) and (@keyword is null or taxfeeId=@keyword or taxfeename like '%'||@keyword||'%');

                        select * from TaxFee 
                        where (1=1) and (@keyword is null or taxfeeId=@keyword or taxfeename like '%'||@keyword||'%')
                        offset @Skip limit @Take;"
            ;
            try
            {
                using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
                {
                    var reader = await db.QueryMultipleAsync(findAllQuery, new { Skip = skip, Take = take, keyword = model.Keyword });

                    int count = reader.Read<int>().FirstOrDefault();

                    List<TaxFeeDataModel> AssetDataModelList = reader.Read<TaxFeeDataModel>().ToList();

                    var result = new PagingResponseModel<List<TaxFeeDataModel>>(AssetDataModelList, count, model.PageNumber, model.PageSize);
                    return result;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<int> CreateTaxFee(TaxFeeDataModel taxfeeDataModel)
        {
            int resultCode = -1;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                db.Open();
                var trans = db.BeginTransaction();
                try
                {
                  
                    const string StoreProcedure_createTaxFee = "\"inserttaxfee\"";
                    var param = new
                    {
                        v_taxfeeid = taxfeeDataModel.TaxFeeId,
                        v_taxfeename = taxfeeDataModel.TaxFeeName,
                        v_valuedate = taxfeeDataModel.ValueDate,
                        v_taxrate = taxfeeDataModel.TaxRate,
                        v_feerate = taxfeeDataModel.FeeRate,
                        v_isactive = taxfeeDataModel.IsActive,
                        v_createduser = taxfeeDataModel.CreatedUser
                    };

                    var dynamicParameters = new DynamicParameters();
                    dynamicParameters.AddDynamicParams(param);
                    dynamicParameters.Add("@resultcode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    var results = await db.ExecuteAsync(StoreProcedure_createTaxFee, dynamicParameters, trans, null, CommandType.StoredProcedure);
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

        public async Task<PagingResponseModel<List<TaxFeePendingDataModel>>> GetListTaxFeePending(SearchModel model)
        {
            int maxPagSize = 50;
            model.PageSize = (model.PageSize > 0 && model.PageSize <= maxPagSize) ? model.PageSize : maxPagSize;
            int skip = (model.PageNumber - 1) * model.PageSize;
            int take = model.PageSize;

            const string findAllQuery = @"select count(*) 
                            from TaxFeePending 
                            where (1=1) and (@keyword is null or taxfeeId=@keyword or taxfeename like '%'||@keyword||'%');

                        select * from TaxFeePending 
                        where (1=1) and (@keyword is null or taxfeeId=@keyword or taxfeename like '%'||@keyword||'%')
                        offset @Skip limit @Take;"
            ;
            try
            {
                using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
                {
                    var reader = await db.QueryMultipleAsync(findAllQuery, new { Skip = skip, Take = take, keyword = model.Keyword });

                    int count = reader.Read<int>().FirstOrDefault();

                    List<TaxFeePendingDataModel> AssetPendingDataModelList = reader.Read<TaxFeePendingDataModel>().ToList();
                    var result = new PagingResponseModel<List<TaxFeePendingDataModel>>(AssetPendingDataModelList, count, model.PageNumber, model.PageSize);
                    return result;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<TaxFeePendingDataModel> GetTaxFeePendingById(string TaxFeeId)
        {
            TaxFeePendingDataModel result = null;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                const string findQueryById = @"SELECT trim(taxfeeid) taxfeeid, taxfeename, valuedate, taxrate, feerate, createduser, createdate, 
updateuser, updatedate, status, recordno, approveduser, approveddate, isactive
FROM TaxfeePending where TaxFeeId=@TaxfeeId";
                var parameters = new { TaxfeeId = TaxFeeId };
                var results = await db.QuerySingleOrDefaultAsync<TaxFeePendingDataModel>(findQueryById, parameters);
                // if (results!=null && results.Any())
                result = results;
            }

            return result;
        }

        public async Task<TaxFeeDataModel> GetTaxFeeById(string TaxFeeId)
        {
            TaxFeeDataModel result = null;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                const string findQueryById = @"SELECT trim(taxfeeid) taxfeeid, taxfeename, valuedate, taxrate, feerate, createduser, 
createdate, updateuser, updatedate, status, recordno, approveduser, approveddate, isactive 
FROM Taxfee where TaxFeeId=@TaxfeeId";
                var parameters = new { TaxfeeId = TaxFeeId };
                var results = await db.QuerySingleOrDefaultAsync<TaxFeeDataModel>(findQueryById, parameters);
                // if (results!=null && results.Any())
                result = results;
            }

            return result;
        }

        public async Task<int> Approve(TaxFeeApproveModel TaxFeeApproveModel)
        {
            int resultCode = -1;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                db.Open();
                var trans = db.BeginTransaction();
                try
                {
                    const string StoreProcedure_approve = "approvetaxfee";
                    var param = new
                    {
                        v_taxfeeid = TaxFeeApproveModel.TaxFeeId,
                        v_confirmstatus = TaxFeeApproveModel.ConfirmStatus,
                        v_approveduser = TaxFeeApproveModel.ApprovedUser
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
    }
}
