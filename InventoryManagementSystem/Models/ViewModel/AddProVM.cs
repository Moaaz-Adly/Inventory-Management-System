namespace InventoryManagementSystem.Models.ViewModel
{
    public class AddProVM
    {
        public string ProName { get; set; }
        public string ProSKU { get; set; }
        public int CategoryId { get; set; }
        public decimal ProPrice { get; set; }
        public int StockQuantity { get; set; }
        public int LowStockQuantity { get; set; }
        public List<Category> categories { get; set; }
    }
}
