namespace PortfolioApi.Model
{
    public class PortfolioUsersPendingDataModel
    {
        public int RoleType { get; set; }

        public int RecordNo { get; set; }

        public string PortfolioId { get; set; }

        public string UserId { get; set; }
        public String? FullName { get; set; }
        public String? RoleTypeName { get; set; }

    }
}
