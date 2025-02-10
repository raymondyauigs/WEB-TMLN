using HYDrmb.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using HYDrmb.Abstraction;
using System.Data;
using System.Runtime.InteropServices.WindowsRuntime;

namespace HYDrmb.Framework.AppModel
{

    public class QueryReservationModel
    {
        [Display(Name = "From Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]

        public DateTime? DateFrom { get; set; }

        [Display(Name = "Till Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DateTo { get; set; }
        public int page { get; set; }
        public int total { get; set; }

        public bool SelfOnly { get; set; }

        public List<rmbReservation_view> Records { get; set; }
    }


    public class QuerySettingModel
    {
        [Display(Name = "Show All")]
        public bool ShowAll { get; set; }

        [Display(Name = "Filter Setting Name")]
        public string AskCommon { get; set; }

        [Display(Name = "Setting Name")]
        public string ColumnSettingId { get; set; }
        [Display(Name = "Setting Value")]
        public string ColumnSettingValue { get; set; }

        public int page { get; set; }
        public bool allRecords { get; set; }

        public string sort { get; set; }

        public int totalRecords { get; set; }
    }

    
    
    
    public class QueryUserModel
    {

        [Display(Name = "Show All")]
        public bool ShowAll { get; set; }

        [Display(Name = "Filter Name")]
        public string AskUserName { get; set; }
        [Display(Name ="User Name")]
        public string ColumnUserName { get; set; }


        [Display(Name = "Post")]
        public string ColumnPost { get; set; }

        [Display(Name = "Division")]
        public string ColumnDivision { get; set; }
        [Display(Name = "Email")]
        public string ColumnEmail { get; set; }


        [Display(Name ="Level")]
        public int ColumnLevel { get; set; }


        [Display(Name = "One Month")]
        public bool ColumnPower
        {
            get; set;
        }


        [Display(Name = "Admin")]
        public bool ColumnAdmin
        {
            get; set;
        }

        [Display(Name = "VIP")]
        public bool ColumnVIP
        {
            get; set;
        }

        [Display(Name = "Enabled")]
        public bool ColumnEnabled
        {
            get;set;
        }
        [Display(Name = "Request Change Password")]
        public bool ColumnReset
        {
            get;set;
        }


        [Display(Name = "Created At")]
        public bool ColumnCreatedAt { get; set; }
        [Display(Name = "Updated At")]
        public bool ColumnUpdatedAt { get; set; }
        public int page { get; set; }
        public bool allRecords { get; set; }

        public string sort { get; set; }

        public int totalRecords { get; set; }

    }

  

}