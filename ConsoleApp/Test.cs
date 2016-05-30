using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Spider
{
    class Test
    {
        public void Test1()
        {
            HttpClient h = new HttpClient();

            h.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            h.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:33.0) Gecko/20100101 Firefox/33.0");
            h.DefaultRequestHeaders.Add("Referer", "http://urmia.divar.ir/browse/");
            h.DefaultRequestHeaders.Add("Pragma", "no-cache");
            h.DefaultRequestHeaders.Add("Host", "urmia.divar.ir");
            // h.DefaultRequestHeaders.Add("Content-Type","application/json; charset=UTF-8");
            h.DefaultRequestHeaders.Add("Connection", "keep-alive");
            h.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            h.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            h.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");

            NameValueCollection nameValue = new NameValueCollection();
            nameValue["password"] = "qazwsx123";
            nameValue["captcha_type"] = "cn";
            nameValue["remember_me"] = "true";
            nameValue["phone_num"] = "18515496613";
            StringBuilder sb = new StringBuilder();
            foreach (var key in nameValue.AllKeys)
            {
                
              sb.AppendFormat("{0}={1}&", key, nameValue[key]);
  
            }
            var str = sb.ToString().TrimEnd('&');
            HttpContent requestContent = new StreamContent(GenerateStreamFromString(str));
            Task<HttpResponseMessage> response = h.PostAsync("http://urmia.divar.ir/json/", requestContent);
            response.Wait(3000);
            byte[] responseText = response.Result.Content.ReadAsByteArrayAsync().Result;
            System.Console.WriteLine(responseText); // you would know what to do with the data ;)
        }

        static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
