using HYDrmb.Abstraction;
using HYDrmb.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI = HYDrmb.Abstraction.Constants.UI;
using DT = HYDrmb.Abstraction.Constants.DT;
using HYDrmb.Framework.AppModel;
using NPOI.OpenXmlFormats.Encryption;

namespace HYDrmb.Service
{
    public class SettingsService : ISettingService
    {
        IMiscLog log;
        HYDrmbEntities db;
        public SettingsService(IMiscLog mlog,HYDrmbEntities mdb) { 
            log = mlog;
            db = mdb;
        }

        public IEnumerable<KeyValuePair<string, string>> GetSettingFor(string type,int target=0)
        {
            if(type == UI.SETT_PREFERENCE)
            {
                yield return new KeyValuePair<string, string>(nameof(PreferenceType.FULL),DT.WHOLE);
                yield return new KeyValuePair<string, string>(nameof(PreferenceType.AM), nameof(PreferenceType.AM));
                yield return new KeyValuePair<string, string>(nameof(PreferenceType.PM), nameof(PreferenceType.PM));
            }
            else if(type == UI.SETT_CARSAVAIL)
            {
                var carmax = db.CoreSettings.Where(e=> e.SettingId == UI.SETT_CARSAVAIL).FirstOrDefault();
                yield return new KeyValuePair<string, string>(carmax.SettingValue,DT.CARMAX);
            }
            else if(type == UI.SETT_LOCATION)
            {
                var locations = db.CoreSettings.Where(e => e.SettingId == UI.SETT_LOCATION).Select(e=> e.SettingValue).FirstOrDefault();
                if(locations!=null)
                {
                    foreach(var l in locations.ItSplit("|").OrderBy(e=>e))
                    {
                        yield return new KeyValuePair<string, string>(l, l);
                    }
                }
            }
            else if(type==UI.SETT_YESNO)
            {
                yield return new KeyValuePair<string, string>(DT.YES,DT.YES);
                yield return new KeyValuePair<string, string>(DT.NO,DT.NO);

            }
            else if(type==UI.SETT_UPOSTTYPE)
            {
                var posts = db.CoreSettings.Where(e => e.SettingId == UI.SETT_UPOSTTYPE).Select(e => e.SettingValue).FirstOrDefault();
                if(posts!=null)
                {
                    foreach (var p in posts.ItSplit(",").OrderBy(e=>e))
                        yield return new KeyValuePair<string, string>(p, p);
                }

            }
            else if(type == UI.SETT_SESSNTYPE)
            {
                var posts = db.CoreSettings.Where(e => e.SettingId == UI.SETT_SESSNTYPE).Select(e => e.SettingValue).FirstOrDefault();
                if (posts != null)
                {
                    var descHash = EditModels._KEYVALS.ToDictionary(e => e.Value, e => e.Key);
                    foreach (var p in posts.ItSplit(","))
                    {
                        
                        yield return new KeyValuePair<string, string>(descHash[p], p);
                    }
                        
                }
            }
            

        }
    }
}
