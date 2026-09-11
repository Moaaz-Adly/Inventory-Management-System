namespace InventoryManagementSystem.Models.ViewModel
{
    public class DetailsProVM
    {
        public int ProductID { get; set; }
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public int LowStockThreshold { get; set; }

        public int TotalPurchasedQuantity { get; set; }
        public int TotalSoldQuantity { get; set; }

        public string SupplierName { get; set; }
    }
}
