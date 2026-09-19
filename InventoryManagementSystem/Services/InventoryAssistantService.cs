using InventoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace InventoryManagementSystem.Services
{
    public class InventoryAssistantService
    {
        private readonly InventoryDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public InventoryAssistantService(
            InventoryDbContext context,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<string> AskAsync(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                return "من فضلك اكتب سؤالك.";
            }

            // ==========================================
            // PRODUCTS
            // ==========================================

            var products = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .Select(p => new
                {
                    p.ProductId,
                    p.SKU,
                    p.ProductName,

                    Category = p.Category != null
                        ? p.Category.CategoryName
                        : "غير محدد",

                    p.UnitPrice,
                    p.StockQuantity,
                    p.LowStockThreshold
                })
                .Take(100)
                .ToListAsync();


            // ==========================================
            // SUPPLIERS
            // ==========================================

            var suppliers = await _context.Suppliers
                .AsNoTracking()
                .Select(s => new
                {
                    s.SupplierId,
                    s.SupplierName,
                    s.ContactName,
                    s.Phone,
                    s.Email,
                    s.Address
                })
                .Take(50)
                .ToListAsync();


            // ==========================================
            // PURCHASES
            // ==========================================

            var purchases = await _context.Purchases
                .Include(p => p.Supplier)
                .AsNoTracking()
                .OrderByDescending(p => p.PurchaseDate)
                .Select(p => new
                {
                    p.PurchaseId,

                    Supplier = p.Supplier != null
                        ? p.Supplier.SupplierName
                        : "غير محدد",

                    p.PurchaseDate,
                    p.TotalAmount
                })
                .Take(30)
                .ToListAsync();


            // ==========================================
            // SALES
            // ==========================================

            var sales = await _context.Sales
                .AsNoTracking()
                .OrderByDescending(s => s.SaleDate)
                .Select(s => new
                {
                    s.SaleId,
                    s.SaleDate,
                    s.TotalAmount,
                    s.CustomerInfo
                })
                .Take(30)
                .ToListAsync();


            // ==========================================
            // BUILD DATABASE CONTEXT
            // ==========================================

            var inventoryData = new StringBuilder();

            inventoryData.AppendLine("===== INVENTORY DATABASE =====");
            inventoryData.AppendLine();


            // PRODUCTS

            inventoryData.AppendLine("===== PRODUCTS =====");

            foreach (var product in products)
            {
                inventoryData.AppendLine(
                    $"ID: {product.ProductId} | " +
                    $"SKU: {product.SKU} | " +
                    $"Name: {product.ProductName} | " +
                    $"Category: {product.Category} | " +
                    $"Price: {product.UnitPrice} | " +
                    $"Stock: {product.StockQuantity} | " +
                    $"LowStockThreshold: {product.LowStockThreshold}"
                );
            }


            // SUPPLIERS

            inventoryData.AppendLine();
            inventoryData.AppendLine("===== SUPPLIERS =====");

            foreach (var supplier in suppliers)
            {
                inventoryData.AppendLine(
                    $"ID: {supplier.SupplierId} | " +
                    $"Name: {supplier.SupplierName} | " +
                    $"Contact: {supplier.ContactName} | " +
                    $"Phone: {supplier.Phone} | " +
                    $"Email: {supplier.Email} | " +
                    $"Address: {supplier.Address}"
                );
            }


            // PURCHASES

            inventoryData.AppendLine();
            inventoryData.AppendLine("===== RECENT PURCHASES =====");

            foreach (var purchase in purchases)
            {
                inventoryData.AppendLine(
                    $"Purchase ID: {purchase.PurchaseId} | " +
                    $"Supplier: {purchase.Supplier} | " +
                    $"Date: {purchase.PurchaseDate:yyyy-MM-dd} | " +
                    $"Total: {purchase.TotalAmount}"
                );
            }


            // SALES

            inventoryData.AppendLine();
            inventoryData.AppendLine("===== RECENT SALES =====");

            foreach (var sale in sales)
            {
                inventoryData.AppendLine(
                    $"Sale ID: {sale.SaleId} | " +
                    $"Date: {sale.SaleDate:yyyy-MM-dd} | " +
                    $"Total: {sale.TotalAmount} | " +
                    $"Customer: {sale.CustomerInfo}"
                );
            }


            // ==========================================
            // PROMPT
            // ==========================================

            var prompt = $"""
                You are an AI Inventory Assistant inside an Inventory Management System.

                Answer questions about:

                - Products
                - Stock
                - Low stock products
                - Categories
                - Suppliers
                - Purchases
                - Sales

                IMPORTANT RULES:

                1. Use ONLY the inventory data provided below.
                2. Never invent information.
                3. If the information is not available, say so clearly.
                4. Answer in the same language as the user's question.
                5. Keep answers clear and concise.
                6. Use bullet points when listing multiple items.
                7. A product is low stock when:
                   StockQuantity <= LowStockThreshold.
                8. Never modify the database.
                9. You are an information assistant only.

                INVENTORY DATA:

                {inventoryData}

                USER QUESTION:

                {question}
                """;


            // ==========================================
            // GEMINI API
            // ==========================================

            var apiKey = _configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return "Gemini API Key غير موجودة. تأكد من User Secrets.";
            }


            var model =
                _configuration["Gemini:Model"]
                ?? "gemini-2.5-flash";


            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";


            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = prompt
                            }
                        }
                    }
                },

                generationConfig = new
                {
                    temperature = 0.2,
                    maxOutputTokens = 1000
                }
            };


            var json = JsonSerializer.Serialize(requestBody);

            using var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );


            var response = await _httpClient.PostAsync(
                url,
                content
            );


            var responseBody =
                await response.Content.ReadAsStringAsync();


            if (!response.IsSuccessStatusCode)
            {
                return
                    $"Gemini API Error: {(int)response.StatusCode}\n\n" +
                    responseBody;
            }


            // ==========================================
            // PARSE RESPONSE
            // ==========================================

            using var document =
                JsonDocument.Parse(responseBody);


            var root = document.RootElement;


            var text =
                root
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();


            return text ?? "لم يتم الحصول على إجابة من Gemini.";
        }
    }
}