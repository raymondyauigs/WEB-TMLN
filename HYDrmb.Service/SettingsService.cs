﻿using HYDrmb.Abstraction;
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

        public string GetValue(string type)
        {
            if (new[] { UI.SETT_LAYOUTNAME }.Contains(type))
            {
                return db.CoreSettings.FirstOrDefault(e => e.SettingId == type)?.SettingValue;
            }
            return null;
        }

        public T GetValueFor<T>(string type) where T: struct , IComparable, IFormattable, IComparable<T>, IEquatable<T>
        {
            T result = default(T);


            return result;
        }
        public IEnumerable<KeyValuePair<string, string>> GetSettingFor(string type,int target=0)
        {
            if (type == UI.SETT_NOTIFY)
            {
                var notifysettings = new[] { UI.NOTIFY_SUBJ,UI.NOTIFY_TO, UI.NOTIFY_CC, UI.NOTIFY_SERVER, UI.NOTIFY_PORT };
                foreach (var it in db.CoreSettings.Where(e => notifysettings.Contains(e.SettingId))
                    .ToList())
                {
                    yield return new KeyValuePair<string, string>(it.SettingId, it.SettingValue);
                }

            }
            else if(type == UI.SETT_ROOMTYPE)
            {
                var roomtypes = db.RmbResources.Where(e => e.ResourceType.EndsWith(".Room")).ToList();
                foreach(var t in roomtypes.OrderBy(e=> e.ResourceName))
                {
                    yield return new KeyValuePair<string, string>(t.ResourceName, t.ResourceName);
                }
            }
            else if(type == UI.SETT_PREFERENCE)
            {
                yield return new KeyValuePair<string, string>(nameof(PreferenceType.FULL),DT.WHOLE);
                yield return new KeyValuePair<string, string>(nameof(PreferenceType.AM), nameof(PreferenceType.AM));
                yield return new KeyValuePair<string, string>(nameof(PreferenceType.PM), nameof(PreferenceType.PM));
                yield return new KeyValuePair<string, string>(nameof(PreferenceType.CUSTOM), nameof(PreferenceType.CUSTOM));
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
                    foreach (var p in posts.ItSplit("|"))
                    {
                        
                        yield return new KeyValuePair<string, string>(descHash[p], p);
                    }
                        
                }
            }
            

        }
    }
}
