namespace PortfolioApi.Model
{
    public class CashTransactionDataModel
    {
        public Int32? OrderIndex { get; set; }

        public DateTime DealDate { get; set; }

        public DateTime ValueDate { get; set; }

        public DateTime PaymentDate { get; set; }

        public Decimal Amount { get; set; }

        public string Currency { get; set; }

        public Decimal TransactionValue { get; set; }

        public decimal? ExchangeRate{ get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? updateddate { get; set; }

        public DateTime? approveddate { get; set; }

        public Boolean IsDeleted { get; set; }

        /// <summary>
        /// BUY/SELL
        /// </summary>
        public string DealTypeName { get; set; }//


        public int TypeId { get; set; }

        public string? TypeName
        { get; set; }

        public int DealType
        {
            get { if (DealTypeName.ToUpper().Trim() == "CREDIT")
                    return 1;
                else if (DealTypeName.ToUpper().Trim() == "DEBIT")
                    return -1;
                else
                    return 0;
            }
        }

        public string PortfolioId { get; set; }

        public string? PortfolioName { get; set; }

        public string? ApprovedUser { get; set; }

        public string? Note { get; set; }

        public String? ApprovedStatus { get; set; }

        public string? CreatedUser { get; set; }

        public string? Status { get; set; }

        public string? UpdatedUser { get; set; }

        public string? TransactionNo { get; set; }

    }

    public class CashTransactionApproveModel
    {
        public string TransactionNo { get; set; }

        public int ConfirmStatus { get; set; }

        public string? ApprovedUser { get; set; }
    }

    public class CashTransactionDeleteModel
    {
        public string TransactionNo { get; set; }

        public string? DeletedUser { get; set; }
    }
    public class CreatedCashTransacionResponseModel
    {
        public int ResultCode { get; set; } = -1;
        public string TransactionNo { get; set; }
    }

    public class SearchCashTransactionModel
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public string Keyword { get; set; }

        public DateTime? FromTransactionDate { get; set; }
        public DateTime? ToTransactionDate { get; set; }

        public DateTime? FromPaymentDate { get; set; }
        public DateTime? ToPaymentDate { get; set; }

        public String? PortfolioId { get; set; }
        public String? Status { get; set; }
        public String? DealTypeName { get; set; }

        public int? DealType
        {
            get
            {
                if (DealTypeName.ToUpper().Trim() == "CREDIT")
                    return 1;
                else if (DealTypeName.ToUpper().Trim() == "DEBIT")
                    return -1;
                else
                    return 0;
            }
        }

    }
}
