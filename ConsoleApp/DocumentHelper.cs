using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spider
{
    /// <summary>
    /// Html Document 帮助类
    /// </summary>
    public class DocumentHelper
    {
        /// <summary>
        /// 从HtmlDocument中根据input元素的名称获取value值
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="inputName"></param>
        /// <returns></returns>
        public static string GetInputValue(HtmlDocument doc, string inputName)
        {
            string result = string.Empty;
            try
            {
                string xpath = string.Format("//input[@name='{0}']", inputName);
                HtmlNode inputNode = doc.DocumentNode.SelectSingleNode(xpath);
                if (inputNode != null && inputNode.Attributes != null && inputNode.Attributes.Count > 0)
                {
                    HtmlAttribute attribute = inputNode.Attributes["value"];
                    if (attribute != null)
                    {
                        result = attribute.Value;
                    }
                }
            }
            catch (Exception)
            {


            }
            return result;
        }
        /// <summary>
        /// json对象的属性字典
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static Dictionary<string,dynamic> JsonToDic(string jsonString)
        {
            var jsonResult = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonString);
            return jsonResult;
        }
        /// <summary>
        /// 获取json对象中的某个key值
        /// </summary>
        /// <param name="sonString"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetJsonValue(string sonString, string key)
        {
            var jsonResult = JsonToDic(sonString);
            return jsonResult[key];
        }
        /// <summary>
        /// 字符转为流
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Stream GenerateStreamFromString(string s)
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
