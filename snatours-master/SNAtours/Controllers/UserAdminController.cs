using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Net;


namespace SNAtours.Controllers
{
    [Authorize(Roles = "EnableManageRole(Admin)")]
    public class UserAdminController : Controller
    {
        public UserAdminController() { }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }


        public UserAdminController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        //
        // GET: /Users/
        public async Task<ActionResult> Index()
        {
            return View(await UserManager.Users.ToListAsync());
        }

        // GET: /Users/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);

            ViewBag.RoleNames = await UserManager.GetRolesAsync(user.Id);

            return View(user);
        }

        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userRoles = await UserManager.GetRolesAsync(user.Id);
            return View(new SNAtours.Models.EditUserViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem() { Selected = userRoles.Contains(x.Name), Text = x.Name, Value = x.Name })
            });
        }

        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(
            [Bind(Include = "Email,Id,Address,City,State,PostalCode")] SNAtours.Models.EditUserViewModel editUser, params string[] selectedRoles)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(editUser.Id);
                if (user != null)
                {
                    user.UserName = editUser.Email;
                    user.Email = editUser.Email;
                    //user.Address = editUser.Address;
                    //user.City = editUser.City;
                    //user.State = editUser.State;
                    //user.PostalCode = editUser.PostalCode;

                    var userRoles = await UserManager.GetRolesAsync(user.Id);
                    selectedRoles = selectedRoles ?? new string[] { };
                    var result = await UserManager.AddToRolesAsync(user.Id, selectedRoles.Except(userRoles).ToArray<string>());

                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", result.Errors.First());
                        return View();
                    }
                    result = await UserManager.RemoveFromRolesAsync(user.Id, userRoles.Except(selectedRoles).ToArray<string>());
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", result.Errors.First());
                        return View();
                    }
                    return RedirectToAction("Index");

                }
                else
                {
                    return HttpNotFound();
                }
            }
            else
            {
                ModelState.AddModelError("", "Edit user failed.");
                return View();
            }
        }

        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirm(string id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var user = await UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                var result = await UserManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("DeleteUser", result.Errors.First());
                    return View();
                }

                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }
        // GET: /Users/Create
        public async Task<ActionResult> Create()
        {
            //Get the list of Roles
            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(SNAtours.Models.RegisterViewModel userViewModel, params string[] selectedRoles)
        {
            if (ModelState.IsValid)
            {
                var user = new SNAtours.Models.ApplicationUser()
                {
                    UserName = userViewModel.Email,
                    Email = userViewModel.Email
                };

                var createResult = await UserManager.CreateAsync(user, userViewModel.Password);

                if (createResult.Succeeded)
                {
                    // add user to the role
                    if (selectedRoles != null && selectedRoles.Count() > 0)
                    {
                        var addToRoleResult = await UserManager.AddToRolesAsync(user.Id, selectedRoles);
                        if (!addToRoleResult.Succeeded)
                        {
                            ModelState.AddModelError("", addToRoleResult.Errors.First());
                            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
                            return View();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", createResult.Errors.First());
                        ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
                        return View();

                    }
                    return RedirectToAction("Index");
                }
            }

            ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
            return View();
        }
    }
}