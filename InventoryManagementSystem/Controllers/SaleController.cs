using InventoryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    public class SaleController : Controller
    {
        private readonly InventoryDbContext _context;

        public SaleController(InventoryDbContext context)
        {
            _context = context;
        }

        // GET: Sale
        public async Task<IActionResult> Index()
        {
            var sales = await _context.Sales
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            return View(sales);
        }

        // GET: Sale/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sale = await _context.Sales
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync(s => s.SaleId == id);

            if (sale == null)
            {
                return NotFound();
            }

            return View(sale);
        }

        // GET: Sale/Create
        public async Task<IActionResult> Create()
        {
            var products = await _context.Products
                .Where(p => p.StockQuantity > 0)
                .ToListAsync();

            ViewBag.Products = products;

            return View();
        }

        // POST: Sale/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int[] productIds,
            int[] quantities)
        {
            if (productIds == null || quantities == null ||
                productIds.Length == 0 ||
                productIds.Length != quantities.Length)
            {
                ModelState.AddModelError("", "Please select products and quantities.");
                return await Create();
            }

            var saleItems = new List<SaleItem>();
            decimal totalAmount = 0;

            for (int i = 0; i < productIds.Length; i++)
            {
                if (quantities[i] <= 0)
                {
                    ModelState.AddModelError("", "Quantity must be greater than zero.");
                    return await Create();
                }

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productIds[i]);

                if (product == null)
                {
                    ModelState.AddModelError("", "Product not found.");
                    return await Create();
                }

                // Prevent selling more than available stock
                if (quantities[i] > product.StockQuantity)
                {
                    ModelState.AddModelError(
                        "",
                        $"Not enough stock for {product.ProductName}. Available: {product.StockQuantity}");

                    return await Create();
                }

                var itemTotal = quantities[i] * product.UnitPrice;

                saleItems.Add(new SaleItem
                {
                    ProductId = product.ProductId,
                    Quantity = quantities[i],
                    UnitPrice = product.UnitPrice
                });

                totalAmount += itemTotal;

                // Decrease stock
                product.StockQuantity -= quantities[i];
            }

            var sale = new Sale
            {
                SaleDate = DateTime.Now,
                TotalAmount = totalAmount,
                SaleItems = saleItems
            };

            _context.Sales.Add(sale);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Sale/Receipt/5
        public async Task<IActionResult> Receipt(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sale = await _context.Sales
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync(s => s.SaleId == id);

            if (sale == null)
            {
                return NotFound();
            }

            return View(sale);
        }

        // GET: Low Stock
        public async Task<IActionResult> LowStock()
        {
            var products = await _context.Products
                .Where(p => p.StockQuantity <= p.LowStockThreshold)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();

            return View(products);
        }
    }
}