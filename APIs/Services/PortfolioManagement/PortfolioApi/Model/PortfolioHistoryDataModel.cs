namespace PortfolioApi.Model
{
    public class PortfolioHistoryDataModel
    {
        public DateTime? HistoryTime { get; set; }

        public DateTime? CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public Int32 RecordNo { get; set; }

        public Guid Id { get; set; }

        public string Status { get; set; }

        public string UpdateUser { get; set; }

        public string PortfolioId { get; set; }

        public string PortfolioName { get; set; }

        public string TaxFeeId { get; set; }

        public string CreatedUser { get; set; }
       
    }
}
