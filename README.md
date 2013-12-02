detuyun-csharp-sdk
==================


此 CSharp SDK 适用于.net framework>4.0版本，基于 得图云存储官方API 构建。使用此 SDK 构建您的网络应用程序，能让您以非常便捷地方式将数据安全地存储到七牛云存储上。无论您的网络应用是一个网站程序，还是包括从云端（服务端程序）到终端（手持设备应用）的架构的服务或应用，通过得图云存储及其 SDK，都能让您应用程序的终端用户高速上传和下载，同时也让您的服务端更加轻盈。

- [应用接入](#install)
	- [获取Access Key 和 Secret Key](#acc-appkey)
- [使用说明](#detuyun-api)
	- [1 初始化DetuYun](#detuyun-init)
	- [2 上传文件](#detuyun-upload)
	- [3 下载文件](#detuyun-down)
	- [4 创建目录](#detuyun-createdir)
	- [5 删除目录或者文件](#detuyun-deletedir)
	- [6 获取目录文件列表](#detuyun-getdir)
	- [7 获取文件信息](#detuyun-getfile)
	- [8 获取空间使用状况](#detuyun-getused)
- [异常处理](#detuyun-exception)



<a name="install"></a>
## 应用接入

<a name="acc-appkey"></a>

### 1. 获取Access Key 和 Secret Key

要接入得图云存储，您需要拥有一对有效的 Access Key 和 Secret Key 用来进行签名认证。可以通过如下步骤获得：

1. <a href="http://www.detuyun.com/user/accesskey" target="_blank">登录得图云开发者自助平台，查看 Access Key 和 Secret Key 。</a>

<a name=detuyun-api></a>
## 使用说明

<a name="detuyun-init"></a>
### 1.初始化DetuYun

       
        public DetuYun(string bucketname, string username, string password)
        {
            this.bucketname = bucketname;
            this.username = username;
            this.password = password;
        }


参数`bucketname`为空间名称，`username`为Access Key，`password`为Access Secret。

示例代码如下：

	DetuYun detuyun = new DetuYun("moriaty", "ifzdm129", "xsgam6abjrmm2jytcnc4enzwpu8wtd");

**超时时间设置**
在初始化DetuYun上传时，可以选择设置上传请求超时时间（默认30s）:
```
DetuYun detuyun = new DetuYun("moriaty", "ifzdm129", "xsgam6abjrmm2jytcnc4enzwpu8wtd"，600);
```

<a name="detuyun-upload"></a>
### 2. 上传文件
	// 直接传递文件内容的形式上传
	bool b = detuyun.writeFile('/temp/text_demo.txt', 'Hello World', True);
	Console.WriteLine(b);
	
	// 数据流方式上传，可降低内存占用
	Hashtable headers = new Hashtable();
	FileStream fs = new FileStream("f:\\picture\\detu1.jpg", FileMode.Open, FileAccess.Read);
	BinaryReader r = new BinaryReader(fs);
	byte[] postArray = r.ReadBytes((int)fs.Length);
	bool b = detuyun.writeFile("/a/test.jpg", postArray, true);
	Console.WriteLine(b);

上传成功返回`true`，上传失败则抛出异常。

<a name=detuyun-down></a>
### 3. 下载文件

      
        byte[] contents = upyun.readFile("/a/test.jpg");
			Console.WriteLine(contents.Length);

下载成功，返回文件内容; 失败则抛出相应异常。

<a name=detuyun-createdir></a>
### 4.创建目录

	detuyun.mkDir("/b/ccc",true)

创建目录时可使用 `detuyun.mkDir("/a/b/c", true)`进行父级目录的自动创建，最深10级目录。目录路径必须以斜杠 `/` 结尾，创建成功返回 `True`，否则抛出异常。

<a name=detuyun-deletedir></a>
### 5.删除目录或者文件
	Console.WriteLine(detuyun.rmDir("/a/"));//删除目录
	Console.WriteLine(detuyun.deleteFile("/dc1/my.jpg"));//删除文件

删除成功返回True，否则抛出异常。注意删除目录时，必须保证目录为空 ，否则也会抛出异常。

<a name=detuyun-getdir></a>
### 6.获取目录文件列表
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

获取目录文件以及子目录列表。需要获取根目录列表是，使用 `string[] ss = strhtml.Split('\\');` ，或直接表用方法不传递参数。
目录获取失败则抛出异常。

<a name=detuyun-getfile></a>
### 7.获取文件信息
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
                ht.Add("name",info[0]);
                ht.Add("type", info[1]);
                ht.Add("size", info[2]);
                ht.Add("date", info[3]);
               
            }
            catch (Exception)
            {
                ht = new Hashtable();
            }
            return ht;
        }

获取文件信息时通过Tab键分隔获取相应内容，返回结果为一个数组。

<a name=detuyun-getused></a>
###8.获取空间使用状况

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

示例代码：

```
var s = detuyun.getFolderUsage("/");
```
返回的结果为空间使用量，单位 kb

返回的结果为空间使用量，单位 ***kb***

<a name=detuyun-exception></a>
### 异常处理
当API请求发生错误时，SDK将抛出异常，具体错误代码请参考 <a target="_blank"  href="http://www.detuyun.com/docs/page6.html">标准API错误代码表</a>

根据返回HTTP CODE的不同，SDK将抛出以下异常：

* **DetuYunAuthorizationException** 401，授权错误
* **DetuYunForbiddenException** 403，权限错误
* **DetuYunNotFoundException** 404，文件或目录不存在
* **DetuYunNotAcceptableException** 406， 目录错误
* **DetuYunServiceUnavailable** 503，系统错误

未包含在以上异常中的错误，将统一抛出 `DetuYunException` 异常。
