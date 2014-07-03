﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

namespace Kuyam.Domain
{
    public class SecurityHelper
    {       
        public static string CreateSaltKey(int size) 
        {
            // Generate a cryptographic random number
            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[size];
            rng.GetBytes(buff);

            // Return a Base64 string representation of the random number
            return Convert.ToBase64String(buff);
        }

        public static string CreatePasswordHash(string password, string saltkey, string passwordFormat = "SHA1")
        {
            if (String.IsNullOrEmpty(passwordFormat))
                passwordFormat = "SHA1";
            string saltAndPassword = String.Concat(password, saltkey);
            string hashedPassword =
                FormsAuthentication.HashPasswordForStoringInConfigFile(saltAndPassword, passwordFormat);
            return hashedPassword;
        }
        
        public static string EncryptText(string plainText, string encryptionPrivateKey = "") 
        {
            if (string.IsNullOrEmpty(plainText)||String.IsNullOrEmpty(encryptionPrivateKey))
                return plainText;

            //if (String.IsNullOrEmpty(encryptionPrivateKey))
            //    encryptionPrivateKey = _securitySettings.EncryptionKey;

            var tDESalg = new TripleDESCryptoServiceProvider();
            tDESalg.Key = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(0, 16));
            tDESalg.IV = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(8, 8));

            byte[] encryptedBinary = EncryptTextToMemory(plainText, tDESalg.Key, tDESalg.IV);
            return Convert.ToBase64String(encryptedBinary);
        }

        public static string DecryptText(string cipherText, string encryptionPrivateKey = "") 
        {
            if (String.IsNullOrEmpty(cipherText) || String.IsNullOrEmpty(encryptionPrivateKey))
                return cipherText;

            //if (String.IsNullOrEmpty(encryptionPrivateKey))
            //    encryptionPrivateKey = _securitySettings.EncryptionKey;

            var tDESalg = new TripleDESCryptoServiceProvider();
            tDESalg.Key = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(0, 16));
            tDESalg.IV = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(8, 8));

            byte[] buffer = Convert.FromBase64String(cipherText);
            return DecryptTextFromMemory(buffer, tDESalg.Key, tDESalg.IV);
        }

        #region Utilities

        private static byte[] EncryptTextToMemory(string data, byte[] key, byte[] iv) 
        {
            using (var ms = new MemoryStream()) {
                using (var cs = new CryptoStream(ms, new TripleDESCryptoServiceProvider().CreateEncryptor(key, iv), CryptoStreamMode.Write)) {
                    byte[] toEncrypt = new UnicodeEncoding().GetBytes(data);
                    cs.Write(toEncrypt, 0, toEncrypt.Length);
                    cs.FlushFinalBlock();
                }

                return ms.ToArray();
            }
        }

        private static string DecryptTextFromMemory(byte[] data, byte[] key, byte[] iv) 
        {
            using (var ms = new MemoryStream(data)) {
                using (var cs = new CryptoStream(ms, new TripleDESCryptoServiceProvider().CreateDecryptor(key, iv), CryptoStreamMode.Read))
                {
                    var sr = new StreamReader(cs, new UnicodeEncoding());
                    return sr.ReadLine();
                }
            }
        }

        #endregion
    }
}
