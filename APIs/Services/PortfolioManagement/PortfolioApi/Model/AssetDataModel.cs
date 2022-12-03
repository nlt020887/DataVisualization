namespace PortfolioApi.Model
{
    public class AssetDataModel
    {

        public Boolean IsActive { get; set; }

        public DateTime? CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public int RecordNo { get; set; }
        public Decimal? UnitPrice { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public String? UpdateUser { get; set; }

        public String? ApprovedUser { get; set; }

        public string Status { get; set; }

        public string AssetName { get; set; }

        public string AssetType { get; set; }

        public string CreatedUser { get; set; }

        public string AssetId { get; set; }

    }

}
