using static System.Net.Mime.MediaTypeNames;
using System;

namespace PortfolioApi.Model
{
    public class TaxFeePendingDataModel
    {
        public string  TaxFeeId { get; set; }
        public String TaxFeeName { get; set; }
        public DateTime ValueDate { get; set; }
        public Decimal TaxRate { get; set; }
        public Decimal FeeRate { get; set; }

        public string CreatedUser { get; set; }
        public DateTime CreateDate { get; set; }    
        public String? UpdateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string Status { get; set; }
        public int RecordNo { get; set; }

        public string ApprovedUser { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public Boolean IsActive { get; set; }
    }
}
