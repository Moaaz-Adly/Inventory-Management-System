using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Models.ViewModels;

namespace InventoryManagementSystem.Controllers
{
    public class PurchasesController : Controller
    {
        private readonly InventoryDbContext _context;

        public PurchasesController(InventoryDbContext context)
        {
            _context = context;
        }

        // 1. Purchase History (سجل المشتريات)
        public async Task<IActionResult> Index()
        {
            var purchases = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();

            return View(purchases);
        }

        // 2. Purchase Details (تفاصيل الفاتورة)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var purchase = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .FirstOrDefaultAsync(m => m.PurchaseId == id);

            if (purchase == null) return NotFound();

            return View(purchase);
        }

        // 3. Create Purchase (GET)
        public async Task<IActionResult> Create()
        {
            ViewBag.Suppliers = new SelectList(await _context.Suppliers.ToListAsync(), "SupplierId", "Name");

            ViewBag.Products = await _context.Products
                .Select(p => new { Id = p.ProductId, Name = p.ProductName, CostPrice = p.UnitPrice })
                .ToListAsync();

            return View(new CreatePurchaseViewModel());
        }

        // 4. Create Purchase (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePurchaseViewModel model)
        {
            if (model.Items == null || !model.Items.Any())
            {
                ModelState.AddModelError("", "At least one item must be added to the purchase.");
            }

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
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
                        decimal lineTotal = item.Quantity * item.UnitCost;
                        totalAmount += lineTotal;

                        var purchaseItem = new PurchaseItem
                        {
                            PurchaseId = purchase.PurchaseId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitCost = item.UnitCost,
                            Price = lineTotal
                        };
                        _context.PurchaseItems.Add(purchaseItem);

                        // Increment stock quantity after purchase
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += item.Quantity;
                            _context.Products.Update(product);
                        }
                    }

                    purchase.TotalAmount = totalAmount;
                    _context.Purchases.Update(purchase);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "An unexpected error occurred while saving the purchase.");
                }
            }

            ViewBag.Suppliers = new SelectList(await _context.Suppliers.ToListAsync(), "SupplierId", "SupplierName", model.SupplierId);
            ViewBag.Products = await _context.Products
                .Select(p => new { Id = p.ProductId, Name = p.ProductName, CostPrice = p.UnitPrice })
                .ToListAsync();

            return View(model);
        }
    }
    }

