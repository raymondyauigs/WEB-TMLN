using HYDrmb.Abstraction;
using HYDrmb.Framework.Tools;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HYDrmb.Framework.AppModel
{
    public class EditUserModel: IEditUserModel
    {

        

        [Display(Name = "Id")]
        public int Id { get; set; }


        [Required(ErrorMessage = "Please Enter Id..")]
        [Display(Name = "User Id")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Please Enter Username..")]
        [Display(Name = "User Name")]
        public string UserName { get; set; }


        //[Required(ErrorMessage = "Please Enter Person Name..")]
        [Display(Name = "Person Name")]
        public string Person { get; set; }

        [Required(ErrorMessage = "Please Enter the level Code...")]
        [Display(Name = "Access Level")]
        public int Level { get; set; }

        [RegularExpression("^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$", ErrorMessage ="Invalid Email Format")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please Enter Disabled...")]
        [Display(Name = "Disabled")]
        public bool Disabled { get; set; }

        
        [Display(Name = "Enabled")]

        public bool Enabled { get {  return !Disabled; } }


        [Required(ErrorMessage = "Please Enter the Post ...")]
        [Display(Name = "Post")]
        public string Post { get; set; }
        
        [Display(Name = "Division")]
        public string Division { get; set; }

        [Required(ErrorMessage = "Please Enter Admin ...")]
        [Display(Name = "Admin?")]
        public bool IsAdmin { get; set; }
        [Required(ErrorMessage = "Please Enter Admin ...")]
        // (One Month Booking)
        [Display(Name = "Power User?")]
        public bool IsPowerUser { get; set; }

        public bool IsReset {
            get;set;
        }
        // (Have Car available in all Priorities)
        [Display(Name = "VIP User?")]
        public bool IsVIP { get; set; }

        [DisplayFormat(ApplyFormatInEditMode =true,DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? UpdatedAt { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? CreatedAt { get; set; }
    }
}