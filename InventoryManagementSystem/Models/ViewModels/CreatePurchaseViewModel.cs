using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models.ViewModels
{
    public class CreatePurchaseViewModel
    {
        [Required(ErrorMessage = "يرجى اختيار المورد")]
        [Display(Name = "المورد")]
        public int SupplierId { get; set; }

        public List<PurchaseItemInputModel> Items { get; set; } = new();
    }

    public class PurchaseItemInputModel
    {
        [Required(ErrorMessage = "يرجى اختيار المنتج")]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون 1 على الأقل")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "السعر يجب أن يكون أكبر من 0")]
        public decimal UnitCost { get; set; }
    }
}