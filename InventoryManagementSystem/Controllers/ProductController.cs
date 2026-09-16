using InventoryManagementSystem.Models;
using InventoryManagementSystem.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    public class ProductController : Controller
    {
        private readonly InventoryDbContext Context;

        public ProductController(InventoryDbContext context)
        {
            Context = context;
        }
        public IActionResult Index(string search, int? categoryId, int page = 1)
        {
            int pageSize = 5;

            var products = Context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p =>
                    p.ProductName.ToLower().Contains(search.ToLower()) ||
                    p.SKU.ToLower().Contains(search.ToLower()));
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId== categoryId.Value);
            }

            ViewBag.Categories = Context.Categories.ToList();

            int totalProducts = products.Count();

            var pagedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            return View(pagedProducts);
        }
        public IActionResult Details(int id)
        {
            var product = Context.Products
                .Include(p => p.Category)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Purchase)
                        .ThenInclude(p => p.Supplier)
                .Include(p => p.SaleItems)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }
            var vm = new DetailsProVM
            {
                ProductID = product.ProductId,
                SKU = product.SKU,
                ProductName = product.ProductName,
                CategoryName = product.Category.CategoryName,
                UnitPrice = product.UnitPrice,
                StockQuantity = product.StockQuantity,
                LowStockThreshold = product.LowStockThreshold,

                TotalPurchasedQuantity = product.PurchaseItems
                    .Sum(x => x.Quantity),

                TotalSoldQuantity = product.SaleItems
                    .Sum(x => x.Quantity),

                SupplierName = product.PurchaseItems
                    .Select(x => x.Purchase.Supplier.SupplierName)
                    .FirstOrDefault()
            };

            return View("Details", vm);
        }
        [HttpGet]
        public IActionResult AddProduct()
        {
            AddProVM addProVM = new AddProVM();

            addProVM.categories = Context.Categories.ToList();

            return View("AddProduct", addProVM);
        }
        [HttpPost]
        public IActionResult SaveAdd(AddProVM addProVM)
        {
            if (ModelState.IsValid)
            {
                Product product = new Product
                {
                    SKU = addProVM.ProSKU.ToString(),
                    ProductName = addProVM.ProName,
                    CategoryId = addProVM.CategoryId,
                    UnitPrice = addProVM.ProPrice,
                    StockQuantity = addProVM.StockQuantity,
                    LowStockThreshold = addProVM.LowStockQuantity
                };

                Context.Products.Add(product);
                Context.SaveChanges();

                return RedirectToAction("Index", addProVM);
            }

            addProVM.categories = Context.Categories.ToList();

            return View("AddProduct", addProVM);
        }
        public IActionResult EditProduct(int id)
        {
            var product = Context.Products.FirstOrDefault(e => e.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            EditProVM editProVM = new EditProVM
            {
                ProId = product.ProductId,
                ProSKU = product.SKU,
                ProName = product.ProductName,
                CategoryId = product.CategoryId,
                ProPrice = product.UnitPrice,
                StockQuantity = product.StockQuantity,
                LowStockQuantity = product.LowStockThreshold,
                categories = Context.Categories.ToList()
            };

            return View("EditProduct", editProVM);
        }

        [HttpPost]
        public IActionResult SaveEdit(EditProVM editProVM)
        {
            if (!ModelState.IsValid)
            {
                editProVM.categories = Context.Categories.ToList();
                return View("EditProduct", editProVM);
            }

            var product = Context.Products.FirstOrDefault(e => e.ProductId == editProVM.ProId);

            if (product == null)
            {
                return NotFound();
            }

            product.SKU = editProVM.ProSKU;
            product.ProductName = editProVM.ProName;
            product.CategoryId = editProVM.CategoryId;
            product.UnitPrice = editProVM.ProPrice;
            product.StockQuantity = editProVM.StockQuantity;
            product.LowStockThreshold = editProVM.LowStockQuantity;

            Context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult RemoveProduct(int id)
        {
            var product = Context.Products.FirstOrDefault(e => e.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View("RemoveProduct", product);
        }
        [HttpPost]
        [ActionName("RemoveProduct")]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = Context.Products.FirstOrDefault(e => e.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            Context.Products.Remove(product);
            Context.SaveChanges();

            return RedirectToAction("Index");
        }
        public IActionResult Stock()
        {
            var products = Context.Products.ToList();

            return View("Product",products);
        }
    }
}
