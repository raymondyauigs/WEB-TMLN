using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HYDtmn.Framework.AppModel
{

    public class ChangePwdModel
    {

        [Display(Name = "Id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please Enter Username..")]
        [Display(Name = "User Name")]
        public string UserName { get; set; }


        [Required(ErrorMessage = "Please Enter New Password...")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string Pwd { get; set; }

        [Required(ErrorMessage = "Please Confirm New Password...")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("Pwd")]
        public string Confirmpwd { get; set; }

    }

    public class ChangePasswordModel
    {
                
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please Enter Username..")]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please Enter Old Password...")]
        [DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        public string OldPwd { get; set; }


        [Required(ErrorMessage = "Please Enter New Password...")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string Pwd { get; set; }

        [Required(ErrorMessage = "Please Confirm New Password...")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("Pwd")]
        public string Confirmpwd { get; set; }


        public string ReturnUrl { get; set; }

    }
}