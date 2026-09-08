using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SKU { get; set; }

        [Required]
        [MaxLength(150)]
        public string ProductName { get; set; }

        public int CategoryId { get; set; }

        public decimal UnitPrice { get; set; }

        public int StockQuantity { get; set; }

        public int LowStockThreshold { get; set; }

        public Category Category { get; set; }

        public ICollection<PurchaseItem> PurchaseItems { get; set; }
            = new List<PurchaseItem>();

        public ICollection<SaleItem> SaleItems { get; set; }
            = new List<SaleItem>();
    }
}
