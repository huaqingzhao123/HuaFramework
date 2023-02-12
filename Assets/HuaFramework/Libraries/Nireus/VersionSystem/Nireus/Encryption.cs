//#define USE_ENCRYPTION

using UnityEngine;
using System.Text;
using System.Collections;
using System.Security.Cryptography;

namespace Nireus
{
    public class Encryption
    {
        static readonly byte[] XOR_KEY = Encoding.ASCII.GetBytes("(*6)s9@##&%*(*6)s92asd09&KsmadI@amsldkfj@IUASL%kd5j^iaoOI@Asjdasmnfjaks%dhHUIaos");

        public static byte[] xor(byte[] encrypt_bytes, byte[] encrypt_key)
        {
#if USE_ENCRYPTION
			if (encrypt_bytes.Length <= 0) throw new System.Exception("No Encrypt string");
			if (encrypt_key.Length <= 0) throw new System.Exception("No Encrypt key");

			int length = encrypt_bytes.Length;
			int key_length = encrypt_key.Length;

			for (int i = 0; i < length; i++)
			{
				encrypt_bytes[i] = (byte)(encrypt_bytes[i] ^ encrypt_key[i % key_length]);
			}
#endif
            return encrypt_bytes;
        }

        public static byte[] xor(byte[] encrypt_bytes)
        {
            return xor(encrypt_bytes, XOR_KEY);
        }

        public static string md5(string encrypt_string)
        {
#if USE_ENCRYPTION
            if (string.IsNullOrEmpty(encrypt_string)) throw new System.Exception("encrypt_string is null.");
			return md5(Encoding.Default.GetBytes(encrypt_string));
#else
            return encrypt_string;
#endif
        }

        public static string md5(byte[] encrypte_data)
        {
            string md5_str = string.Empty;
            if (encrypte_data == null) throw new System.Exception("encrypt_string is null.");
            MD5 md5_class = new MD5CryptoServiceProvider();
            md5_str = System.BitConverter.ToString(md5_class.ComputeHash(encrypte_data)).Replace("-", "");
            return md5_str;
        }
    }
}
