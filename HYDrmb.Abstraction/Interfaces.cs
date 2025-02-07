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
        int BookingId { get; set; }        
        int TimeFrom { get; set; }
        int TimeTill { get; set; }
    }
    public interface IQueryDrivingModel
    {
        int DrivingDutyId { get; set; }
        string DriverTag { get; set; }
        string DriverName { get; set; }
        string DriverMobile { get; set; }
        string CarPlateNo { get; set; }
        int Priority { get; set; }
    }
    public interface IEventModel
    {
        string id { get; set; }
        DateTime start { get; set; }
        DateTime end { get; set; }
        string title { get; set; }
    }

    /*Driver Service
     * 
     */

    public interface IDriverService
    {
        bool TransactionNow(Func<bool> doIt, string label = null);
        bool HolidayBySub(IEditDriverModel editdriver, string userid);
        bool HolidayOn(DateTime onholidayDate, int id, string userid);
        bool EditDriver(IEditDriverModel editdriver, string userid);
        bool DeleteDriver(int id, string userid);
        bool RemoveDuty(int id, string userid);

        int GetPriorityFor(int drivingdutyid);
        bool RemoveReplacement(int id, string userid);
        bool RemoveHoliday(int id, string userid);
        IEnumerable<KeyValuePair<int, string>> GetPriorities();

        bool RemoveDrivingForDate(int bookingid, string userid, DateTime date, bool isvip, string sessiontype);


        bool AddDrivingForDate(IBkdBookingEditModel model,IBkdBookingComplianceModel sessionmodel);
        IEnumerable<IQueryDrivingModel> GetDrivingsValidForDate(DateTime date, bool isvip, string sessiontype,int editbookingid=0);
    }


    /*Car Booking Service
     * 
     */
    public interface IBookingService
    {
        bool TransactionNow(Func<bool> doIt, string label = null);

        bool IgnoreQuestion(int id, string userid);
        bool DeleteBooking(int[] ids, string userid);
        bool SaveBooking(IBkdBookingEditModel model,string userid);
        IBkdBookingEditModel GetBooking(int id, string userid);
        IEnumerable<IEventModel> GetEvents(bool selfonly,string userid, string fromdate, string todate,Dictionary<string,string> colors);
        IEnumerable<IviewBkdbooking> GetBookings(bool selfonly,string userid, string fromdate, string todate, string search, string type,string colid=nameof(IviewBkdbooking.ReservedStartAt),string sort="asc");
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
        bool TransactionNow(Func<bool> doIt, string label = null);
        string ImportDataTable(DataTable dt, string updateby, out int count);

        IEnumerable<KeyValuePair<int, string>> GetLevels(params int[] exclude);

        bool ChangeUserPassword(string userid, string password, string editedby, string oldpassword = null);
        
        object EditUser(string userid, string userName, string person, int level, string post, string division, bool isAdmin, string editedby, string adminScope=null, string email = null, string password = null, int Id = 0);
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

    public interface IEditDriverModel
    {
        int Id { get; set; }

        int ReplacementId { get; set; }

        int DeputyId { get; set; }
        string DriverName { get; set; }
        string DriverMobile { get; set; }
        int Priority { get; set; }
        Nullable<System.DateTime> updatedAt { get; set; }
        string updatedBy { get; set; }
        string DriverTag { get; set; }
        string PlateNo { get; set; }
        System.DateTime ValidFrom { get; set; }

        string Preference { get; set; }

        string NullaPreference { get; }


        bool Adhoc { get; set; }
        bool Disabled { get; set; }
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

    public interface IBkdBookingComplianceModel
    {
        int MaxPeriod { get; set; }

        DateTime SessionDate { get; set; }
        DateTime SessionStart { get; set; }
        DateTime SessionEnd { get; set; }
    }

    public interface IBkdBookingEditModel
    {
        int Id { get; set; }
        bool VIP { get; set; }
        System.DateTime ReservedStartAt { get; set; }
        System.DateTime ReservedEndAt { get; set; }
        string SessionType { get; set; }
        string UserName { get; set; }
        string UserPost { get; set; }
        string DriverName { get; set; }

        string DriverMobile { get; set; }
        string CarPlateNo { get; set; }
        string PickupLocation { get; set; }
        string DropLocation { get; set; }
        string Remarks { get; set; }
        Nullable<System.DateTime> updatedAt { get; set; }
        string updatedBy { get; set; }
        bool Invalidated { get; set; }
        int DrivingDutyId { get; set; }

        bool TripReturn { get; set; }

        string ReturnYesNo { get; set; }


    }
    public interface IEditSettingModel
    {
        string SettingId { get; set; }
        string SettingValue { get; set; }
        bool CanEdit { get; set; }
    }
    public interface IviewBkdholiday
    {
        int HolidayFullId { get; set; }
        string NameOnHoliday { get; set; }
        string NameOnDuty { get; set; }
        string MobileOnHoliday { get; set; }
        string MobileOnDuty { get; set; }
        string HolidayPlateNo { get; set; }
        string DutyPlateNo { get; set; }
        Nullable<int> DutyFullId { get; set; }
        string BookingUserName { get; set; }
        string BookingUserPost { get; set; }
        Nullable<System.DateTime> BookingDate { get; set; }

        DateTime DutyDate { get; set; }

        int DutyDriverId { get; set; }

        int BookingId { get; set; }

        int DutySheetId { get; set; }


        string Remarks { get; set; }
        bool FallIntoAdhoc { get; set; }

    }

    public interface IviewBkdDriver
    {
        int Id { get; set; }
        int Tag { get; set; }
        int ReplacementId { get; set; }
        bool IsTemporary { get; set; }
        Nullable<System.DateTime> SubstitutedOn { get; set; }
        Nullable<System.DateTime> ServicingFrom { get; set; }
        bool IsCurrent { get; set; }
        string DriverMobile { get; set; }
        string DriverName { get; set; }
        string PlateNo { get; set; }
        string Preference { get; set; }
        string NullaPreference { get; set; }
        Nullable<System.DateTime> updatedAt { get; set; }
        string updatedBy { get; set; }
        bool Adhoc { get; set; }
        System.DateTime ValidFrom { get; set; }

        int Priority { get; set; }
    }

    public interface IviewBkdbooking
    {
        int Id { get; set; }
        bool VIP { get; set; }
        System.DateTime ReservedStartAt { get; set; }
        System.DateTime ReservedEndAt { get; set; }
        string SessionType { get; set; }
        string UserName { get; set; }
        string UserPost { get; set; }
        string DriverName { get; set; }
        string DriverMobile { get; set; }
        string CarPlateNo { get; set; }
        string PickupLocation { get; set; }
        string DropLocation { get; set; }
        string Remarks { get; set; }
        System.DateTime updatedAt { get; set; }
        string updatedBy { get; set; }

        string ReservedDate { get; set; }
        string FromTime { get; set; }
        string TillTime { get; set; }
        string IsVIP { get; set; }

        bool TripReturn { get; set; }

        bool InQuestion { get; set; }

        string InQuestionStr { get; set; }


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
