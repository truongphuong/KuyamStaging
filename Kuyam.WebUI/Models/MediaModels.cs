using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kuyam.Database;
using M2.Util;

namespace Kuyam.WebUI.Models
{
	public class MediaDisplayModel : IModel
	{
		public int MediaID { get; set; }
		public int Height { get; set; }
		public int Width { get; set; }

        public Medium Media { get; set; }
		public Dictionary<string, string> Data { get; set; }
		
		/// <summary>
		/// Base constructor requires class variables be set manually and LockAndLoad() called
		/// </summary>
		public MediaDisplayModel()
		{
			Data = new Dictionary<string, string>();
		}

		/// <summary>
		/// Parameterized constructor will call LockAndLoad()
		/// </summary>
		/// <param name="mediaID">ID of the media defined in the Media table</param>
		/// <param name="height">in pixels</param>
		/// <param name="width">in pixels</param>
		public MediaDisplayModel(int mediaID, int height, int width)
		{
			MediaID = mediaID;
			Height = height;
			Width = width;
			Data = new Dictionary<string, string>();
			LockAndLoad();
		}
	
		public void LockAndLoad()
		{
			if (MediaID <= 0)
				throw new Exception("MediaID not set in MediaDisplayModel");

			Media = Kuyam.Database.DAL.GetMedia(MediaID);

			Data = Media.LocationData.ParseQueryString();

		}
	}
}