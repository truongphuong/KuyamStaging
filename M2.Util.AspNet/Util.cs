using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace M2.Util.AspNet
{
	public class Util
	{
		public static void RestartApp(string webConfigPath)
		{
			FileHelper.SetLastWriteTime(webConfigPath);
		}
	}
}
