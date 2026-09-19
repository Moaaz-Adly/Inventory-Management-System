using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace InventoryManagementSystem.Models.ViewModel
{
    public class EditProVM
    {
        [Required]
        public int ProId { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
        [Display(Name = "Product Name")]
        public string ProName { get; set; }

        [Required(ErrorMessage = "SKU is required.")]
        [Display(Name = "SKU")]
        public string ProSKU { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0.")]
        [Display(Name = "Price")]
        public decimal ProPrice { get; set; }

        [Required(ErrorMessage = "Stock quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Low stock threshold is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Threshold cannot be negative.")]
        [Display(Name = "Low Stock Threshold")]
        public int LowStockQuantity { get; set; }

        [ValidateNever]
        public List<Category>? categories { get; set; }
    }
}
