using HYDrmb.Abstraction;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace HYDrmb.Framework.metadata
{

    

    public class rmbReservationview_meta
    {
        public int Id { get; set; }
        [Display(Name = "Reservation Start")]
        public System.DateTime ReservedStartAt { get; set; }
        [Display(Name = "Reservation End")]
        public System.DateTime ReservedEndAt { get; set; }
        [Display(Name = "Reservation Date")]
        public Nullable<System.DateTime> ReservedDate { get; set; }
        [Display(Name = "Session")]
        public string SessionType { get; set; }
        [Display(Name = "Contact Name")]
        public string ContactName { get; set; }
        [Display(Name = "Contact Post")]
        public string ContactPost { get; set; }
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }
        [Display(Name = "Room")]
        public string RoomName { get; set; }
        [Display(Name = "Location")]
        public string RoomLocation { get; set; }
        [Display(Name = "Location Type")]
        public string LocationType { get; set; }
        [Display(Name = "Room Reserved")]
        public string RoomType { get; set; }
        [Display(Name = "From Time")]
        public string FromTime { get; set; }
        [Display(Name = "Till Time")]
        public string TillTime { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

    }

    
}  



