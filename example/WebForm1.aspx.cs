using detuyun_sdk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace newdocuments
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
           
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            DetuYun detuyun = new DetuYun("moriaty", "ifzdm129", "xsgam6abjrmm2jytcnc4enzwpu8wtd");

            // 切换 API 接口的域名
            //detuyun.setApiDomain("s.DetuYun.com");

            /// 获取空间占用大小
            var str = detuyun.getBucketUsage();
            this.Label1.Text = str.ToString();

            /// 创建目录
            // 创建目录时可使用 detuyun.mkDir("/a/b/c", true) //进行父级目录的自动创建（最深10级目录）
            //var str = detuyun.mkDir("/dw", true);
            //this.Label1.Text = str.ToString();

            /// 上传文件
            //Hashtable headers = new Hashtable();
            ////detuyun.delete("image\a.jpg", headers);
            //FileStream fs = new FileStream("h:\\picture\\cosplay\\my.jpg", FileMode.Open, FileAccess.Read);
            //BinaryReader r = new BinaryReader(fs);
            //byte[] postArray = r.ReadBytes((int)fs.Length);
            /// 设置待上传文件的 Content-MD5 值（如得图云服务端收到的文件MD5值与用户设置的不一致，将回报错误）
            //detuyun.setContentMD5(DetuYun.md5_file("h:\\picture\\cosplay\\my.jpg"));
            /// 设置待上传文件的访问密钥（注意：仅支持图片空！，设置密钥后，无法根据原文件URL直接访问，需带 URL 后面加上 （缩略图间隔标志符+密钥） 进行访问）
            /// 如缩略图间隔标志符为 ! ，密钥为 bac，上传文件路径为 /folder/test.jpg ，那么该图片的对外访问地址为： http://空间域名/folder/test.jpg!bac
            //detuyun.setFileSecret("bac");
            //Console.WriteLine("上传文件");
            //bool b = detuyun.writeFile("/su", postArray, true);
            // 上传文件时可使用 upyun.writeFile("/a/test.jpg",postArray, true); //进行父级目录的自动创建（最深10级目录）
                     
            //var str = detuyun.writeFile("/dc1/my2.jpg", postArray, true);

            /// 读取目录
   
            //ArrayList str = detuyun.readDir("/dc1/su");
            //foreach (var item in str)
            //{
            //    FolderItem a = (FolderItem)item;
            //    //Console.WriteLine(a.filename);
            //    this.Label1.Text += a.name + "|";
            //}

            //this.Label1.Text = str.ToString();
            //this.Label2.Text = str1.ToString();


            /// 获取总体空间的占用信息
            //var s = detuyun.getFolderUsage("/");
            //this.Label1.Text = s.ToString();

            /// 获取文件信息 return Hashtable('type'=> file | folder, 'size'=> file size, 'date'=> unix time) 或 null
            //Hashtable ht = detuyun.getFileInfo("/dc1/my2.jpg");
            //this.Label1.Text = ht["type"].ToString()+"|"+ht["date"].ToString();



            ///读取文件
            //byte[] contents = detuyun.readFile("/dc1/my.jpg");
            //var s = contents.Length;
            //this.Label1.Text = s.ToString();


            /// 删除文件
           
            //var s = detuyun.deleteFile("/dc1/my.jpg");
            //this.Label1.Text = s.ToString();
            
            ///删除目录
            //var s = detuyun.rmDir("/dq/");
            //this.Label1.Text = s.ToString();
        }
    }
}