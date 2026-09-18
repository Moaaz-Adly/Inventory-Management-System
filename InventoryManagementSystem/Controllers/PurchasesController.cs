using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Models.ViewModels;
using System.Text.Json;

namespace InventoryManagementSystem.Controllers
{
    public class PurchasesController : Controller
    {
        private readonly InventoryDbContext _context;

        public PurchasesController(InventoryDbContext context)
        {
            _context = context;
        }

        // 1. Purchase History
        public async Task<IActionResult> Index()
        {
            var purchases = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();

            return View(purchases);
        }

        // 2. Purchase Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var purchase = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .FirstOrDefaultAsync(p => p.PurchaseId == id);

            if (purchase == null)
                return NotFound();

            return View(purchase);
        }

        // 3. Create Purchase - GET
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCreateData();

            return View(new CreatePurchaseViewModel());
        }

        // 4. Create Purchase - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePurchaseViewModel model)
        {
            if (model.Items == null || !model.Items.Any())
            {
                ModelState.AddModelError(
                    "",
                    "At least one item must be added to the purchase."
                );
            }

            if (ModelState.IsValid)
            {
                using var transaction =
                    await _context.Database.BeginTransactionAsync();

                try
                {
                    var purchase = new Purchase
                    {
                        SupplierId = model.SupplierId,
                        PurchaseDate = DateTime.Now,
                        TotalAmount = 0
                    };

                    _context.Purchases.Add(purchase);

                    await _context.SaveChangesAsync();

                    decimal totalAmount = 0;

                    foreach (var item in model.Items)
                    {
                        decimal lineTotal =
                            item.Quantity * item.UnitCost;

                        totalAmount += lineTotal;

                        var purchaseItem = new PurchaseItem
                        {
                            PurchaseId = purchase.PurchaseId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitCost = item.UnitCost
                        };

                        _context.PurchaseItems.Add(purchaseItem);

                        // Increase stock
                        var product =
                            await _context.Products.FindAsync(item.ProductId);

                        if (product != null)
                        {
                            product.StockQuantity += item.Quantity;
                        }
                    }

                    purchase.TotalAmount = totalAmount;

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        "",
                        "An unexpected error occurred while saving the purchase."
                    );
                }
            }

            // لو حصل Validation Error
            // لازم نرجع كل البيانات للـ View
            await LoadCreateData(model.SupplierId);

            return View(model);
        }

        // ==========================================
        // Helper Method
        // ==========================================

        private async Task LoadCreateData(int? selectedSupplierId = null)
        {
            // Suppliers
            var suppliers = await _context.Suppliers
                .ToListAsync();

            ViewBag.Suppliers = new SelectList(
                suppliers,
                "SupplierId",
                "SupplierName",
                selectedSupplierId
            );

            // Products
            var products = await _context.Products
                .Select(p => new
                {
                    Id = p.ProductId,
                    Name = p.ProductName,
                    CostPrice = p.UnitPrice
                })
                .ToListAsync();

            // نحول المنتجات إلى JSON هنا
            // بدل ما نعتمد على dynamic في الـ View
            ViewBag.ProductsJson = JsonSerializer.Serialize(products);
        }
    }
}