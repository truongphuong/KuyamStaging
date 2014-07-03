using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using M2.Util;

namespace Kuyam.Database
{
	public static class CryptoHelper
	{
		private static Crypto _crypto = new Crypto();

		static CryptoHelper()
		{
			//_crypto.Active = true;
			//_crypto.Key = "FDDBA79A-319E-4057-8E3E-104F659C1BD2";
		}

		#region Crypto
		public static string Encrypt(string dec)
		{
			if (dec == null)
				return null;

			return _crypto.Encrypt(dec);
		}

		public static string Decrypt(string enc)
		{
			if (enc == null)
				return null;

			if (!enc.EndsWith("="))
			{
				//LogError("Customer data appears to be unencrypted: " + enc);
				return enc;
			}

			return _crypto.Decrypt(enc);
		}

		#endregion
	}
}
