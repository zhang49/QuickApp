using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace QuickApp
{
    public class ScriptCallbackManager
    {
        public delegate bool GenDocDelegate(string data);
        public GenDocDelegate GenDocFunc;

        /// <summary>
        /// 生成文档, js调用方法时，方法名首字母需小写
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string GenerateDoc(string data)
        {
            Dictionary<string, object> root = new Dictionary<string, object>();
            try
            {
                bool? ret = GenDocFunc?.Invoke(data);
                if (ret.HasValue && ret == true)
                {
                    var msgData = new Dictionary<string, object>();
                    //msgData["output_filepath"] = Directory.GetCurrentDirectory() + Properties.Resources.OutPutDocRelFilePath;
                    root["error_code"] = 0;
                    root["error_str"] = "";
                    root["data"] = msgData;
                }
            }
            catch (Exception exp)
            {
                root["error_code"] = 1;
                root["error_str"] = exp.Message;
            }
            return JsonConvert.SerializeObject(root);
        }
        public string ShowAccessSystemImgEditWindow(string data)
        {
            Dictionary<string, object> root = new Dictionary<string, object>();
            try
            {
                root["error_code"] = 0;
                root["error_str"] = "";

            }
            catch (Exception exp)
            {
                root["error_code"] = 1;
                root["error_str"] = exp.ToString();
            }
            return JsonConvert.SerializeObject(root);
        }

        public string GetAccessSystemImg()
        {
            Dictionary<string, object> root = new Dictionary<string, object>();
            root["error_code"] = 1;
            root["error_str"] = "open error";
            root["data"] = "";
            try
            {
                //string imgBase64 = CUtil.GetImageBase64(Directory.GetCurrentDirectory() + Properties.Resources.AccessSystemImgRelFilePath, new Size(610, 488));
                //if (!string.IsNullOrEmpty(imgBase64))
                //{
                //    root["error_code"] = 0;
                //    root["error_str"] = "";
                //    root["data"] = "data:image/jpg;base64," + imgBase64;
                //}
            }
            catch (Exception exp)
            {
                root["error_str"] = exp.ToString();
            }
            return JsonConvert.SerializeObject(root);
        }

        public string OpenOutPutDoc()
        {
            Dictionary<string, object> root = new Dictionary<string, object>();
            try
            {
              //  System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() + Properties.Resources.OutPutDocRelFilePath);
                root["error_code"] = 0;
                root["error_str"] = "";
                root["data"] = "";
            }
            catch (Exception exp)
            {
                root["error_code"] = root["error_str"] = exp.ToString();
                root["data"] = "";
            }
            return JsonConvert.SerializeObject(root);
        }


    }
}
