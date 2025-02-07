using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYDrmb.Abstraction
{
    public enum SessionType
    {
        AM,
        PM,
        FULL,
        CUSTOM
    }
    [Flags]
    public enum PreferenceType
    {
        None = 0,
        AM=1,
        PM=2,
        FULL=3,

    }
    public enum LogType
    {
        Error,
        Information,
        Warning,
        Fatal,
        Debug
    }
    public enum LogCategory
    {
        NotSpecify,
        AccessLog,
        ErrorLog,
        EventLog,
        MiscLog,
        EnquiryLog
    }
    

    public enum AutoWantedType
    {
        IsVIP,
        SessionType,
        UserName,
        UserPost,
        DriverName,
        CarPlateNo,
        PickupLocation,
        DropLocation,
        FromTime,
        TillTime,
        InQuestion

    }


    public static class Constants
    {
        public static readonly int[] Endorsers = new[] { 5, 8, 9 };
        public static readonly int[] Drafters = new[] { 18, 8 };
        public static readonly int[] Auditors = new[] { 6, 5 };

        public const string breakstyle = "page-break-before:always; mso-break-type:section-break";
        public const string log4netForBatch = "batchlognetfile";
        public const string log4netForAdmin = "adminlognetfile";

        public const string engSUList = "engSUList";
        public const string engSTList = "engSTList";

        public const string statStart = "statStart";

        public static class ExcelPart
        {

            // 0 - 32 : Main
            // 33 - 40 : verify
            // 41 - 49 : Exit

            public static string[] coldescs = new[]{"Structure No.",
            "Taken over date",
            "Parent / Child",
            "Type of Structure (HSIS)",
            "Child Structure(s)",
            "Parent Structure",
            "Adjoining Structures",
            "Location",
            "Independent Staircase",
            "District Council (1)",
            "District Council (2)",
            "Highways Region",
            "No. of Exits",
            "Provision of Full BFA Facilities",
            "Walkway Width >= 2m",
            "Gradient <=10%",
            "Steps on Walkway",
            "Walkway BFA facility for Steps",
            "Walkway Alternative BFA means",
            "Walkway Alternative BFA detail",
            "Walkway Alternative Distance (m)",
            "BFA at Main Walkway",
            "Walkway Remarks",
            "Status of Record",
            "Prepared / Updated by",
            "Prepared / Updated by Post",
            "Prepared / Updated Date",
            "Endorsed by",
            "Endorsed by Post",
            "Endorsed Date",
            "Audited by",
            "Audited by Post",
            "Audited Date",

            "Walkway already investigated in ORP / UAP",
            "ORP / UAP Current Status",
            "ORP / UAP Remarks",
            "Investigation Agreement No.",
            "D&C Agreement No.",
            "Walkway itself with BFA but connecting to an adjoining structure without BFA",
            "B&S checked",
            "Region to follow up",

            "Exit {0} connection to Building",
            "Exit {0} BFA at Exit",
            "Exit {0} reason for no BFA",
            "Exit {0} BFA facility",
            "Exit {0} alternative BFA means",
            "Exit {0} alternative BFA detail",
            "Exit {0} alternative distance",
            "Exit {0} Remarks",
            "BFA at Exit {0}",

            "Exit A connection to Building",
            "Exit A BFA at Exit",
            "Exit A reason for no BFA",
            "Exit A BFA facility",
            "Exit A alternative BFA means",
            "Exit A alternative BFA detail",
            "Exit A alternative distance",
            "Exit A Remarks",
            "BFA at Exit A",
            "Exit B connection to Building",
            "Exit B BFA at Exit",
            "Exit B reason for no BFA",
            "Exit B BFA facility",
            "Exit B alternative BFA means",
            "Exit B alternative BFA detail",
            "Exit B alternative distance",
            "Exit B Remarks",
            "BFA at Exit B",
            "Exit C connection to Building",
            "Exit C BFA at Exit",
            "Exit C reason for no BFA",
            "Exit C BFA facility",
            "Exit C alternative BFA means",
            "Exit C alternative BFA detail",
            "Exit C alternative distance",
            "Exit C Remarks",
            "BFA at Exit C",
            "Exit D connection to Building",
            "Exit D BFA at Exit",
            "Exit D reason for no BFA",
            "Exit D BFA facility",
            "Exit D alternative BFA means",
            "Exit D alternative BFA detail",
            "Exit D alternative distance",
            "Exit D Remarks",
            "BFA at Exit D",
            "Exit E connection to Building",
            "Exit E BFA at Exit",
            "Exit E reason for no BFA",
            "Exit E BFA facility",
            "Exit E alternative BFA means",
            "Exit E alternative BFA detail",
            "Exit E alternative distance",
            "Exit E Remarks",
            "BFA at Exit E",
            "Exit F connection to Building",
            "Exit F BFA at Exit",
            "Exit F reason for no BFA",
            "Exit F BFA facility",
            "Exit F alternative BFA means",
            "Exit F alternative BFA detail",
            "Exit F alternative distance",
            "Exit F Remarks",
            "BFA at Exit F",
            "Exit G connection to Building",
            "Exit G BFA at Exit",
            "Exit G reason for no BFA",
            "Exit G BFA facility",
            "Exit G alternative BFA means",
            "Exit G alternative BFA detail",
            "Exit G alternative distance",
            "Exit G Remarks",
            "BFA at Exit G",
            "Exit H connection to Building",
            "Exit H BFA at Exit",
            "Exit H reason for no BFA",
            "Exit H BFA facility",
            "Exit H alternative BFA means",
            "Exit H alternative BFA detail",
            "Exit H alternative distance",
            "Exit H Remarks",
            "BFA at Exit H",
            "Exit J connection to Building",
            "Exit J BFA at Exit",
            "Exit J reason for no BFA",
            "Exit J BFA facility",
            "Exit J alternative BFA means",
            "Exit J alternative BFA detail",
            "Exit J alternative distance",
            "Exit J Remarks",
            "BFA at Exit J",
            "Exit K connection to Building",
            "Exit K BFA at Exit",
            "Exit K reason for no BFA",
            "Exit K BFA facility",
            "Exit K alternative BFA means",
            "Exit K alternative BFA detail",
            "Exit K alternative distance",
            "Exit K Remarks",
            "BFA at Exit K",

            "BFA Not OK Solely due to Inadequate Walkway Width",
            "Strucutre BFA OK but Connected to non-BFA Footpath",
            "B&S note 1",
            "B&S note 2",
            "B&S note 3"

            };

            public static string[] colkeys = new[] {"StctNo",
            "TakeDate",
            "InitType",
            "HSISType",
            "ChildType",
            "ParentType",
            "StctAdjoin",
            "Location",
            "StairIndp",
            "Council1",
            "Council2",
            "InRegion",
            "ExitCnt",
            "FacilityProvision",
            "WideEnough",
            "Gradlt10",
            "WithSteps",
            "StepsForBFA",
            "AlterMeans",
            "AlterDetail",
            "AlterDist",
            "AtMainForBFA",
            "Remarks",
            "Status",
            "UpdatedBy",
            "UpdatedByPost",
            "UpdatedAt",
            "EndorsedBy",
            "EndorsedByPost",
            "EndorsedAt",
            "AuditedBy",
            "AuditedByPost",
            "AuditedAt",


            "InvgORPorUAP",
            "InvgStatusORPorUAP",
            "InvgRemarksORPorUAP",
            "InvgAgreeNo",
            "DCInAgreeNo",
            "ConnectAdjoin",
            "BSInChecked",
            "FollowupRegion",

            "ExitToBuild",
            "AtExitFor",
            "ReasonwNoBFA",
            "DescwBFA",
            "AlterMeanswBFA",
            "AlterDetailwBFA",
            "AlterDistOnExit",
            "RemarksOnExit",
            "ExitwBFA",



            "ExitAToBuild",
            "AtExitForA",
            "ReasonAwNoBFA",
            "DescAwBFA",
            "AlterMeansAwBFA",
            "AlterDetailAwBFA",
            "AlterDistA",
            "RemarksA",
            "ExitAwBFA",
            "ExitBToBuild",
            "AtExitForB",
            "ReasonBwNoBFA",
            "DescBwBFA",
            "AlterMeansBwBFA",
            "AlterDetailBwBFA",
            "AlterDistB",
            "RemarksB",
            "ExitBwBFA",
            "ExitCToBuild",
            "AtExitForC",
            "ReasonCwNoBFA",
            "DescCwBFA",
            "AlterMeansCwBFA",
            "AlterDetailCwBFA",
            "AlterDistC",
            "RemarksC",
            "ExitCwBFA",
            "ExitDToBuild",
            "AtExitForD",
            "ReasonDwNoBFA",
            "DescDwBFA",
            "AlterMeansDwBFA",
            "AlterDetailDwBFA",
            "AlterDistD",
            "RemarksD",
            "ExitDwBFA",
            "ExitEToBuild",
            "AtExitForE",
            "ReasonEwNoBFA",
            "DescEwBFA",
            "AlterMeansEwBFA",
            "AlterDetailEwBFA",
            "AlterDistE",
            "RemarksE",
            "ExitEwBFA",
            "ExitFToBuild",
            "AtExitForF",
            "ReasonFwNoBFA",
            "DescFwBFA",
            "AlterMeansFwBFA",
            "AlterDetailFwBFA",
            "AlterDistF",
            "RemarksF",
            "ExitFwBFA",
            "ExitGToBuild",
            "AtExitForG",
            "ReasonGwNoBFA",
            "DescGwBFA",
            "AlterMeansGwBFA",
            "AlterDetailGwBFA",
            "AlterDistG",
            "RemarksG",
            "ExitGwBFA",
            "ExitHToBuild",
            "AtExitForH",
            "ReasonHwNoBFA",
            "DescHwBFA",
            "AlterMeansHwBFA",
            "AlterDetailHwBFA",
            "AlterDistH",
            "RemarksH",
            "ExitHwBFA",
            "ExitJToBuild",
            "AtExitForJ",
            "ReasonJwNoBFA",
            "DescJwBFA",
            "AlterMeansJwBFA",
            "AlterDetailJwBFA",
            "AlterDistJ",
            "RemarksJ",
            "ExitJwBFA",
            "ExitKToBuild",
            "AtExitForK",
            "ReasonKwNoBFA",
            "DescKwBFA",
            "AlterMeansKwBFA",
            "AlterDetailKwBFA",
            "AlterDistK",
            "RemarksK",
            "ExitKwBFA" ,

            "WidthNotAcpt",
            "ToNonBFAPath",
            "BSNote1",
            "BSNote2",
            "BSNote3",
            };
        }


        public static class CmdKey
        {
            public const string UnlockKey = "macroname";
            public const string XslmKey = "excelname";
            public const string ParamfileKey = "paramfile";
            public const string TargetfileKey = "targetFile";
        }

        public static class Setting
        {
            public const string fromCal = nameof(fromCal);

            public const string ReturnUrl = "ReturnUrl";

            public const string AuthorizeCookieKey = "HYD.AuthorizeCookie_Key";

            public const string dbconn = "dbconn";
            public const string batchmode = "mode";
            public const string logpath = "lognetsite";
            public const string loghydpath = "hydadminsite";
            public const string logasgpath = "hydassgnsite";
            public const string logpdspath = "HYDrmbsite";
            public const string rdxpath = "rdxpath";
            public const string bakpath = "bakpath";
            public const string cmdpath = "cmdpath";
            public const string browsepath = "browsepath";
            public const string streampath = "streampath";


            public const string templateexcel = "templateexcel";
            public const string templatepdf = "templatepdf";

            public const string filenamelimit = "filenamelimit";

            public const string scheduletime = "schedulecron";
            public const string passthrough = "passthrough";
            public const string testfile = "testfile";


            public const string genpdfpath = "genpdfpath";
            public const string genexcelpath = "genexcelpath";
            public const string genwordpath = "genwordpath";            
            public const string uploadpath = "uploadpath";
            public const string sharepath = "sharepath";
            public const string exportoutputpath = "exportoutputpath";
            
            public const string docspath = "docspath";


            public const string exportfile = "exportfile";

            public const string logfile = "logfile";

            public const string pndreply = "pndreply";
            public const string DefaultPwd = nameof(DefaultPwd);
            public const string backcolorAM = nameof(backcolorAM);
            public const string backcolorPM = nameof(backcolorPM);
            public const string backcolorFULL = nameof(backcolorFULL);
            public const string backcolorCUSTOM = nameof(backcolorCUSTOM);


            //for analysis script


        }
        public static class Session
        {
            public const string TagId = "$tagid$";
            public const string UserId = "$userid$";
            public const string UserName = "$username$";
            public const string UserLevel = "$userlevel$";
            public const string IsAdmin = "$isadmin$";
            public const string Division = "$division$";
            public const string IsBSGuy = "$isbsguy$";
            public const string UserEmail = "$useremail$";
            public const string FullMenu = "$fullmenu$";
            public const string Message = "$message$";
            public const string MessageCount= "$msgcount$";


            public const string UserInfoDisplay = "$userdisplay$";
            public const string SESSION_CACHINGSERVICE = "cachingservice";
            public const string SESSION_KEY = "HYDrmb.sessions.";
            public const string UPLOAD_TOKEN = "$uploadtoken$";
            public const string EXCELEXPORT = ".excelexport";
            public const string SELFONLY = ".selfonly";
            public static string SESSION_EXCELEXPORT = $"{SESSION_CACHINGSERVICE}{EXCELEXPORT}";            
            public static string SESSION_SELFONLY = $"{SESSION_CACHINGSERVICE}{SELFONLY}";
        }

        public static class DT
        {
            public const string YESNA = "Yes / N.A.";
            public const string YESEXIT = "Yes, but exit solely connects to at-grade non-BFA footpath or staircase";
            public const string UDATE = "Updated Date";
            public const string EDATE = "Endorsed Date";
            public const string ADATE = "Audited Date";
            public const string YES = "Yes";
            public const string NO = "No";

            public const string WHOLE = "Full Day";
            public const string CARMAX = "No. of Cars allowed";
            

        }
        public static class DBkey
        {
            public const string dbmeta = nameof(dbmeta);
            public const string dbprovider = nameof(dbprovider);
            public const string dbuser = nameof(dbuser);
            public const string dbpwd = nameof(dbpwd);
            public const string dbcatalog = nameof(dbcatalog);
            public const string dbsource = nameof(dbsource);

        }

        public static class UI
        {
            public const string NotUsedItem = "*(Not Used)";
            public const string AllItems = "(All {0})";
            public const string ERROR_MESSAGE = "__Error_Message__";



            public const string NAME_DFA_ADMINS = "Administrator";

            
            public const string STTS_BKD_INVAD = "Invalidated";
            public const string STTS_BKD_CNCEL = "Cancelled";
            public const string STTS_BKD_RESRV = "Reserved";


            public const string DIVISION_BS = "B&S";
            public const string DIVISION_NT = "NT";
            public const string DIVISION_UB = "Urban";
            public const string DIVISION_TM = "TMCA/TSCA";

            public const string NAME_DRIVERP0 = "Driver 0 (VIP)";
            public const string NAME_DRIVERP1 = "Driver 1 (Normal)";



            public const string NAME_ADMINISTRATOR = "Admin User";
            //book within month
            public const string NAME_POWERUSER = "Power User";

            public const string NAME_VIPUSER = "VIP User";
            //book within 7 days
            public const string NAME_NORMALUSER = "Normal User";

            public const string SETT_PREFERENCE = "PreferenceType";
            public const string SETT_LOCATION = "LocationType";
            public const string SETT_DRIVINGS = "DrivingType";
            public const string SETT_UPOSTTYPE = "UserPostType";
            public const string SETT_SESSNTYPE = "SessionType";
            public const string SETT_YESNO = "YNType";
            public const string SETT_NAYNO = "NAYNOType";
            public const string SETT_IREGION = "InRegionType";
            public const string SETT_DATTYPE = "DateType";
            public const string SETT_DCSTYPE = "DocsType";

            public const string SETT_CARSAVAIL = "CarsAvailable";


            public const string INIT_PARENT = "Parent";
            public const string INIT_Child = "Child";

            public const string SETT_USRSET = "UserReset";

            public const string SETT_IMGSET = "ImageReset";

            public const string SETT_VPMCNT = "VIPMaxCount";

            public const string MAIN_COND_YES = "Yes";
            public const string MAIN_COND_NO = "No";

            

        }
    }
}
