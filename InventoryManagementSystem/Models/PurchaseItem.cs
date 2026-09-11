using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models
{
    public class PurchaseItem
    {

        [Key]
        public int PurchaseItemId { get; set; }

        public int PurchaseId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitCost { get; set; }

        public Purchase Purchase { get; set; }

        public Product Product { get; set; }
    }
}
