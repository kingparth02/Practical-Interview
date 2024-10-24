using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Dapper;
using ManagementApp.Models;
using System.Linq;

namespace ManagementApp.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public User ValidateUser(string email, string password)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var parameters = new { Email = email, Password = password };
                return db.QueryFirstOrDefault<User>("sp_UserLogin", parameters, commandType: CommandType.StoredProcedure);
            }
        }


        public IEnumerable<User> GetAllUsers()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<User>("sp_GetAllUsers", commandType: CommandType.StoredProcedure);
            }
        }

        public int CreateUser(User user, string categories)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var parameters = new
                {
                    Name = user.Name,
                    Phone = user.Phone,
                    Email = user.Email,
                    Password = user.Password,
                    IsAdmin = user.IsAdmin,
                    Categories = categories
                };

                return db.ExecuteScalar<int>("sp_CreateUser", parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<UserCategory> GetUserCategories(int userId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<UserCategory>("SELECT Id, UserId, CategoryId FROM UserCategory WHERE UserId = @UserId",
                    new { UserId = userId });
            }
        }

        public bool UpdateUser(User user, string categories)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@Id", user.Id);
                        parameters.Add("@Name", user.Name);
                        parameters.Add("@Phone", user.Phone);
                        parameters.Add("@Email", user.Email);
                        parameters.Add("@Categories", categories);

                        var result = db.Execute("sp_UpdateUser", parameters,
                            transaction: transaction,
                            commandType: CommandType.StoredProcedure);

                        transaction.Commit();
                        return result > 0;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool DeleteUser(int id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@Id", id);

                        Console.WriteLine($"Attempting to delete user with Id: {id}");

                        var result = db.Execute("sp_DeleteUser", parameters,
                            transaction: transaction,
                            commandType: CommandType.StoredProcedure);

                        if (result > 0)
                        {
                            transaction.Commit();
                            return true;
                        }
                        else
                        {
                            transaction.Rollback();
                            Console.WriteLine("Failed to delete the user.");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log more detailed information
                        transaction.Rollback();
                        Console.WriteLine("Error in DeleteUser: " + ex.Message);
                        throw;
                    }
                }
            }
        }





        public IEnumerable<Category> GetCategories()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<Category>("SELECT Id, Name FROM Category");
            }
        }

        public IEnumerable<SubCategory> GetSubCategories(int categoryId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var parameters = new { CategoryId = categoryId };
                return db.Query<SubCategory>("sp_GetSubCategoriesByCategory", parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<Category> GetCategoriesWithAssignedSubCategories(int userId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                // Get all categories
                var categories = db.Query<Category>("SELECT Id, Name FROM Category").ToList();

                // Get assigned subcategories for the user
                var assignedSubCategories = GetUserAssignedSubCategories(userId).ToList();

                // Associate subcategories to categories
                foreach (var category in categories)
                {
                    category.SubCategories = assignedSubCategories.Where(s => s.CategoryId == category.Id).ToList();
                }

                return categories.Where(c => c.SubCategories.Any()).ToList(); // Return only categories with assigned subcategories
            }
        }

        public IEnumerable<Category> GetCategoriesWithSubCategories()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                // Query to get all categories
                var categories = db.Query<Category>("SELECT Id, Name FROM Category").ToList();

                // Query to get all subcategories
                var subCategories = db.Query<SubCategory>("SELECT Id, Name, CategoryId FROM SubCategory").ToList();

                // Loop through categories and associate subcategories
                foreach (var category in categories)
                {
                    category.SubCategories = subCategories.Where(s => s.CategoryId == category.Id).ToList();
                }

                return categories; // Return categories with their associated subcategories
            }
        }

        public IEnumerable<Category> GetAllCategoriesWithSubCategories()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                // Fetch all categories and their subcategories
                var categories = db.Query<Category>("SELECT * FROM Category").ToList();
                var subCategories = db.Query<SubCategory>("SELECT * FROM SubCategory").ToList();

                // Associate subcategories with categories
                foreach (var category in categories)
                {
                    category.SubCategories = subCategories.Where(s => s.CategoryId == category.Id).ToList();
                }

                return categories;
            }
        }

        public IEnumerable<SubCategory> GetUserAssignedSubCategories(int userId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<SubCategory>(
                    "SELECT s.Id, s.Name " +
                    "FROM UserSubCategories us " +
                    "JOIN SubCategory s ON us.SubCategoryId = s.Id " +
                    "WHERE us.UserId = @UserId",
                    new { UserId = userId }
                ).ToList();
            }
        }


    }

}
