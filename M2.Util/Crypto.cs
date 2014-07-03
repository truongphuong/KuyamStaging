// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace M2.Util
{
    public class Crypto
    {
        public bool Active { get; set; }
        public string Key { get; set; }

        public string Encrypt(string dec)
        {
            if (!Active || dec == null || dec == string.Empty)
                return dec;
            else
                return EncryptData(dec, Key, Active);
        }

        public string Decrypt(string enc)
        {
            if (!Active || enc == null || enc == string.Empty)
                return enc;
            else
                return DecryptData(enc, Key, Active);
        }

        // AES functions below from:
        // http://www.topxml.com/rbnews/XmlSerializer/re-24282_Simple-string-byte-encryption-and-decryption-using-AES-in-C.aspx

        /// <summary>
        /// Use AES to encrypt data string. The output string is the encrypted bytes as a base64 string.
        /// The same key must be used to decrypt the string.
        /// </summary>
        /// <param name="data">Clear string to encrypt.</param>
        /// <param name="key">Key used to encrypt the string.</param>
        /// <returns>Encrypted result as Base64 string.</returns>
        public static string EncryptData(string data, string key, bool isActive = true)
        {
            if (!isActive)
                return data;

            if (data == null)
                throw new ArgumentNullException("data");

            if (key == null || key == "")
                return data;
            //	throw new ArgumentNullException("key");

            byte[] encBytes = EncryptData(Encoding.UTF8.GetBytes(data), key, PaddingMode.ISO10126);
            return Convert.ToBase64String(encBytes);
        }

        /// <summary>
        /// Decrypt the data string to the original string.  The data must be the base64 string
        /// returned from the EncryptData method.
        /// </summary>
        /// <param name="data">Encrypted data generated from EncryptData method.</param>
        /// <param name="key">Key used to decrypt the string.</param>
        /// <returns>Decrypted string.</returns>
        public static string DecryptData(string data, string key, bool isActive = true)
        {
            if (!isActive)
                return data;

            if (data == null)
                throw new ArgumentNullException("data");

            if (key == null || key == "")
                return data;
            //	throw new ArgumentNullException("key");

            byte[] encBytes = Convert.FromBase64String(data);
            byte[] decBytes = DecryptData(encBytes, key, PaddingMode.ISO10126);

            return Encoding.UTF8.GetString(decBytes);
        }

        public static byte[] EncryptData(byte[] data, string key, PaddingMode paddingMode, bool isActive = true)
        {
            if (!isActive)
                return data;

            if (data == null || data.Length == 0)
                throw new ArgumentNullException("data");

            if (key == null || key == "")
                return data;
            //	throw new ArgumentNullException("key");

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(key, Encoding.UTF8.GetBytes("Salt"));
            RijndaelManaged rm = new RijndaelManaged();
            rm.Padding = paddingMode;
            ICryptoTransform encryptor = rm.CreateEncryptor(pdb.GetBytes(16), pdb.GetBytes(16));
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream encStream = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    encStream.Write(data, 0, data.Length);
                    encStream.FlushFinalBlock();
                    return msEncrypt.ToArray();
                }
            }
        }

        public static byte[] DecryptData(byte[] data, string key, PaddingMode paddingMode, bool isActive = true)
        {
            if (!isActive)
                return data;

            if (data == null || data.Length == 0)
                throw new ArgumentNullException("data");

            if (key == null || key == "")
                return data;
            //	throw new ArgumentNullException("key");

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(key, Encoding.UTF8.GetBytes("Salt"));
            RijndaelManaged rm = new RijndaelManaged();
            rm.Padding = paddingMode;
            ICryptoTransform decryptor = rm.CreateDecryptor(pdb.GetBytes(16), pdb.GetBytes(16));
            using (MemoryStream msDecrypt = new MemoryStream(data))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    // Decrypted bytes will always be less then encrypted bytes, so len of encrypted data will be big enouph for buffer.
                    byte[] fromEncrypt = new byte[data.Length];                // Read as many bytes as possible.
                    int read = csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);
                    if (read < fromEncrypt.Length)
                    {
                        // Return a byte array of proper size.
                        byte[] clearBytes = new byte[read];
                        Buffer.BlockCopy(fromEncrypt, 0, clearBytes, 0, read);
                        return clearBytes;
                    }
                    return fromEncrypt;
                }
            }
        }
    }
}
