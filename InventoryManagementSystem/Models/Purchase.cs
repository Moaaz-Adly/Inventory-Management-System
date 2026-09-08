using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models
{
    public class Purchase
    {

        [Key]
        public int PurchaseId { get; set; }

        public int SupplierId { get; set; }

        public DateTime PurchaseDate { get; set; }

        public decimal TotalAmount { get; set; }

        public Supplier Supplier { get; set; }

        public ICollection<PurchaseItem> PurchaseItems { get; set; }
            = new List<PurchaseItem>();
    }
}
