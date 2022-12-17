namespace PortfolioApi.Model
{
    public class TransactionDataModel
    {
        public Int32? OrderIndex { get; set; }

        public DateTime TransactionDate { get; set; }

        public DateTime ValueDate { get; set; }

        public DateTime PaymentDate { get; set; }

        public Decimal TransactionAmount { get; set; }

        public Decimal TransactionPrice { get; set; }

        public Decimal TransactionValue { get; set; }

        public decimal? TaxAmount { get; set; }

        public decimal? FeeAmount { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? updateddate { get; set; }

        public DateTime? approveddate { get; set; }

        public Boolean IsDeleted { get; set; }

        /// <summary>
        /// BUY/SELL
        /// </summary>
        public string DealTypeName { get; set; }//

        public int DealType
        {
            get { if (DealTypeName.ToUpper().Trim() == "BUY")
                    return 1;
                else if (DealTypeName.ToUpper().Trim() == "SELL")
                    return -1;
                else
                    return 0;
            }
        }

        public string PortfolioId { get; set; }

        public string AssetId { get; set; }

        public string? AssetName { get; set; }

        public string? ApprovedUser { get; set; }

        public string? Note { get; set; }

        public String? ApprovedStatus { get; set; }

        public string? CreatedUser { get; set; }

        public string? Status { get; set; }

        public string? UpdatedUser { get; set; }

        public string TaxFeeId { get; set; }

        public string? TransactionNo { get; set; }

    }

    public class TransactionApproveModel
    {
        public string TransactionNo { get; set; }

        public int ConfirmStatus { get; set; }

        public string? ApprovedUser { get; set; }
    }

    public class TransactionDeleteModel
    {
        public string TransactionNo { get; set; }

        public string? DeletedUser { get; set; }
    }
    public class CreatedTransacionResponseModel
    {
        public int ResultCode { get; set; } = -1;
        public string TransactionNo { get; set; }
    }
}
