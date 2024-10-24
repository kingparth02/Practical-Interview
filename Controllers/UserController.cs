using ManagementApp.Models;
using ManagementApp.Repository;
using System.Linq;
using System.Web.Mvc;

namespace ManagementApp.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UserController()
        {
            _userRepository = new UserRepository();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UserDashboard()
        {
            int userId = (int)Session["UserId"]; // Get user ID from session
            var allCategories = _userRepository.GetCategoriesWithSubCategories();
            var assignedSubCategories = _userRepository.GetUserAssignedSubCategories(userId);

            // Filter categories to only include those that have assigned subcategories
            var filteredCategories = allCategories
                .Select(category => new Category
                {
                    Id = category.Id,
                    Name = category.Name,
                    SubCategories = category.SubCategories
                        .Where(sub => assignedSubCategories.Any(a => a.Id == sub.Id))
                        .ToList()
                })
                .Where(c => c.SubCategories.Any()) // Only include categories that have assigned subcategories
                .ToList();

            return View(filteredCategories); // Pass filtered categories to the view
        }
    }
}
