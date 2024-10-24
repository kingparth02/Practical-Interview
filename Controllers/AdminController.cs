using ManagementApp.Models;
using ManagementApp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ManagementApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepository;

        public AdminController()
        {
            _userRepository = new UserRepository();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _userRepository.ValidateUser(model.Email, model.Password);
                if (user != null)
                {
                    // Set session variables for logged-in user
                    Session["UserId"] = user.Id;
                    Session["UserName"] = user.Name;
                    Session["IsAdmin"] = user.IsAdmin;

                    // Redirect based on role
                    if (user.IsAdmin)
                    {
                        return RedirectToAction("Index", "Admin"); // Admin panel
                    }
                    else
                    {
                        return RedirectToAction("Index", "User"); // User panel
                    }
                }

                ModelState.AddModelError("", "Invalid login attempt");
            }
            return View(model);
        }

        [AdminAuth]
        public ActionResult Index()
        {
            var users = _userRepository.GetAllUsers();
            return View(users);
        }

        [AdminAuth]
        public ActionResult Create()
        {
            ViewBag.Categories = _userRepository.GetCategories();
            return View();
        }


        [AdminAuth]
        [HttpPost]
        public ActionResult Create(User user, string categories)
        {
            if (ModelState.IsValid)
            {

                var userId = _userRepository.CreateUser(user, categories);


                TempData["SuccessMessage"] = "User created successfully!";


                return RedirectToAction("Index");
            }


            ViewBag.Categories = _userRepository.GetCategories();
            return View(user);
        }


        [AdminAuth]
        [HttpGet]
        public JsonResult GetSubCategories(int categoryId)
        {
            var subCategories = _userRepository.GetSubCategories(categoryId);
            return Json(subCategories, JsonRequestBehavior.AllowGet);
        }

        [AdminAuth]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var users = _userRepository.GetAllUsers();
            var user = users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return HttpNotFound();
            }

            // Get all categories for dropdown
            ViewBag.Categories = _userRepository.GetCategories();

            // Get user's selected categories
            var userCategories = _userRepository.GetUserCategories(id.Value);
            ViewBag.SelectedCategories = userCategories.Select(c => c.CategoryId).ToList();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User user, string categories)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    categories = !string.IsNullOrEmpty(categories)
                        ? string.Join(",", categories.Split(',').Select(c => c.Trim()))
                        : string.Empty;

                    var success = _userRepository.UpdateUser(user, categories);
                    if (!success)
                    {
                        return RedirectToAction("Index");
                    }

                    return RedirectToAction("Index");
                }

                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, message = "Validation failed", errors = errors });
            }
            catch (Exception ex)
            {
                // Log the exception here
                return Json(new { success = false, message = "An error occurred while updating the user.", error = ex.Message });
            }
        }



        [HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Delete(int id)
{
    try
    {
        var success = _userRepository.DeleteUser(id);
        if (success)
        {
            return RedirectToAction("Index");
        }
        return RedirectToAction("Index");
    }
    catch (Exception ex)
    {
        // Log the error
        Console.WriteLine($"Error deleting user {id}: {ex.Message}");
        return Json(new { success = false, message = "An error occurred while deleting the user." });
    }
}



        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        


    }

    public class AdminAuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Check if the user is logged in and if they are an admin
            if (filterContext.HttpContext.Session["UserId"] == null || !(bool)filterContext.HttpContext.Session["IsAdmin"])
            {
                filterContext.Result = new RedirectResult("~/Admin/Login"); // Redirect to login if not admin
            }

            base.OnActionExecuting(filterContext);
        }
    }

    public class UserAuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Check if the user is logged in and if they are not an admin
            if (filterContext.HttpContext.Session["UserId"] == null || (bool)filterContext.HttpContext.Session["IsAdmin"])
            {
                filterContext.Result = new RedirectResult("~/Admin/Login"); // Redirect to login if not user
            }

            base.OnActionExecuting(filterContext);
        }
    }

}

