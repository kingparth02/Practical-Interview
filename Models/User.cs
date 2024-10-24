using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ManagementApp.Models
{
    //public class User
    //{
    //    public int Id { get; set; }

    //    [Required(ErrorMessage = "Name is required.")]
    //    [StringLength(100, ErrorMessage = "Name can't be longer than 100 characters.")]
    //    public string Name { get; set; }

    //    [Required(ErrorMessage = "Phone is required.")]
    //    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits.")]
    //    public string Phone { get; set; }

    //    [Required(ErrorMessage = "Email is required.")]
    //    [EmailAddress(ErrorMessage = "Invalid Email Address.")]
    //    public string Email { get; set; }

    //    [Required(ErrorMessage = "Password is required.")]
    //    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    //    [DataType(DataType.Password)]
    //    public string Password { get; set; }

    //    public bool IsAdmin { get; set; }

    //    // Foreign key relationships
    //    public int? CategoryId { get; set; }
    //    public int? SubCategoryId { get; set; }
    //}
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be 10 digits")]
        public string Phone { get; set; }
            
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        //[Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        public bool IsAdmin { get; set; }
        public string Categories { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    }

    public class SubCategory
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
    }

    public class UserCategory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }


}