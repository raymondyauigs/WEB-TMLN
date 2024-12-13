using HYDtmn.Framework.metadata;
using System.ComponentModel.DataAnnotations;
using HYDtmn.Abstraction;
using System.ComponentModel.DataAnnotations.Schema;

namespace HYDtmn.Framework
{


    [MetadataType(typeof(SettingEdit_MetaData))]
    public partial class CoreSetting: IEditSettingModel
    {

    }



}
