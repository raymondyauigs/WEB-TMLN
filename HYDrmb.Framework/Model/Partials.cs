using HYDrmb.Framework.metadata;
using System.ComponentModel.DataAnnotations;
using HYDrmb.Abstraction;
using System.ComponentModel.DataAnnotations.Schema;

namespace HYDrmb.Framework
{
    [MetadataType(typeof(rmbReservationview_meta))]
    public partial class rmbReservation_view: IviewReservation
    {

    }


    [MetadataType(typeof(SettingEdit_MetaData))]
    public partial class CoreSetting: IEditSettingModel
    {

    }



}
