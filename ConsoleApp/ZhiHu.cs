using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace Spider
{
    /// <summary>
    /// 知乎
    /// </summary>
    public class ZhiHu
    {
        private string Phone { get; set; }
        private string PassWord { get; set; }
        string index = "https://www.zhihu.com";//首页
        string login = "https://www.zhihu.com/login/phone_num";//手机登录页
        CookieContainer cookies = new CookieContainer();//cookie
        NameValueCollection nameValue = new NameValueCollection();


        public ZhiHu(string phonNum, string pass)
        {
            this.Phone = phonNum;
            this.PassWord = pass;
        }
        /// <summary>
        /// 请求登录页
        /// </summary>
        /// <returns></returns>
        public void Index()
        {
            //首页
            var request = HttpHelper.CreateRequest(index, RequestMethod.GET, cookies);
            request.Headers["Accept-Language"] = "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4";
            request.Headers["Accept-Encoding"] = "gzip, deflate, sdch";
            request.Accept = @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            var respData = HttpHelper.GetWebResponseData(request, null);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(respData);
            var xsdf = DocumentHelper.GetInputValue(doc, "_xsrf");//登录需要
            nameValue["_xsrf"] = xsdf;
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public bool Login()
        {
            //登录
            var request = HttpHelper.CreateRequest(login, RequestMethod.POST, cookies);
            request.Accept = @"Accept: */*";
            request.Headers["X-Requested-With"] = "XMLHttpRequest";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.Headers["Origin"] = "https://www.zhihu.com/";
            request.Headers["Accept-Language"] = "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4";
            request.Headers["Accept-Encoding"] = "gzip, deflate, sdch";
            //post参数
            nameValue["password"] = PassWord;
            nameValue["captcha_type"] = "cn";
            nameValue["remember_me"] = "true";
            nameValue["phone_num"] = Phone;
            //参数写入HttpWebRequest
            HttpHelper.SetRequestParameters(nameValue, request);
            //获取结果
            var jsonString = HttpHelper.GetWebResponseData(request, null);

            var msg = DocumentHelper.GetJsonValue(jsonString, "msg");//检查msg=登陆成功
            Console.WriteLine(msg);
            if (msg == "登陆成功")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 登录后首页
        /// </summary>
        /// <returns></returns>
        public string AfterLogin()
        {
            var request = HttpHelper.CreateRequest(index, RequestMethod.GET, cookies);
            request.Headers["Accept-Language"] = "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4";
            request.Headers["Accept-Encoding"] = "gzip, deflate, sdch";
            request.Accept = @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            var data = HttpHelper.GetWebResponseData(request, null);//内容，接下来就可以解析了
            return data;
        }

        public async Task<string> HttpClientTest()
        {
            HttpClient h = new HttpClient(
            new HttpClientHandler
            {
              //CookieContainer = cookies,
              AutomaticDecompression = DecompressionMethods.GZip //防止返回的json乱码
                                       | DecompressionMethods.Deflate
            });
            
            h.DefaultRequestHeaders.Add("UserAgent", Configs.ChromeAgent);
            h.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
            h.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, sdch");
            h.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            //1.首页
            var response = await h.GetAsync(index);
            string content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                //获取隐藏的input值
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(content);
                var xsdf = DocumentHelper.GetInputValue(doc, "_xsrf");//登录需要
                nameValue["_xsrf"] = xsdf;
            }
            else
            {
                return null;
            }
            //2.登陆
            h.DefaultRequestHeaders.Clear();
            h.DefaultRequestHeaders.Add("UserAgent", Configs.ChromeAgent);
            h.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            h.DefaultRequestHeaders.Add("Origin", index);
            h.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
            h.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, sdch");
            h.DefaultRequestHeaders.Add("Accept", "*/*");
            //post参数
            nameValue["password"] = PassWord;
            nameValue["captcha_type"] = "cn";
            nameValue["remember_me"] = "true";
            nameValue["phone_num"] = Phone;
            StringBuilder sb = new StringBuilder();
            foreach (var key in nameValue.AllKeys)
            {
                sb.AppendFormat("{0}={1}&", key, nameValue[key]);
            }
            var str = sb.ToString().TrimEnd('&');
            var request = new HttpRequestMessage(HttpMethod.Post, login);
            var requestContent = str;
            request.Content = new StringContent(requestContent, Encoding.UTF8, "application/x-www-form-urlencoded");

            response = await h.SendAsync(request);
            content = await response.Content.ReadAsStringAsync();
            var dic = DocumentHelper.JsonToDic(content);

            if (dic.ContainsKey("msg"))
            {
                if (dic["msg"] != "登陆成功")//登录过于频繁，请稍等重试;errcode:100030
                {
                    Console.WriteLine(dic["msg"]);
                    return null;
                }
            }
            //3.抓取首页
            h.DefaultRequestHeaders.Clear();
            h.DefaultRequestHeaders.Add("UserAgent", Configs.ChromeAgent);
            h.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
            h.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, sdch");
            h.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            response = await h.GetAsync(index);
            content = await response.Content.ReadAsStringAsync();
            return content;

        }
    }
}
