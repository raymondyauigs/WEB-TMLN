using HYDrmb.Framework.metadata;
using System.ComponentModel.DataAnnotations;
using HYDrmb.Abstraction;
using System.ComponentModel.DataAnnotations.Schema;

namespace HYDrmb.Framework
{


    [MetadataType(typeof(SettingEdit_MetaData))]
    public partial class CoreSetting: IEditSettingModel
    {

    }



}
