using InventoryManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    public class InventoryAssistantController : Controller
    {
        private readonly InventoryAssistantService _assistantService;

        public InventoryAssistantController(
            InventoryAssistantService assistantService)
        {
            _assistantService = assistantService;
        }


        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Ask(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                ViewBag.Question = question;
                ViewBag.Answer = "من فضلك اكتب سؤالك.";

                return View("Index");
            }


            try
            {
                var answer =
                    await _assistantService.AskAsync(question);

                ViewBag.Question = question;
                ViewBag.Answer = answer;
            }
            catch (Exception ex)
            {
                ViewBag.Question = question;

                ViewBag.Answer =
                    "حدث خطأ أثناء تشغيل الـ AI Assistant.\n\n" +
                    ex.Message;
            }


            return View("Index");
        }
    }
}