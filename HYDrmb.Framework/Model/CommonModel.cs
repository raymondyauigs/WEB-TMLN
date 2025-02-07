using HYDrmb.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYDrmb.Framework.Model
{
	
	public class ThumbModel
	{
		public string type { get; set; }
		public string text { get; set; }
		public string title { get; set; }
		public string link { get; set; }
		public string src { get; set; }
	}
	public class ResumableConfiguration
	{
		/// <summary>
		/// Gets or sets number of expected chunks in this upload.
		/// </summary>
		public int Chunks { get; set; }

		/// <summary>
		/// Gets or sets unique identifier for current upload.
		/// </summary>
		public string Identifier { get; set; }

		public string Upload_Token { get; set; }

		/// <summary>
		/// Gets or sets file name.
		/// </summary>
		public string FileName { get; set; }

		public ResumableConfiguration()
		{

		}

		/// <summary>
		/// Creates an object with file upload configuration.
		/// </summary>
		/// <param name="identifier">Upload unique identifier.</param>
		/// <param name="filename">File name.</param>
		/// <param name="chunks">Number of file chunks.</param>
		/// <returns>File upload configuration.</returns>
		public static ResumableConfiguration Create(string identifier, string filename, int chunks, string uploadtoken)
		{
			return new ResumableConfiguration { Identifier = identifier, FileName = filename, Chunks = chunks, Upload_Token = uploadtoken };
		}
	}
	public class JSort
    {
        public string Column { get; set; }
        public string Order { get; set; }
    }


    public class JsonSearchClass : IBelongtoTable
    {
        // table name
        public String tablename { get; set; }
        // table column name
        public String key { get; set; }
        // table column value
        public String value { get; set; }
        // table column datatype
        public String datatype { get; set; }
        // table column operator
        public String op { get; set; }
        // group for multi-select checklist
        public String group { get; set; }
    }
}
