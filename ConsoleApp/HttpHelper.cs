using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Spider
{
    /// <summary>
    /// http帮助类
    /// </summary>
    public class HttpHelper
    {
        //static HttpHelper()
        //{
        //    ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
        //}

        //private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        //{
        //    return true;
        //}
        public static HttpWebRequest CreateRequest(string url, RequestMethod method, CookieContainer cookieContainer)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ProtocolVersion = HttpVersion.Version11;
            request.Method = method.ToString();
            request.KeepAlive = true;
            request.CookieContainer = cookieContainer ?? new CookieContainer();
            request.UserAgent = Configs.ChromeAgent;
            request.Headers["Accept-Language"] = "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4";
            request.Headers["Accept-Encoding"] = "gzip, deflate, sdch";
            request.Accept = @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            return request;
        }

        /// <summary>
        /// 获取请求返回的文本数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetWebResponseData(HttpWebRequest request, Encoding encode = null)
        {
            HttpWebResponse response = null;

            Encoding encodeTemp = encode ?? Encoding.UTF8;
            string data = string.Empty;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        using (GZipStream gZipStream = new GZipStream(responseStream, CompressionMode.Decompress))
                        {
                            using (StreamReader reader = new StreamReader(gZipStream, encodeTemp))
                            {
                                data = reader.ReadToEnd();
                            }
                        }
                    }
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        using (DeflateStream deflateStreame = new DeflateStream(responseStream, CompressionMode.Decompress))
                        {
                            using (StreamReader reader = new StreamReader(deflateStreame, encodeTemp))
                            {
                                data = reader.ReadToEnd();
                            }
                        }
                    }
                    else
                    {
                        using (StreamReader reader = new StreamReader(responseStream, encodeTemp))
                        {
                            data = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
                if (request != null)
                {
                    request.Abort();
                }
            }
            return data;
        }

        /// <summary>
        /// 获取HttpWebResponse
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static HttpWebResponse GetWebResponse(HttpWebRequest request, Encoding encode = null)
        {
            HttpWebResponse response = null;

            Encoding encodeTemp = encode ?? Encoding.UTF8;
            string data = string.Empty;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                return response;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 将请求参数写入
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="request"></param>
        public static void SetRequestParameters(NameValueCollection parameters, HttpWebRequest request, Encoding encode = null)
        {
            int count = 0;
            StringBuilder queryString = new StringBuilder();
            Encoding tempEncode = encode ?? Encoding.UTF8;
            foreach (var key in parameters.AllKeys)
            {
                if (count == 0)
                {
                    queryString.AppendFormat("{0}={1}", key, Encode(parameters[key], tempEncode));
                }
                else
                {
                    queryString.AppendFormat("&{0}={1}", key, Encode(parameters[key], tempEncode));
                }
                count++;
            }

            byte[] postDataBytes = tempEncode.GetBytes(queryString.ToString());
            request.ContentLength = postDataBytes.Length;
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(postDataBytes, 0, postDataBytes.Length);
            }
        }
        /// <summary>
        /// 将字符串编码
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Encode(string input, Encoding encode = null)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            Encoding tempEncode = encode ?? Encoding.UTF8;
            return Encoding.ASCII.GetString(EncodeToBytes(input, tempEncode));
        }
        
        /// <summary>
        /// 将字符串转化成字节数组
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        private static byte[] EncodeToBytes(string input, Encoding encode)
        {
            if (string.IsNullOrEmpty(input))
                return new byte[0];

            byte[] inbytes = encode.GetBytes(input);

            int unsafeChars = 0;
            char c;
            foreach (byte b in inbytes)
            {
                c = (char)b;
                if (NeedsEscaping(c))
                {
                    unsafeChars++;
                }
            }

            if (unsafeChars == 0)
            {
                return inbytes;
            }

            byte[] outbytes = new byte[inbytes.Length + (unsafeChars * 2)];
            int pos = 0;

            for (int i = 0; i < inbytes.Length; i++)
            {
                byte b = inbytes[i];

                if (NeedsEscaping((char)b))
                {
                    outbytes[pos++] = (byte)'%';
                    outbytes[pos++] = (byte)IntToHex((b >> 4) & 0xf);
                    outbytes[pos++] = (byte)IntToHex(b & 0x0f);
                }
                else
                {
                    outbytes[pos++] = b;
                }
            }

            return outbytes;
        }
        /// <summary>
        /// 过滤字符
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static bool NeedsEscaping(char c)
        {
            return !((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')
                    || c == '-' || c == '_' || c == '.' || c == '~');
        }
        /// <summary>
        /// 将整形转化成16进制的字符
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static char IntToHex(int n)
        {
            if (n < 0 || n >= 16)
            {
                throw new ArgumentOutOfRangeException("n");
            }

            if (n <= 9)
            {
                return (char)(n + (int)'0');
            }
            else
            {
                return (char)(n - 10 + (int)'A');
            }
        }
    }

    /// <summary>
    /// http请求类型
    /// </summary>
    public enum RequestMethod
    {
        GET,
        POST,
        PUT,
        DELETE
    }

    public class Configs
    {
        public const string ChromeAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.112 Safari/537.36";
    }
}
