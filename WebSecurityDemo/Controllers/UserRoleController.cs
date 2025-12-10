using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebSecurityDemo.Repositories;
using WebSecurityDemo.ViewModels;

namespace WebSecurityDemo.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class UserRoleController : Controller
    {
        private readonly UserRepository _userRepo;
        private readonly RoleRepository _roleRepo;
        private readonly UserRoleRepository _userRoleRepo;

        public UserRoleController(UserRepository userRepo,
                                  RoleRepository roleRepo,
                                  UserRoleRepository userRoleRepo)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
        }

        // Show all users
        public IActionResult Index()
        {
            var users = _userRepo.GetAllUsers();
            return View(users);
        }

        // Show all roles for a specific user
        public async Task<IActionResult> Detail(string userName)
        {
            var roles = await _userRoleRepo.GetUserRolesAsync(userName);

            ViewBag.UserName = userName;
            return View(roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        // Delete role from user
        public async Task<IActionResult> Delete(string userName, string roleName)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(roleName))
            {
                TempData["Message"] = "Invalid request.";
                return RedirectToAction("Detail", new { userName });
            }

            var result = await _userRoleRepo.RemoveUserRoleAsync(userName, roleName);

            if (result)
            {
                TempData["Message"] = $"Role '{roleName}' removed from user '{userName}'.";
            }
            else
            {
                TempData["Message"] = $"Unable to remove role '{roleName}'.";
            }

            return RedirectToAction("Detail", new { userName });
        }

        // Present user with ability to assign roles to a user.
        public IActionResult Create(string userName)
        {
            BuildDropdownLists(userName);
            return View();
        }

        // Assigns role to user.
        [HttpPost]
        public async Task<IActionResult> Create(UserRoleVM userRoleVM)
        {
            if (!ModelState.IsValid)
            {
                BuildDropdownLists(userRoleVM.Email ?? string.Empty);
                return View(userRoleVM);
            }

            var success = await _userRoleRepo.AddUserRoleAsync(
                userRoleVM.Email,
                userRoleVM.Role);

            if (!success)
            {
                ModelState.AddModelError("",
                    "Unable to assign the role. The user may already " +
                    "have this role or the role does not exist.");

                BuildDropdownLists(userRoleVM.Email);
                return View(userRoleVM);
            }

            return RedirectToAction(nameof(Detail),
                new { userName = userRoleVM.Email });
        }

        private void BuildDropdownLists(string selectedUser)
        {
            // --- Users dropdown ---
            var users = _userRepo.GetAllUsers();

            ViewBag.UserSelectList = new SelectList(
                users.Select(u => new SelectListItem
                {
                    Value = u.Email,
                    Text = u.Email
                }),
                "Value",
                "Text",
                selectedUser
            );

            // --- Roles dropdown ---
            var roles = _roleRepo.GetAllRoles();

            ViewBag.RoleSelectList = new SelectList(
                roles.Select(r => new SelectListItem
                {
                    Value = r.RoleName,
                    Text = r.RoleName
                }),
                "Value",
                "Text"
            );

            ViewBag.SelectedUser = selectedUser;
        }
    }
}
