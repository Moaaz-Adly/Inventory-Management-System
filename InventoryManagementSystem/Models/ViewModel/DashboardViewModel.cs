namespace InventoryManagementSystem.Models.ViewModel
{
    public class DashboardViewModel
    {

        // =========================
        // KPI Statistics
        // =========================

        public int TotalProducts { get; set; }

        public int TotalCategories { get; set; }

        public int TotalSuppliers { get; set; }

        public int TotalStockQuantity { get; set; }

        public int LowStockCount { get; set; }


        // =========================
        // Purchases
        // =========================

        public int TotalPurchasesCount { get; set; }

        public decimal TotalPurchasesAmount { get; set; }


        // =========================
        // Sales
        // =========================

        public int TotalSalesCount { get; set; }

        public decimal TotalSalesAmount { get; set; }


        // =========================
        // Low Stock Products
        // =========================

        public List<Product> LowStockProducts { get; set; }
            = new List<Product>();


        // =========================
        // Most Sold Products
        // =========================

        public List<MostSoldProductViewModel> MostSoldProducts { get; set; }
            = new List<MostSoldProductViewModel>();


        // =========================
        // Recent Activity
        // =========================

        public List<RecentActivityViewModel> RecentActivity { get; set; }
            = new List<RecentActivityViewModel>();
    }


    // =========================================
    // Most Sold Product
    // =========================================

    public class MostSoldProductViewModel
    {
        public string ProductName { get; set; } = string.Empty;

        public int TotalQuantitySold { get; set; }

        public decimal TotalRevenue { get; set; }
    }


    // =========================================
    // Recent Activity
    // =========================================

    public class RecentActivityViewModel
    {
        public string Type { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public decimal Amount { get; set; }
    }
}
