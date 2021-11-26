using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace QuickApp
{
    public class ScriptCallbackManager
    {
        public string FileSystemGetRecentFiles()
        {
            CReply rpy = new CReply();
            try
            {
                rpy.Data = Apis.GetRecentFiles();
            }
            catch (Exception exp)
            {
                rpy.ErrCode = 0xFFFF;
                rpy.ErrStr = exp.ToString();
            }
            return rpy.toJson();
        }
        public string FileSystemGetFiles(string str)
        {
            CRequest req = new CRequest();
            CReply rpy = new CReply();
            try
            {
                req.FromJson(str);
                string url = (string)req.Data;
                List<FileSystemItemsInfo> fsItems = new List<FileSystemItemsInfo>();
                rpy.Data = fsItems;
                if (String.IsNullOrEmpty(url))
                {
                    List<FileSystemElementInfo> items = new List<FileSystemElementInfo>();
                    foreach (var item in Directory.GetLogicalDrives())
                    {
                        items.Add(new FileSystemElementInfo() { FullPath = item, Name = item.GetFilePathTopestName() });
                    }
                    fsItems.Add(FileSystemItemsInfo.AsDrive(items));
                }
                else
                {
                    List<FileSystemElementInfo> dirItems = new List<FileSystemElementInfo>();
                    List<FileSystemElementInfo> fileItems = new List<FileSystemElementInfo>();
                    foreach (var item in Directory.GetDirectories(url))
                    {
                        dirItems.Add(new FileSystemElementInfo() { FullPath = item, Name = item.GetFilePathTopestName() });
                    }
                    fsItems.Add(FileSystemItemsInfo.AsDirectory(dirItems));
                    foreach (var item in Directory.GetFiles(url))
                    {
                        fileItems.Add(new FileSystemElementInfo() { FullPath = item, Name = item.GetFilePathTopestName() });
                    }
                    fsItems.Add(FileSystemItemsInfo.AsFile(fileItems));
                }
            }
            catch (Exception e)
            {
                rpy.ErrCode = 0xFFFF;
                rpy.ErrStr = e.ToString();
            }
            string ttt = rpy.toJson();
            return rpy.toJson();
        }

        public string FileSystemMoveTo(string str)
        {
            CRequest req = new CRequest();
            CReply rpy = new CReply();
            try
            {
                req.FromJson(str);
                var data = (Dictionary<string, string>)req.Data;
                string srcFilePath = data["srcFilePath"];
                string srcType = data["srcType"];
                string destFilePath = data["destFilePath"];
                string destType = data["destType"];
                if (srcType == FileSystemItemsInfo.FILE && destType == FileSystemItemsInfo.FILE)
                {
                    File.Move(srcFilePath, destFilePath);
                }
                else if (srcType == FileSystemItemsInfo.FILE && destType == FileSystemItemsInfo.DIRECTORY)
                {
                    destFilePath += "\\" + srcFilePath.GetFilePathTopestName();
                    File.Move(srcFilePath, destFilePath);
                }
                else if (srcType == FileSystemItemsInfo.DIRECTORY && destType == FileSystemItemsInfo.DIRECTORY)
                {
                    Directory.Move(srcFilePath, destType);
                }
                else
                {
                    rpy.ErrCode = 1;
                    rpy.ErrStr = "file type error";
                }
            }
            catch (Exception e)
            {
                rpy.ErrCode = 0xFFFF;
                rpy.ErrStr = e.ToString();
            }
            return rpy.toJson();
        }

        public string FileSystemCopyTo(string str)
        {
            CRequest req = new CRequest();
            CReply rpy = new CReply();
            try
            {
                req.FromJson(str);
                var data = (Dictionary<string, string>)req.Data;
                string srcFilePath = data["srcFilePath"];
                string srcType = data["srcType"];
                string destFilePath = data["destFilePath"];
                string destType = data["destType"];
                if (srcType == FileSystemItemsInfo.FILE && destType == FileSystemItemsInfo.FILE)
                {
                    File.Copy(srcFilePath, destFilePath);
                }
                else if (srcType == FileSystemItemsInfo.FILE && destType == FileSystemItemsInfo.DIRECTORY)
                {
                    destFilePath += "\\" + srcFilePath.GetFilePathTopestName();
                    File.Copy(srcFilePath, destFilePath);
                }
                else if (srcType == FileSystemItemsInfo.DIRECTORY && destType == FileSystemItemsInfo.DIRECTORY)
                {
                    Apis.CopyDirectory(srcFilePath, destType);
                }
                else
                {
                    rpy.ErrCode = 1;
                    rpy.ErrStr = "file type error";
                }
            }
            catch (Exception e)
            {
                rpy.ErrCode = 0xFFFF;
                rpy.ErrStr = e.ToString();
            }
            return rpy.toJson();
        }

        public string FileSystemDelete(string str)
        {
            CRequest req = new CRequest();
            CReply rpy = new CReply();
            try
            {
                req.FromJson(str);
                var data = (Dictionary<string, string>)req.Data;
                string filePath = data["filePath"];
                string fileType = data["type"];
                switch (fileType)
                {
                    case FileSystemItemsInfo.DIRECTORY:
                        Directory.Delete(filePath, true);
                        break;
                    case FileSystemItemsInfo.FILE:
                        File.Delete(filePath);
                        break;
                    default:
                        rpy.ErrCode = 1;
                        rpy.ErrStr = "file type error";
                        break;
                }

            }
            catch (Exception e)
            {
                rpy.ErrCode = 0xFFFF;
                rpy.ErrStr = e.ToString();
            }
            return rpy.toJson();
        }

        public string StartApp(string str)
        {
            CRequest req = new CRequest();
            CReply rpy = new CReply();
            try
            {
                req.FromJson(str);
                string filePath = (string)req.Data;
                if (!Apis.StartApp(filePath))
                {
                    rpy.ErrCode = 1;
                }
            }
            catch (Exception e)
            {
                rpy.ErrCode = 0xFFFF;
                rpy.ErrStr = e.ToString();
            }
            return rpy.toJson();
        }


        [Serializable]
        class FileSystemItemsInfo
        {
            private string type;
            private List<FileSystemElementInfo> items = new List<FileSystemElementInfo>();

            public const string DRIVE = "drive";
            public const string DIRECTORY = "directory";
            public const string FILE = "file";

            public string Type { get => type; set => type = value; }
            public List<FileSystemElementInfo> Items { get => items; set => items = value; }

            public static FileSystemItemsInfo AsFile(List<FileSystemElementInfo> items)
            {
                return new FileSystemItemsInfo() { Type = FILE, Items = items };
            }
            public static FileSystemItemsInfo AsDirectory(List<FileSystemElementInfo> items)
            {
                return new FileSystemItemsInfo() { Type = DIRECTORY, Items = items };
            }
            public static FileSystemItemsInfo AsDrive(List<FileSystemElementInfo> items)
            {
                return new FileSystemItemsInfo() { Type = DRIVE, Items = items };
            }

        }


        [Serializable]
        class FileSystemElementInfo
        {
            private string name;
            private string fullPath;

            public string Name { get => name; set => name = value; }
            public string FullPath { get => fullPath; set => fullPath = value; }
        }

        [Serializable]
        class CRequest
        {
            private int errCode = 0;
            private string errStr = "";
            private object data;

            public CRequest()
            {

            }

            public int ErrCode { get => errCode; set => errCode = value; }
            public string ErrStr { get => errStr; set => errStr = value; }
            public object Data { get => data; set => data = value; }
            public bool FromJson(string str)
            {
                CRequest r = null;
                try
                {
                    r = JsonConvert.DeserializeObject<CRequest>(str);
                    ErrStr = r.ErrStr;
                    ErrCode = r.ErrCode;
                    Data = r.Data;
                    return true;
                }
                catch (Exception e)
                {

                }
                return false;
            }
            public string toJson()
            {
                return JsonConvert.SerializeObject(this);
            }
        }
        [Serializable]
        class CReply
        {
            private int errCode = 0;
            private string errStr = "";
            private object data;
            public CReply()
            {
                errCode = 0;
                errStr = "";
            }

            public int ErrCode { get => errCode; set => errCode = value; }
            public string ErrStr { get => errStr; set => errStr = value; }
            public object Data { get => data; set => data = value; }
            public string toJson()
            {
                return JsonConvert.SerializeObject(this);
            }

        }
    }
}
