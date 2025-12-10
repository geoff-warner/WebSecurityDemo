using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSecurityDemo.Repositories;
using WebSecurityDemo.ViewModels;

namespace WebSecurityDemo.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly ILogger<RoleController> _logger;
        private readonly RoleRepository _roleRepo;

        public RoleController(ILogger<RoleController> logger,
                              RoleRepository roleRepo)
        {
            _logger = logger;
            _roleRepo = roleRepo;
        }

        public IActionResult Index()
        {
            List<RoleVM> roleVM = _roleRepo.GetAllRoles();
            return View(roleVM);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(RoleVM roleVM)
        {
            if (ModelState.IsValid)
            {
                bool isSuccess = _roleRepo.CreateRole(roleVM.RoleName);

                if (isSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Role creation failed." +
                                             " The role may already exist.");
                }
            }
            return View(roleVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public IActionResult Delete(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return RedirectToAction(nameof(Index));
            }

            bool success = _roleRepo.DeleteRole(roleName);

            if (!success)
            {
                TempData["Message"] = $"Failed to delete role: {roleName}";
            }
            else
            {
                TempData["Message"] = $"Role '{roleName}' was deleted.";
            }

            return RedirectToAction(nameof(Index));
        }





    }
}


