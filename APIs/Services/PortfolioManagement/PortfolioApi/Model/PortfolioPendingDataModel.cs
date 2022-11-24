namespace PortfolioApi.Model
{
    public class PortfolioPendingDataModel
    {
        public int RecordNo { get; set; }

        public DateTime? CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public string CreatedUser { get; set; }

        public string Status { get; set; }

        public string UpdateUser { get; set; }

        public string PortfolioId { get; set; }

        public string PortfolioName { get; set; }

        public string TaxFeeId { get; set; }
        public List<PortfolioUsersPendingDataModel> PortfolioUserPending { get; set; }
    }
}
