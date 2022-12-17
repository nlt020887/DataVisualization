using Dapper;
using JwtAuthenticationManager.Models;
using PortfolioApi.Model;
using System.Data;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace PortfolioApi.Infrastructure
{
    public class Transaction : ITransaction
    {
        private readonly IConfiguration _configuration;
        private string ConnectionString
        {
            get { return _configuration.GetConnectionString("DefaultConnection"); }
        }
        public Transaction(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<int> Approve(TransactionApproveModel input)
        {
            int resultCode = -1;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                db.Open();
                var trans = db.BeginTransaction();
                try
                {
                    const string StoreProcedure_approve = "approvetransaction";
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

        public async Task<CreatedTransacionResponseModel> CreatedTransaction(TransactionDataModel input)
        {
            CreatedTransacionResponseModel model = new CreatedTransacionResponseModel();
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                db.Open();
                var trans = db.BeginTransaction();
                try
                {

                    const string StoreProcedure_create= "\"inserttransaction\"";
                    var param = new
                    {                       
                        v_transactiondate = input.TransactionDate,
                        v_portfolioid =input.PortfolioId,
                        v_assetid =input.AssetId,
                        v_dealtype = input.DealType,
                        v_valuedate =input.ValueDate,
                        v_paymentdate = input.PaymentDate,
                        v_transactionamount = input.TransactionAmount,
                        v_transactionprice  = input.TransactionPrice,
                        v_transactionvalue = input.TransactionValue,
                        v_taxfeeid = input.TaxFeeId,
                        v_taxamount = input.TaxAmount,
                        v_feeamount = input.FeeAmount,
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

        public async Task<int> Delete(TransactionDeleteModel input)
        {
            int resultCode = -1;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                db.Open();
                var trans = db.BeginTransaction();
                try
                {
                    const string StoreProcedure_del = "deletetransaction";
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

        public async Task<string> GetNewTransactionNo(string DealTypeName)
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
                    const string StoreProcedure_del = "createtransactionnewno";
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

        public async Task<TransactionDataModel> GetTransactionByTransNo(string TransactionNo)
        {
            TransactionDataModel result = null;
            using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                const string findQueryById = @"SELECT transactionno,
                transactiondate, 
                portfolioid,
                assetid,
                assetname,
                dealtype,
                valuedate, 
                paymentdate,
                transactionamount,
                transactionprice, 
                transactionvalue,
                taxfeeid,
                taxamount,
                feeamount,
                note,
                createddate,
                createduser,
                updateddate,
                updateduser,
                approveddate,
                approveduser,
                approvedstatus,
                status,
                isdeleted,
                orderindex
                FROM public.transaction
                where transactionNo =@transactionno;";
                var parameters = new { transactionno = TransactionNo };
                var results = await db.QuerySingleOrDefaultAsync<TransactionDataModel>(findQueryById, parameters);
                // if (results!=null && results.Any())
                result = results;
            }

            return result;
        }

        public async Task<PagingResponseModel<List<TransactionDataModel>>> SearchListTransaction(SearchTransactionModel model)
        {
            int maxPagSize = 50;
            model.PageSize = (model.PageSize > 0 && model.PageSize <= maxPagSize) ? model.PageSize : maxPagSize;

            int skip = (model.PageNumber - 1) * model.PageSize;
            int take = model.PageSize;

            const string findAllQuery = @"SELECT count(*)
                        FROM public.transaction
                        where (1=1) 
                        and (@FromTransactionDate is null or transactiondate >= cast(@FromTransactionDate as date))
                        and (@ToTransactionDate is null or transactiondate <= cast(@ToTransactionDate  as date)+1)
                        and (@FromPaymentDate is null or paymentdate >= cast(@FromPaymentDate as date))
                        and (@ToPaymentDate is null or paymentdate <= cast(@ToPaymentDate  as date)+1)
                        and (@PortfolioId is null or  portfolioid= @PortfolioId)
                        and (@AssetId is null or  assetid= @AssetId)
                        and (@DealType=0 or dealtype=@DealType);

                       SELECT transactionno,
                        transactiondate, 
                        portfolioid,
                        assetid,
                        assetname,
                        dealtype,
                        valuedate, 
                        paymentdate,
                        transactionamount,
                        transactionprice, 
                        transactionvalue,
                        taxfeeid,
                        taxamount,
                        feeamount,
                        note,
                        createddate,
                        createduser,
                        updateddate,
                        updateduser,
                        approveddate,
                        approveduser,
                        approvedstatus,
                        status,
                        isdeleted,
                        orderindex
                        FROM public.transaction
                        where (1=1) 
                        and (@FromTransactionDate is null or transactiondate >= cast(@FromTransactionDate as date))
                        and (@ToTransactionDate is null or transactiondate <= cast(@ToTransactionDate  as date)+1)
                        and (@FromPaymentDate is null or paymentdate >= cast(@FromPaymentDate as date))
                        and (@ToPaymentDate is null or paymentdate <= cast(@ToPaymentDate  as date)+1)
                        and (@PortfolioId is null or  portfolioid= @PortfolioId)
                        and (@AssetId is null or  assetid= @AssetId)
                        and (@DealType=0 or dealtype=@DealType)
                        offset @Skip limit @Take;"
            ;
            try
            {
                using (IDbConnection db = new Npgsql.NpgsqlConnection(ConnectionString))
                {
                    var reader = await db.QueryMultipleAsync(findAllQuery, new { Skip = skip, 
                        Take = take,
                        FromTransactionDate = model.FromTransactionDate,
                        ToTransactionDate = model.ToTransactionDate,
                        FromPaymentDate = model.FromPaymentDate,
                        ToPaymentDate = model.ToPaymentDate,
                        PortfolioId = model.PortfolioId ,
                        AssetId = model.AssetId ,
                        DealType = model.DealType   
                    });

                    int count = reader.Read<int>().FirstOrDefault();

                    List<TransactionDataModel> AssetDataModelList = reader.Read<TransactionDataModel>().ToList();

                    var result = new PagingResponseModel<List<TransactionDataModel>>(AssetDataModelList, count, model.PageNumber, model.PageSize);
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
