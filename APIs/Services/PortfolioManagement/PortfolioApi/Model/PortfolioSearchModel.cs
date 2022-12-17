namespace PortfolioApi.Model
{
    public class SearchModel
    {
        public int PageSize{ get; set; } 
        public int PageNumber{ get; set; }
        public string Keyword { get; set; }
    }

    public class SearchTransactionModel
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public string Keyword { get; set; }

        public DateTime? FromTransactionDate { get; set; }
        public DateTime? ToTransactionDate { get; set; }

        public DateTime? FromPaymentDate { get; set; }
        public DateTime? ToPaymentDate { get; set; }

        public String? PortfolioId { get; set; }
        public String? AssetId { get; set; }
        public String? Status { get; set; }
        public String? DealTypeName { get; set; }

        public int? DealType
        {
            get
            {
                if (DealTypeName.ToUpper().Trim() == "BUY")
                    return 1;
                else if (DealTypeName.ToUpper().Trim() == "SELL")
                    return -1;
                else
                    return 0;
            }
        }

    }
}
