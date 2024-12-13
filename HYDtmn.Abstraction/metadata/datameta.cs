using HYDtmn.Abstraction;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYDtmn.Framework.metadata
{

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
