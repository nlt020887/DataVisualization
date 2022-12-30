using Dapper;
using JwtAuthenticationManager.Models;
using PortfolioApi.Model;
using System.Data;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace PortfolioApi.Infrastructure
{
    public class InvestorRepository : IInvestorRepository
    {
        private readonly IConfiguration _configuration;
        private string ConnectionString
        {
            get { return _configuration.GetConnectionString("DefaultConnection"); }
        }
        public InvestorRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<int> Approve(InvestorApproveModel input)
        {
            int resultCode = -1;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                db.Open();
                var trans = db.BeginTransaction();
                try
                {
                    const string StoreProcedure_approve = "approveinvestor";
                    var param = new
                    {
                        v_investorno = input.InvestorNo,
                        v_confirmstatus = input.ConfirmStatus,
                        v_approveduser = input.ApprovedUser
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

        public async Task<CreatedInvestorResponseModel> CreatedInvestor(InvestorDataModel input)
        {
            CreatedInvestorResponseModel model = new CreatedInvestorResponseModel();
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                db.Open();
                var trans = db.BeginTransaction();
                try
                {

                    const string StoreProcedure_create= "\"insertinvestor\"";
                    var param = new
                    {
                        v_dealdate = input.DealDate,
                        v_investoruser = input.InvestorUser,
                        v_investorname = input.InvestorName,
                        v_portfolioid =input.PortfolioId,
                        v_portfolioname = input.PortfolioName,
                        v_dealtype = input.DealType,
                        v_valuedate =input.ValueDate,
                        v_paymentdate = input.PaymentDate,
                        v_noofcert = input.NoOfCert,
                        v_certprice = input.CertPrice,
                        v_transactionvalue = input.TransactionValue,
                        v_fee = input.Fee,
                        v_note = input.Note,
                        v_createduser = input.CreatedUser
                    };

                    var dynamicParameters = new DynamicParameters();
                    dynamicParameters.AddDynamicParams(param);
                    dynamicParameters.Add("@resultcode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    dynamicParameters.Add("@v_investorno", input.InvestorNo,dbType: DbType.String, direction: ParameterDirection.InputOutput);
                    var results = await db.ExecuteAsync(StoreProcedure_create, dynamicParameters, trans, null, CommandType.StoredProcedure);
                    model.ResultCode = dynamicParameters.Get<int>("@resultcode");
                    model.InvestorNo = dynamicParameters.Get<String>("@v_investorno");

                    if (model.ResultCode == 0)
                    {
                        trans.Commit();
                    }
                    else
                        trans.Rollback();
                }
                catch (Exception exx)
                {
                    trans.Rollback();
                    model.ResultCode = -1;
                    throw exx;
                }
                finally
                {
                    db.Close();
                }


            }
            return model;
        }

        public async Task<int> Delete(InvestorDeleteModel input)
        {
            int resultCode = -1;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                db.Open();
                var trans = db.BeginTransaction();
                try
                {
                    const string StoreProcedure_del = "deleteinvestor";
                    var param = new
                    {
                        v_investorno = input.InvestorNo,
                        v_userdelete = input.DeletedUser
                    };

                    var dynamicParameters = new DynamicParameters();
                    dynamicParameters.AddDynamicParams(param);
                    dynamicParameters.Add("@resultcode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    var results = await db.ExecuteAsync(StoreProcedure_del, dynamicParameters, trans, null, CommandType.StoredProcedure);
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

        public async Task<string> GetNewInvestorNo(string DealTypeName)
        {
            string result = String.Empty;
            int dealtype = 0;
            if (string.IsNullOrEmpty(DealTypeName))
                return string.Empty;
            switch (DealTypeName.ToUpper())
            {
                case "BUY":
                    dealtype = 1;
                    break;
                case "SELL":
                    dealtype = -1;
                    break;
                default:
                    return string.Empty;
            }
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                db.Open();
                try
                {
                    const string StoreProcedure_del = "createinvestornewno";
                    var param = new
                    {
                        v_dealtype = dealtype,
                        v_dealdate = DateTime.Now,
                        v_isview = true
                    };

                    var dynamicParameters = new DynamicParameters();
                    dynamicParameters.AddDynamicParams(param);
                    dynamicParameters.Add("@transno", dbType: DbType.String, direction: ParameterDirection.Output);
                    var results = await db.ExecuteAsync(StoreProcedure_del, dynamicParameters,null, null, CommandType.StoredProcedure);
                    result = dynamicParameters.Get<String>("@transno");
                }
                catch (Exception exx)
                {
                    result = String.Empty;
                    throw exx;
                }
                finally
                {
                    db.Close();
                }
            }
            return result;
        }

        public async Task<InvestorDataModel> GetInvestorByTransNo(string InvestorNo)
        {
            InvestorDataModel result = null;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                const string findQueryById = @"SELECT investorno, dealdate, investoruser, investorname, 
                portfolioid, portfolioname, dealtype, valuedate, paymentdate, noofcert, certprice, transactionvalue, fee, 
                note, createddate, createduser, updateddate, updateduser, approveddate, 
                approveduser, approvedstatus, status, isdeleted, orderindex
	            FROM public.investor
                where investorNo =@transactionno;";
                var parameters = new { transactionno = InvestorNo };
                var results = await db.QuerySingleOrDefaultAsync<InvestorDataModel>(findQueryById, parameters);
                // if (results!=null && results.Any())
                result = results;
            }

            return result;
        }

        public async Task<PagingResponseModel<List<InvestorDataModel>>> SearchListInvestor(SearchInvestorModel model)
        {
            int maxPagSize = 50;
            model.PageSize = (model.PageSize > 0 && model.PageSize <= maxPagSize) ? model.PageSize : maxPagSize;

            int skip = (model.PageNumber - 1) * model.PageSize;
            int take = model.PageSize;

            const string findAllQuery = @"SELECT count(*) 
                        FROM public.investor
                        where (@Keyword=null or @Keyword='' or investorno = @Keyword)                    
                          and (@IsFromTransactionDate = ''  or dealdate >= cast(@FromTransactionDate as date))
                        and (@IsToTransactionDate = ''  or dealdate <= cast(@ToTransactionDate  as date)+1)
                        and (@IsFromPaymentDate = ''  or paymentdate >= cast(@FromPaymentDate as date))
                        and (@IsToPaymentDate = ''  or paymentdate <= cast(@ToPaymentDate  as date)+1)
                         and (@PortfolioId = null or @PortfolioId = ''  or  portfolioid= @PortfolioId)
                        and (@DealType=0 or dealtype=@DealType);


                       SELECT investorno, dealdate, investoruser, investorname, portfolioid, portfolioname,
                        dealtype, valuedate, paymentdate, noofcert, certprice, transactionvalue, fee, note, 
                        createddate, createduser, updateddate, updateduser, approveddate, approveduser, approvedstatus, status, isdeleted, orderindex
	                    FROM public.investor
                        where (@Keyword=null or @Keyword='' or investorno = @Keyword)      
                        and (@IsFromTransactionDate = ''  or dealdate >= cast(@FromTransactionDate as date))
                        and (@IsToTransactionDate = ''  or dealdate <= cast(@ToTransactionDate  as date)+1)
                        and (@IsFromPaymentDate = ''  or paymentdate >= cast(@FromPaymentDate as date))
                        and (@IsToPaymentDate = ''  or paymentdate <= cast(@ToPaymentDate  as date)+1)
                         and (@PortfolioId = null or @PortfolioId = ''  or  portfolioid= @PortfolioId)
                        and (@DealType=0 or dealtype=@DealType)
                        offset @Skip limit @Take;"
            ;
            try
            {
                using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
                {
                    var reader = await db.QueryMultipleAsync(findAllQuery, new { Skip = skip, 
                        Take = take,
                        Keyword = model.Keyword,
                        FromTransactionDate = model.FromTransactionDate,
                        IsFromTransactionDate = (model.ToTransactionDate.HasValue==true?"true":""),
                        ToTransactionDate = model.ToTransactionDate,
                        IsToTransactionDate = (model.ToTransactionDate.HasValue == true ? "true" : ""),
                        FromPaymentDate = model.FromPaymentDate,
                        IsFromPaymentDate = (model.FromPaymentDate.HasValue == true ? "true" : ""),
                        ToPaymentDate = model.ToPaymentDate,
                        IsToPaymentDate = (model.ToPaymentDate.HasValue == true ? "true" : ""),
                        PortfolioId = model.PortfolioId ,
                        DealType = model.DealType   
                    });

                    int count = reader.Read<int>().FirstOrDefault();

                    List<InvestorDataModel> AssetDataModelList = reader.Read<InvestorDataModel>().ToList();

                    var result = new PagingResponseModel<List<InvestorDataModel>>(AssetDataModelList, count, model.PageNumber, model.PageSize);
                    return result;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

      
    }
}
