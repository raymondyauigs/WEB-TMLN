using HYDrmb.Framework.metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace HYDrmb.Abstraction
{
    public interface IFromTillModel
    {
        int ReservationId { get; set; }        
        int TimeFrom { get; set; }
        int TimeTill { get; set; }
    }

    public interface IEventModel
    {
        string id { get; set; }
        DateTime start { get; set; }
        DateTime end { get; set; }
        string title { get; set; }
    }



    /*Reservation Service
     * 
     */
    public interface IReservationService
    {
        
        bool TransactionNow(Func<bool> doIt, string label = null);

        
        bool IsOccupied(int id, DateTime start, DateTime end, string roomtype);
        IRmbReservationEditModel GetReservation(int id, string userid);
        bool DeleteReservation(int[] ids, string userid);

        bool SaveReservation(IRmbReservationEditModel model, string userid);
        IEnumerable<IEventModel> GetEvents(string resourcetype,bool selfonly,string userid, string fromdate, string todate,Dictionary<string,string> colors);
        IEnumerable<IviewReservation> GetReservation(bool selfonly,string userid, string fromdate, string todate, string search, string type,string colid=nameof(IviewReservation.ReservedStartAt),string sort="asc");
    }

    /*Setting Service
     * 
     */
    public interface ISettingService
    {
        IEnumerable<KeyValuePair<string, string>> GetSettingFor(string type,int target=0);
    }

    /*User Service
     * 
     */

    public interface IUserService
    {
        bool UpdateProc<T>(string procname, T input) where T : class;
        bool TransactionNow(Func<bool> doIt, string label = null);
        string ImportDataTable(DataTable dt, string updateby, out int count);

        IEnumerable<KeyValuePair<int, string>> GetLevels(params int[] exclude);

        bool ChangeUserPassword(string userid, string password, string editedby, string oldpassword = null);
        
        object EditUser(string userid, string userName, string person, int level, string post, string tel, string division, bool isAdmin, string editedby, string adminScope=null, string email = null, string password = null, int Id = 0);
        bool LoginUser(string userid, string password, string editedby);

        
    }

    public interface ITabModel
    {
        string[] Titles { get; set; }
        int MaxCount { get; set; }

        

    }
    public interface IUrlModel
    {
        IUrl[] Urls { get; set; }
        int MaxCount { get; set; }
        string BaseUrl { get; set; }
        string UrlTitle { get; set; }
        int InitStart { get; set; }

        IUrlModel ExtractModel(int init, int len);
    }

    public interface IUrl
    {
        string Url { get; set; }
        string Type { get; set; }
        string Name { get; set; }
        int Thumb { get; set; }
    }
    public interface IBelongtoTable
    {
        string tablename { get; set; }
    }


    public interface IRmbReservationEditModel
    {
        int Id { get; set; }
        DateTime SessionDate { get; set; }
        DateTime SessionStart { get; set; }
        DateTime SessionEnd { get; set; }
        string SessionType { get; set; }
        string ContactName { get; set; }
        string ContactNumber { get; set; }
        string ContactPost { get; set; }
        string RoomType { get; set; }
        int RoomObjectId { get; set; }
        string LocationType { get; set; }
        string Remarks { get; set; }
        Nullable<DateTime> updatedAt { get; set; }
        string updatedBy { get; set; }
        bool Invalid { get; set; }

    }
    public interface IEditUserModel
    {
        int Id { get; set; }
        string UserName { get; set; }
        int Level { get; set; }

        string Person { get; set; }

        string Post { get; set; }

        string Division { get; set; }

        string Email { get; set; }

        bool IsAdmin { get; set; }

        bool IsPowerUser { get; set; }

        DateTime? CreatedAt { get; set; }

        DateTime? UpdatedAt { get; set; }

        bool IsReset { get; set; }

        bool IsVIP { get; set; }

        bool Disabled { get; set; }

        bool Enabled { get; }


    }



    public interface IEditSettingModel
    {
        string SettingId { get; set; }
        string SettingValue { get; set; }
        bool CanEdit { get; set; }
    }

    

    public interface IviewReservation
    {
        int Id { get; set; }
        System.DateTime ReservedStartAt { get; set; }
        System.DateTime ReservedEndAt { get; set; }
        Nullable<System.DateTime> ReservedDate { get; set; }
        string SessionType { get; set; }
        string ContactName { get; set; }
        string ContactPost { get; set; }
        string ContactNumber { get; set; }
        string RoomName { get; set; }
        string RoomLocation { get; set; }

        string LocationType { get; set; }

        string RoomType { get; set; }

        string Remarks { get; set; }
    }


    public interface IErrorLog
    {
        void LogError(string message, Exception ex = null);
    }

    public interface IMiscLog
    {
        void LogMisc(string message, Exception ex = null);
    }

    public interface IStdbLog
    {
        void LogStdb(string message, Exception ex = null);
    }




}
