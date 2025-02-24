using HYDrmb.Framework.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HYDrmb.Framework.AppModel
{
    public class CreateUserModel
    {
        [Required(ErrorMessage = "Please Enter User ID..")]
        [Display(Name = "User ID")]
        
        public string UserId { get; set; }

        [Required(ErrorMessage = "Please Enter Username..")]
        [Display(Name = "UserName")]
        
        public string UserName { get; set; }


        [Required(ErrorMessage = "Please Enter Password...")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        
        public string Pwd { get; set; }
        
        [Required(ErrorMessage = "Please Enter the Confirm Password...")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Pwd")]
        public string Confirmpwd { get; set; }

        [Required(ErrorMessage = "Please Enter the level Code...")]
        [Display(Name = "Access Level")]
        public int Level { get; set; }

        [RegularExpression("^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$", ErrorMessage = "Invalid Email Format")]
        [Display(Name = "Email")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Please Enter the Post ...")]
        [Display(Name = "Post")]
        [Compare(nameof(UserId), ErrorMessage = "User ID must equal to Post. Please input again before register.")]
        public string Post { get; set; }

        [Display(Name = "Telephone")]
        public string Telephone { get; set; }   


        [Display(Name = "Division")]
        public string Division { get; set; }

        [Required(ErrorMessage = "Please Enter Is Admin ...")]
        [Display(Name = "Admin?")]
        public bool IsAdmin { get; set; }
        [RequiredForAny(PropertyName =nameof(IsAdmin), Values = new[] {(object)false })]
        [Display(Name = "Power User?")]
        public bool IsPowerUser { get; set; }
    }
}