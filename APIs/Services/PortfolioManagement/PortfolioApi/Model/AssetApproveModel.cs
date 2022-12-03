namespace PortfolioApi.Model
{
    public class AssetApproveModel
    {
        public string AssetId { get; set; }

        public int ConfirmStatus { get; set; }

        public string? ApprovedUser { get; set; }
    }
}
