using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
namespace gds_services.Utils
{
    public class Key_Generator
    {
        public static string get_MD5_hash(string text)
        {                        
            String md5Result;
            StringBuilder sb = new StringBuilder();
            MD5 md5Hasher = MD5.Create();
            byte[] byte_text = Encoding.UTF8.GetBytes(text);
            foreach (Byte b in md5Hasher.ComputeHash(byte_text))
            {
                sb.Append(b.ToString("x2").ToLower());
            }            
            md5Result = sb.ToString();
            return md5Result;
        }
    }
}