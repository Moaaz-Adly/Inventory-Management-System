using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models
{
    public class SaleItem
    {

        [Key]
        public int SaleItemId { get; set; }

        public int SaleId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        // Relationship: SaleItem * ---- 1 Sale
        public Sale Sale { get; set; }

        // Relationship: SaleItem * ---- 1 Product
        public Product Product { get; set; }
    }
}
