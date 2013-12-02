using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Globalization;
using System.Threading;
using System.Reflection;


namespace detuyun_sdk
{
    public class DetuYun
    {
        private string bucketname;
        private string username;
        private string password;
        private int timeout = 30;
        private string content_md5;
        private string file_secret;
        protected string endpoint;
        private string api_domain = "s.detuyun.com";
        private bool auth = true;
        private string DL = "/";
        private Hashtable tmp_infos = new Hashtable();
        private bool auto_mkdir = false;
        public string version() { return "1.1.5"; }
        /// <summary>
        /// 初始化 DetuYun 存储接口
	    /// </summary>
        /// <param name="bucketname">空间名称</param>
        /// <param name="username">Access Key</param>
        /// <param name="password">Access Secret</param>
        public DetuYun(string bucketname, string username, string password)
        {
            this.bucketname = bucketname;
            this.username = username;
            this.password = password;
        }
        public DetuYun(string bucketname, string username, string password,int timeout)
        {
            this.bucketname = bucketname;
            this.username = username;
            this.password = password;
            this.timeout = timeout;
        }
        /// <summary>
        /// 切换 API 接口的域名
        /// </summary>
        /// <param name="domain"></param>
        public void setApiDomain(string domain)
        {
            this.api_domain = domain;
        }
        private string md5(string str)
        {
            MD5 m = new MD5CryptoServiceProvider();
            byte[] s = m.ComputeHash(UnicodeEncoding.UTF8.GetBytes(str));
            string resule = BitConverter.ToString(s);
            resule = resule.Replace("-", "");
            return resule.ToLower();
        }

        private void detuyunAuth(Hashtable headers, string method, string uri, HttpWebRequest request)
        {
            DateTime dt = DateTime.UtcNow;
            string date = dt.ToString("ddd, dd MMM yyyy HH':'mm':'ss 'GMT'", CultureInfo.CreateSpecificCulture("en-US"));
            request.Date = dt;
          //  headers.Add("Date", date);
            string auth;
            auth = md5( uri + '&' + date + '&' + md5(this.password));
            headers.Add("Authorization", "DetuYun " + this.username + ':' + auth);
        }


        private HttpWebResponse newWorker(string method, string Url, byte[] postData, Hashtable headers)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://" + api_domain + Url);


            request.Method = method;

            if (this.auto_mkdir == true)
            {
                headers.Add("mkdir", "true");
                this.auto_mkdir = false;
            }
            if (postData != null)
            {
                request.ContentLength = postData.Length;
                request.KeepAlive = true;
                if (this.content_md5 != null)
                {
                    request.Headers.Add("Content-MD5", this.content_md5);
                    this.content_md5 = null;
                }
                if (this.file_secret != null)
                {
                    request.Headers.Add("Content-Secret", this.file_secret);
                    this.file_secret = null;
                }
            }

            if (this.auth)
            {
                detuyunAuth(headers, method, Url, request);
            }
            else
            {
                request.Headers.Add("Authorization", "Basic " +
                    Convert.ToBase64String(new System.Text.ASCIIEncoding().GetBytes(this.username + ":" + this.password)));
            }
            foreach (DictionaryEntry var in headers)
            {
                request.Headers.Add(var.Key.ToString(), var.Value.ToString());
            }

