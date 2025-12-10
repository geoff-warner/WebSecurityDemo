using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSecurityDemo.Models;
using WebSecurityDemo.Repositories;

namespace WebSecurityDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly CustomerRepo _customerRepo;

        public HomeController(CustomerRepo customerRepo)
        {
            _customerRepo = customerRepo;
        }

        // Home page – show all customers
        public IActionResult Index(string message = "")
        {
            ViewBag.Message = message;
            var customers = _customerRepo.GetAllCustomers();
            return View(customers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public IActionResult Delete(int customerId)
        {
            string message = _customerRepo.DeleteCustomer(customerId);
            return RedirectToAction(nameof(Index), new { message });
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
