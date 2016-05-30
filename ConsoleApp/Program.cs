using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Spider
{
    class Program
    {
        static void Main(string[] args)
        {
            ZhiHu zh = new ZhiHu("18515496613","qazwsx123");
            var d= zh.HttpClientTest();
            //zh.Login();
            //var b = zh.Login();
            //var data = zh.AfterLogin();
            Console.WriteLine(d.Result);
        }
    }

}
