namespace PortfolioApi.Model
{
    public class PortfolioApproveModel
    {
        public string PortfolioId { get; set; }

        public int ConfirmStatus { get; set; }
    
        public string? ApprovedUser { get; set; }        

    }
}
