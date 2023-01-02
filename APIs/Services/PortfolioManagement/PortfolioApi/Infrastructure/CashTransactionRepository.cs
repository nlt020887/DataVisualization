using Dapper;
using JwtAuthenticationManager.Models;
using PortfolioApi.Model;
using System.Data;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace PortfolioApi.Infrastructure
{
    public class CashTransactionRepository : ICashTransactionRepository
    {
        private readonly IConfiguration _configuration;
        private string ConnectionString
        {
            get { return _configuration.GetConnectionString("DefaultConnection"); }
        }
        public CashTransactionRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<int> Approve(CashTransactionApproveModel input)
        {
            int resultCode = -1;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                db.Open();
                var trans = db.BeginTransaction();
                try
                {
                    const string StoreProcedure_approve = "approvecashtransaction";
                    var param = new
                    {
                        v_transactionno = input.TransactionNo,
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

        public async Task<CreatedCashTransacionResponseModel> CreatedCashTransaction(CashTransactionDataModel input)
        {
            CreatedCashTransacionResponseModel model = new CreatedCashTransacionResponseModel();
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                db.Open();
                var trans = db.BeginTransaction();
                try
                {

                    const string StoreProcedure_create= "\"insertcashtransaction\"";
                    var param = new
                    {
                        v_dealdate = input.DealDate,
                        v_portfolioid =input.PortfolioId,
                        v_portfolioname = input.PortfolioName,
                        v_dealtype = input.DealType,
                        v_typeid = input.TypeId,
                        v_valuedate =input.ValueDate,
                        v_paymentdate = input.PaymentDate,
                        v_currency = input.Currency,
                        v_amount = input.Amount,
                        v_exchangerate = input.ExchangeRate,
                        v_transactionvalue = input.TransactionValue,                        
                        v_note = input.Note,
                        v_createduser = input.CreatedUser
                    };

                    var dynamicParameters = new DynamicParameters();
                    dynamicParameters.AddDynamicParams(param);
                    dynamicParameters.Add("@resultcode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    dynamicParameters.Add("@v_transactionno",input.TransactionNo,dbType: DbType.String, direction: ParameterDirection.InputOutput);
                    var results = await db.ExecuteAsync(StoreProcedure_create, dynamicParameters, trans, null, CommandType.StoredProcedure);
                    model.ResultCode = dynamicParameters.Get<int>("@resultcode");
                    model.TransactionNo = dynamicParameters.Get<String>("@v_transactionno");

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

        public async Task<int> Delete(CashTransactionDeleteModel input)
        {
            int resultCode = -1;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                db.Open();
                var trans = db.BeginTransaction();
                try
                {
                    const string StoreProcedure_del = "deletecashtransaction";
                    var param = new
                    {
                        v_transactionno = input.TransactionNo,
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

        public async Task<string> GetNewCashTransactionNo(string DealTypeName)
        {
            string result = String.Empty;
            int dealtype = 0;
            if (string.IsNullOrEmpty(DealTypeName))
                return string.Empty;
            switch (DealTypeName.ToUpper())
            {
                case "CREDIT":
                    dealtype = 1;
                    break;
                case "DEBIT":
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
                    const string StoreProcedure_del = "createcashtransactionnewno";
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

        public async Task<CashTransactionDataModel> GetCashTransactionByTransNo(string TransactionNo)
        {
            CashTransactionDataModel result = null;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                const string findQueryById = @"SELECT transactionno, 
                portfolioid, portfolioname,
                dealtype,
                 case when dealtype=1 then 'CREDIT' else 'DEBIT' end as DealTypeName,
                typeid, 
                (select TypeName from cashtransactiontype where typeid = cashtransaction.typeid) as TypeName,
                dealdate,valuedate, paymentdate, amount,
                currency, exchangerate, transactionvalue,
                note, createddate, createduser,
                updateddate, updateduser,
                approveddate, approveduser, 
                approvedstatus, status, 
                isdeleted, orderindex
                FROM public.cashtransaction
                where transactionNo =@transactionno;";
                var parameters = new { transactionno = TransactionNo };
                var results = await db.QuerySingleOrDefaultAsync<CashTransactionDataModel>(findQueryById, parameters);
                // if (results!=null && results.Any())
                result = results;
            }

            return result;
        }

        public async Task<PagingResponseModel<List<CashTransactionDataModel>>> SearchListCashTransaction(SearchCashTransactionModel model)
        {
            int maxPagSize = 50;
            model.PageSize = (model.PageSize > 0 && model.PageSize <= maxPagSize) ? model.PageSize : maxPagSize;

            int skip = (model.PageNumber - 1) * model.PageSize;
            int take = model.PageSize;

            const string findAllQuery = @"SELECT count(*) 
                        FROM public.cashtransaction
                        where (@Keyword=null or @Keyword='' or transactionno = @Keyword)                    
                          and (@IsFromTransactionDate = ''  or dealdate >= cast(@FromTransactionDate as date))
                        and (@IsToTransactionDate = ''  or dealdate <= cast(@ToTransactionDate  as date)+1)
                        and (@IsFromPaymentDate = ''  or paymentdate >= cast(@FromPaymentDate as date))
                        and (@IsToPaymentDate = ''  or paymentdate <= cast(@ToPaymentDate  as date)+1)
                         and (@PortfolioId = null or @PortfolioId = ''  or  portfolioid= @PortfolioId)                       
                        and (@DealType=0 or dealtype=@DealType);

                      SELECT transactionno, 
                        portfolioid, portfolioname,
                        dealtype,
                         case when dealtype=1 then 'CREDIT' else 'DEBIT' end as DealTypeName,
                        typeid, 
                        (select TypeName from cashtransactiontype where typeid = cashtransaction.typeid) as TypeName,
                        dealdate,valuedate, paymentdate, amount,
                        currency, exchangerate, transactionvalue,
                        note, createddate, createduser,
                        updateddate, updateduser,
                        approveddate, approveduser, 
                        approvedstatus, status, 
                        isdeleted, orderindex
                        FROM public.cashtransaction
                        where (@Keyword=null or @Keyword='' or transactionno = @Keyword)      
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
                        IsFromTransactionDate = (model.FromTransactionDate.HasValue==true?"true":""),
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

                    List<CashTransactionDataModel> AssetDataModelList = reader.Read<CashTransactionDataModel>().ToList();

                    var result = new PagingResponseModel<List<CashTransactionDataModel>>(AssetDataModelList, count, model.PageNumber, model.PageSize);
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
