namespace PortfolioApi.Model
{
    public class PortfolioDataModel

    {

        public int RecordNo { get; set; }

        public DateTime? CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public String? CreatedUser { get; set; }

        public String? Status { get; set; }

        public string? UpdateUser { get; set; }

        public string PortfolioId { get; set; } 

        public string PortfolioName { get; set; }

        public string TaxFeeId { get; set; }

        public List<PortfolioUserDataModel> PortfolioUsers { get; set; }

        public string? ApprovedUser { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public Boolean IsActive { get; set; }

    }

}
