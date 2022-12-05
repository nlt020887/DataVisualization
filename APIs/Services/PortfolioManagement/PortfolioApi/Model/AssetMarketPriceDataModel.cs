namespace PortfolioApi.Model
{
    public class AssetMarketPriceDataModel
    {
        public decimal? EodAssetMktPrice { get; set; }

        public DateTime? ValueDate { get; set; }

        public decimal AssetMarketPrice { get; set; }

        public DateTime? CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public int RecordNo { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public Boolean IsActive { get; set; }

        public String? CreatedUser { get; set; }

        public string AssetId { get; set; }

        public String? UpdateUser { get; set; }

        public String? ApprovedUser { get; set; }

        public string Status { get; set; }

    }
}
