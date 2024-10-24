using ManagementApp.Models;
using System.Collections.Generic;

namespace ManagementApp.Repository
{
    public interface IUserRepository
    {
        User ValidateUser(string email, string password);
        IEnumerable<User> GetAllUsers();
        int CreateUser(User user, string categories);
        bool UpdateUser(User user, string categories);
        bool DeleteUser(int id);
        IEnumerable<Category> GetCategories();
        IEnumerable<SubCategory> GetSubCategories(int categoryId);
        IEnumerable<UserCategory> GetUserCategories(int userId);
        IEnumerable<Category> GetCategoriesWithAssignedSubCategories(int userId);
        IEnumerable<SubCategory> GetUserAssignedSubCategories(int userId);
        IEnumerable<Category> GetCategoriesWithSubCategories();
    }
}
