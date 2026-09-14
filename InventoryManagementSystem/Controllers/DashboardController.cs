using InventoryManagementSystem.Models;
using InventoryManagementSystem.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly InventoryDbContext _context;

        public DashboardController(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // =========================
            // BASIC STATISTICS
            // =========================

            var totalProducts = await _context.Products.CountAsync();

            var totalCategories = await _context.Categories.CountAsync();

            var totalSuppliers = await _context.Suppliers.CountAsync();

            var totalStockQuantity = await _context.Products
                .SumAsync(p => p.StockQuantity);

            var lowStockCount = await _context.Products
                .CountAsync(p => p.StockQuantity <= p.LowStockThreshold);


            // =========================
            // LOW STOCK PRODUCTS
            // =========================

            var lowStockProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity <= p.LowStockThreshold)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();


            // =========================
            // PURCHASE STATISTICS
            // =========================

            var totalPurchasesCount = await _context.Purchases
                .CountAsync();

            var totalPurchasesAmount = await _context.Purchases
                .SumAsync(p => p.TotalAmount);


            // =========================
            // SALES STATISTICS
            // =========================

            var totalSalesCount = await _context.Sales
                .CountAsync();

            var totalSalesAmount = await _context.Sales
                .SumAsync(s => s.TotalAmount);


            // =========================
            // MOST SOLD PRODUCTS
            // =========================

            var mostSoldProducts = await _context.SaleItems
                .Include(x => x.Product)
                .GroupBy(x => new
                {
                    x.ProductId,
                    x.Product.ProductName
                })
                .Select(g => new MostSoldProductViewModel
                {
                    ProductName = g.Key.ProductName,

                    TotalQuantitySold = g.Sum(x => x.Quantity),

                    TotalRevenue = g.Sum(x =>
                        x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .Take(10)
                .ToListAsync();


            // =========================
            // RECENT PURCHASES
            // =========================

            var recentPurchases = await _context.Purchases
                .OrderByDescending(p => p.PurchaseDate)
                .Take(5)
                .Select(p => new RecentActivityViewModel
                {
                    Type = "شراء",

                    Description = "عملية شراء رقم " + p.PurchaseId,

                    Date = p.PurchaseDate,

                    Amount = p.TotalAmount
                })
                .ToListAsync();


            // =========================
            // RECENT SALES
            // =========================

            var recentSales = await _context.Sales
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .Select(s => new RecentActivityViewModel
                {
                    Type = "بيع",

                    Description = "عملية بيع رقم " + s.SaleId,

                    Date = s.SaleDate,

                    Amount = s.TotalAmount
                })
                .ToListAsync();


            // =========================
            // COMBINE RECENT ACTIVITY
            // =========================

            var recentActivity = recentPurchases
                .Concat(recentSales)
                .OrderByDescending(x => x.Date)
                .Take(10)
                .ToList();


            // =========================
            // CREATE DASHBOARD MODEL
            // =========================

            var model = new DashboardViewModel
            {
                TotalProducts = totalProducts,

                TotalCategories = totalCategories,

                TotalSuppliers = totalSuppliers,

                TotalStockQuantity = totalStockQuantity,

                LowStockCount = lowStockCount,

                LowStockProducts = lowStockProducts,

                TotalPurchasesCount = totalPurchasesCount,

                TotalPurchasesAmount = totalPurchasesAmount,

                TotalSalesCount = totalSalesCount,

                TotalSalesAmount = totalSalesAmount,

                MostSoldProducts = mostSoldProducts,

                RecentActivity = recentActivity
            };


            return View(model);
        }
    }
}