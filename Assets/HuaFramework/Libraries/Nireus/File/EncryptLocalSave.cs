using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Nireus
{
    public static class EncryptLocalSave
    {
        private static string sKey = "Avdk12NmNDE2NT42MWM3f4F5NTE18GDE";
        private static string sIv = "32ZymTY76/GR4446qYn5jA==";
        public static void SetInt(string key, int val)
        {
            PlayerPrefs.SetString(GetHash(key), Encrypt(val.ToString()));
        }
        public static int GetInt(string key, int default_value = 0)
        {
            string value_str = GetString(key, default_value.ToString());
            int result = default_value;
            int.TryParse(value_str, out result);
            return result;
        }
        public static void SetFloat(string key, float val)
        {
            PlayerPrefs.SetString(GetHash(key), Encrypt(val.ToString()));
        }
        public static float GetFloat(string key, float default_value = 0f)
        {
            string value_str = GetString(key, default_value.ToString());
            float result = default_value;
            float.TryParse(value_str, out result);
            return result;
        }
        public static void SetString(string key, string val)
        {
            PlayerPrefs.SetString(GetHash(key), Encrypt(val));
        }
        public static string GetString(string key, string default_value = "")
        {
            string text = default_value;
            string value_str = PlayerPrefs.GetString(GetHash(key), default_value.ToString());
            if (!text.Equals(value_str))
            {
                text = Decrypt(value_str);
            }
            return text;
        }
        public static bool HasKey(string key)
        {
            string hash = GetHash(key);
            return PlayerPrefs.HasKey(hash);
        }
        public static void DeleteKey(string key)
        {
            string hash = GetHash(key);
            PlayerPrefs.DeleteKey(hash);
        }
        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }
        public static void Save()
        {
            PlayerPrefs.Save();
        }
        private static string Decrypt(string value_str)
        {
            RijndaelManaged rijndaelManaged = new RijndaelManaged
            {
                Padding = PaddingMode.Zeros,
                Mode = CipherMode.CBC,
                KeySize = 128,
                BlockSize = 128
            };
            byte[] bytes = Encoding.UTF8.GetBytes(sKey);
            byte[] rgbIV = Convert.FromBase64String(sIv);
            ICryptoTransform transform = rijndaelManaged.CreateDecryptor(bytes, rgbIV);
            byte[] array = Convert.FromBase64String(value_str);
            byte[] array2 = new byte[array.Length];
            MemoryStream stream = new MemoryStream(array);
            CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read);
            cryptoStream.Read(array2, 0, array2.Length);
            return Encoding.UTF8.GetString(array2).TrimEnd(new char[1]);
        }
        private static string Encrypt(string encrypt_str)
        {
            RijndaelManaged rijndaelManaged = new RijndaelManaged
            {
                Padding = PaddingMode.Zeros,
                Mode = CipherMode.CBC,
                KeySize = 128,
                BlockSize = 128
            };
            byte[] bytes = Encoding.UTF8.GetBytes(sKey);
            byte[] rgbIV = Convert.FromBase64String(sIv);
            ICryptoTransform transform = rijndaelManaged.CreateEncryptor(bytes, rgbIV);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
            byte[] bytes2 = Encoding.UTF8.GetBytes(encrypt_str);
            cryptoStream.Write(bytes2, 0, bytes2.Length);
            cryptoStream.FlushFinalBlock();
            byte[] inArray = memoryStream.ToArray();
            return Convert.ToBase64String(inArray);
        }
        private static string GetHash(string key)
        {
            MD5 mD = new MD5CryptoServiceProvider();
            byte[] array = mD.ComputeHash(Encoding.UTF8.GetBytes(key));
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                stringBuilder.Append(array[i].ToString("x2"));
            }
            return stringBuilder.ToString();
        }
    }
}


