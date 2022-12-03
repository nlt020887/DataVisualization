namespace PortfolioApi.Model
{
    public class TaxFeeApproveModel
    {
        public string TaxFeeId { get; set; }

        public int ConfirmStatus { get; set; }

        public string? ApprovedUser { get; set; }
    }
}
