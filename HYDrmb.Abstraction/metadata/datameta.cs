using HYDrmb.Abstraction;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYDrmb.Framework.metadata
{
    public class RmbReservationEdit_MetaData
    {
        [Display(Name = "Session Date")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Please enter Reservation Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime SessionDate { get; set; }
        [Display(Name = "Session Start")]
        public DateTime SessionStart { get; set; }
        [Display(Name = "Session End")]
        public DateTime SesssionEnd { get; set; }
        [Display(Name = "Session Type")]
        public string SessionType { get; set; }
        [Display(Name = "Contact Name")]
        public string ContactName { get; set; }
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }
        [Display(Name = "Contact Post")]
        public string ContactPost { get; set; }
        [Display(Name = "Room")]
        public string RoomType { get; set; }
        [Display(Name = "Location")]
        public string LocationType { get; set; }
        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        public Nullable<DateTime> updatedAt { get; set; }
        public string updatedBy { get; set; }

        public bool Invalid { get; set; }
    }
    public class SettingEdit_MetaData
    {
        [Display(Name ="Name")]
        public string SettingId { get; set; }
        [Display(Name = "Value")]
        public string SettingValue { get; set;}

        [Display(Name = "Editable?")]
        public bool CanEdit { get; set; }   
    }
    
    
}