            if (postData != null)
            {
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(postData, 0, postData.Length);
                dataStream.Close();
            }
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                this.tmp_infos = new Hashtable();
                foreach (var hl in response.Headers)
                {
                    string name = (string)hl;
                    if (name.Length > 9 && name.Substring(0, 9) == "x-detuyun")
                    {
                        this.tmp_infos.Add(name, response.Headers[name]);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return response;
        }

        private bool delete(string path, Hashtable headers)
        {
            HttpWebResponse resp;
            byte[] a = null;
            resp = newWorker("DELETE", DL + this.bucketname + path, a, headers);
            if ((int)resp.StatusCode == 200)
            {
                resp.Close();
                return true;
            }
            else
            {
                resp.Close();
                return false;
            }
        }

        /// <summary>
        /// 获取总体空间的占用信息
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>

        public int getFolderUsage(string url)
        {
            Hashtable headers = new Hashtable();
            int size;
            byte[] a = null;
            HttpWebResponse resp = newWorker("GET", DL + this.bucketname + url + "?usage", a, headers);
            try
            {
                StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
                string strhtml = sr.ReadToEnd();
                resp.Close();
                size = int.Parse(strhtml);
            }
            catch (Exception ex)
            {
                size = 0;
            }
            return size;
        }


        /// <summary>
        /// 获取空间使用情况
        /// </summary>
        /// <returns></returns>
        public int getBucketUsage()
        {
            return getFolderUsage("");
        }
        
        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="path"></param>
        /// <param name="auto_mkdir"></param>
        /// <returns></returns>
        public bool mkDir(string path, bool auto_mkdir)
        {
            this.auto_mkdir = auto_mkdir;
            Hashtable headers = new Hashtable();
            headers.Add("folder", "create");
            HttpWebResponse resp;
            byte[] a = new byte[0];
            resp = newWorker("PUT", DL + this.bucketname + path, a, headers);
            if ((int)resp.StatusCode == 200)
            {
                resp.Close();
                return true;
            }
            else
            {
                resp.Close();
                return false;
            }
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool rmDir(string path)
        {
            Hashtable headers = new Hashtable();
            return delete(path, headers);
        }

        /// <summary>
        /// 读取目录列表
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ArrayList readDir(string url)
        {
            Hashtable headers = new Hashtable();
            byte[] a = null;
            HttpWebResponse resp = newWorker("GET", DL + this.bucketname + url, a, headers);
            StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
            string strhtml = sr.ReadToEnd();
            resp.Close();
            strhtml = strhtml.Replace("\t", "\\");
            strhtml = strhtml.Replace("\n", "\\");
            string[] ss = strhtml.Split('\\');
            int i = 0;
            ArrayList AL = new ArrayList();
            while (i < ss.Length)
            {
                FolderItem fi = new FolderItem(ss[i], ss[i + 1], int.Parse(ss[i + 2]), int.Parse(ss[i + 3]), ss[i + 4]);
                AL.Add(fi);
                i +=6;
            }
            return AL;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="auto_mkdir"></param>
        /// <returns></returns>
        public bool writeFile(string path, byte[] data, bool auto_mkdir)
        {
            Hashtable headers = new Hashtable();
            this.auto_mkdir = auto_mkdir;
            HttpWebResponse resp;
            resp = newWorker("PUT", DL + this.bucketname + path, data, headers);
            if ((int)resp.StatusCode == 200)
            {
                resp.Close();
                return true;
            }
            else
            {
                resp.Close();
                return false;
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool deleteFile(string path)
        {
            Hashtable headers = new Hashtable();
            return delete(path, headers);
        }


        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public byte[] readFile(string path)
        {
            Hashtable headers = new Hashtable();
            byte[] a = null;

            HttpWebResponse resp = newWorker("GET", DL + this.bucketname + path, a, headers);
            StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
            BinaryReader br = new BinaryReader(sr.BaseStream);
            byte[] by = br.ReadBytes(1024 * 1024 * 200); /// 得图云存储最大文件限制 200MB
            resp.Close();
            return by;
        }

        /**
       * 设置待上传文件的 Content-MD5 值（如得图云服务端收到的文件MD5值与用户设置的不一致，将回报 Not Accept错误）
       * @param $str （文件 MD5 校验码）
       * return null;
       */
        public void setContentMD5(string str)
        {
            this.content_md5 = str;
        }
        /**
        * 设置待上传文件的 访问密钥（注意：仅支持图片空！，设置密钥后，无法根据原文件URL直接访问，需带 URL 后面加上 （缩略图间隔标志符+密钥） 进行访问）
        * 如缩略图间隔标志符为 ! ，密钥为 bac，上传文件路径为 /folder/test.jpg ，那么该图片的对外访问地址为： http://空间域名/folder/test.jpg!bac
        * @param $str （文件 MD5 校验码）
        * return null;
        */
        public void setFileSecret(string str)
        {
            this.file_secret = str;
        } 
        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public Hashtable getFileInfo(string file)
        {
            Hashtable headers = new Hashtable();
            byte[] a = null;
            HttpWebResponse resp = newWorker("HEAD", DL + this.bucketname + file, a, headers);
            resp.Close();
            Hashtable ht;
            string  tmp = this.tmp_infos["x-detuyun-file"].ToString();
            string[] info = tmp.Split('\t');
            try
            {
                ht = new Hashtable();
                ht.Add("type", info[1]);
                ht.Add("size", info[2]);
                ht.Add("date", info[3]);
                //ht.Add("filetype", this.tmp_infos["x-detuyun-file-filetype"]);
            }
            catch (Exception)
            {
                ht = new Hashtable();
            }
            return ht;
        }
        //计算文件的MD5码
        public static string md5_file(string pathName)
        {
            string strResult = "";
            string strHashData = "";

            byte[] arrbytHashValue;
            System.IO.FileStream oFileStream = null;

            System.Security.Cryptography.MD5CryptoServiceProvider oMD5Hasher =
                       new System.Security.Cryptography.MD5CryptoServiceProvider();

            try
            {
                oFileStream = new System.IO.FileStream(pathName, System.IO.FileMode.Open,
                      System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                arrbytHashValue = oMD5Hasher.ComputeHash(oFileStream);//计算指定Stream 对象的哈希值
                oFileStream.Close();
                //由以连字符分隔的十六进制对构成的String，其中每一对表示value 中对应的元素；例如“F-2C-4A”
                strHashData = System.BitConverter.ToString(arrbytHashValue);
                //替换-
                strHashData = strHashData.Replace("-", "");
                strResult = strHashData;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            return strResult.ToLower();
        }


        internal void setApiDomain()
        {
            throw new NotImplementedException();
        }
    }
    //文件目录
    public class FolderItem
    {
        public string name;
        public string type;
        public int size;
        public int time;
        public string filetype;
       
        public FolderItem(string name, string type, int size, int time,string filetype)
        {
            this.name = name;
            this.type = type;
            this.size = size;
            this.time = time;
            this.filetype = filetype;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            DetuYun detuyun = new DetuYun("bucketname", "username", "password");

            /// 切换 API 接口的域名
            detuyun.setApiDomain("s.DetuYun.com");

            ///  获取空间占用大小
            Console.WriteLine("获取空间占用大小");
            Console.WriteLine(detuyun.getBucketUsage());

            /// 创建目录
             //Console.WriteLine(detuyun.mkDir("/b/ccc",true));
            // 创建目录时可使用 detuyun.mkDir("/a/b/c", true) //进行父级目录的自动创建（最深10级目录）
           
            /// 上传文件
            Hashtable headers = new Hashtable();
            //detuyun.delete("image\a.jpg", headers);
            FileStream fs = new FileStream("f:\\picture\\detu1.jpg", FileMode.Open, FileAccess.Read);
            BinaryReader r = new BinaryReader(fs);
            byte[] postArray = r.ReadBytes((int)fs.Length);
            /// 设置待上传文件的 Content-MD5 值（如服务端收到的文件MD5值与用户设置的不一致，将回报错误）
            //detuyun.setContentMD5(DetuYun.md5_file("c:\\1c36319a4ec53893663bd1f1a7b7051e.jpg"));

            ///// 设置待上传文件的 访问密钥（注意：仅支持图片空！，设置密钥后，无法根据原文件URL直接访问，需带 URL 后面加上 （缩略图间隔标志符+密钥） 进行访问）
            ///// 如缩略图间隔标志符为 ! ，密钥为 bac，上传文件路径为 /folder/test.jpg ，那么该图片的对外访问地址为： http://空间域名/folder/test.jpg!bac
            //detuyun.setFileSecret("bac");
            //Console.WriteLine("上传文件");
            bool b = detuyun.writeFile("/a/test.jpg", postArray, true);
            // 上传文件时可使用 upyun.writeFile("/a/test.jpg",postArray, true); //进行父级目录的自动创建（最深10级目录）
            Console.WriteLine(b);

           
            /// 读取目录
            Console.WriteLine("读取目录");
            ArrayList str = detuyun.readDir("/a/");
            foreach (var item in str)
            {
                FolderItem a = (FolderItem)item;
                Console.WriteLine(a.name);
            }

            ///获取总体空间的占用信息
            Console.WriteLine("获取某个目录的空间占用大小");
            Console.WriteLine(detuyun.getFolderUsage("/a/"));

            /// 读取文件
            /// 请查阅代码中的 readFile 函数代码，得图云存储最大文件限制 200Mb
            /// 或在此基础上改写成自己需要的形式

            /// 另外推荐通过web访问文件接口下载文件而非api接口
            Console.WriteLine("读取文件");
            byte[] contents = detuyun.readFile("/a/test.jpg");
            Console.WriteLine(contents.Length);

            /// 获取文件信息 return Hashtable('type'=> file | folder, 'size'=> file size, 'date'=> unix time) 或 null
            Console.WriteLine("获取文件信息");
            Hashtable ht = detuyun.getFileInfo("/a/test.jpg");
            Console.WriteLine(ht["type"]);
            Console.WriteLine(ht["size"]);
            Console.WriteLine(ht["date"]);
            Console.WriteLine(ht["filetype"]);
           
            /// 删除文件
            Console.WriteLine("删除文件");
            Console.WriteLine(detuyun.deleteFile("/a/test.jpg"));

            /// 删除目录（目录必须为空）
            Console.WriteLine("删除目录");
            Console.WriteLine(detuyun.rmDir("/a/"));

            Console.Read();
        }
    }
}