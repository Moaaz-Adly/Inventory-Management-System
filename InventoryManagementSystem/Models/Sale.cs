using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models
{
    public class Sale
    {

        [Key]
        public int SaleId { get; set; }

        public DateTime SaleDate { get; set; }

        public decimal TotalAmount { get; set; }

        [MaxLength(300)]
        public string? CustomerInfo { get; set; }

        // Relationship: Sale 1 ---- * SaleItem
        public ICollection<SaleItem> SaleItems { get; set; }
            = new List<SaleItem>();
    }
}
