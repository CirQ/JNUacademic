using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace JNUacademic {
    public partial class LoginForm : Form {
        private readonly string netError =
            "Network error, please check your network state or try again later.";
        private readonly string host = "http://jwxt.jnu.edu.cn";
        private CookieContainer cookies = null;
        private HttpWebRequest request = null;
        private HttpWebResponse response = null;
        private string cachePage = null;

        private string localStuNum = "";
        private string localPw = "";
		// da
        public LoginForm() {
            cookies = new CookieContainer();
            InitializeComponent();
            PrivateFontCollection font = new PrivateFontCollection();
            font.AddFontFile(Application.StartupPath + @"\RTWSYueRoudGoDemo-Regular.otf");
            this.Font = new Font(font.Families[0].Name, 10.8F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(134)));
        }

        private void FormLoad(object sender, EventArgs e) {
            try {
                // load local file and read the saved number and password
                try {
                    using(StreamReader sr = new StreamReader("data.ini", Encoding.UTF32)) {
                        localStuNum = Decrypt(sr.ReadLine());
                        localPw = Decrypt(sr.ReadLine());
                        stuNumTxt.Text = localStuNum;
                        pwTxt.Text = localPw;
                    }
                } catch(FileNotFoundException) {
                    FileStream fs = new FileStream("data.ini", FileMode.CreateNew);
                    fs.Close();
                }

                OnLoad();
                GetCaptchaImage();
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                this.Close();
            }
        }

        private void capImgClick(object sender, EventArgs e) {
            try {
                GetCaptchaImage();
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private void loginBtnClick(object sender, EventArgs e) {
            try {
                if(CheckLoginForm() == true) {
                    if(LoginRequest() == true) {
                        // if the number and password distinct to that local, ask whether to save
                        if(!stuNumTxt.Text.Equals(localStuNum) || !pwTxt.Text.Equals(localPw)) {
                            DialogResult dr = MessageBox.Show("Save student number and password?", "SAVE?", MessageBoxButtons.OKCancel);
                            if(dr == DialogResult.OK) {
                                try {
                                    using(StreamWriter sw = new StreamWriter("data.ini", false, Encoding.UTF32)) {
                                        sw.WriteLine(Encrypt(stuNumTxt.Text));
                                        sw.WriteLine(Encrypt(pwTxt.Text));
                                    }
                                } catch { }
                            }
                        }

                        this.Hide();
                        new SelectForm(cookies, stuNumTxt.Text).ShowDialog();  
                        // 这样打开新窗口后，原窗口会被阻塞，直到关闭新窗口才执行下一行代码
                        //Application.ExitThread();  // 使Hide的窗口退出，与下一行作用相同。若没有，原窗口会作为后台线程一直存在
                        this.Close();
                    }
                    else {
                        banButtonTimer.Start();
                        loginBtn.Enabled = false;
                        loginBtn.Text = "Login (" + (waitTime - currentCount) + "s)";
                        Match mError = Regex.Match(cachePage, "alert\\('(.*)'\\);//");
                        string Error = mError.Groups[1].Value.ToString();
                        MessageBox.Show(Error, "ERROR", MessageBoxButtons.OK,
                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }
                }
                else {
                    string frmError =
                        "Login information error, please check:\n" +
                        "Student number should be 10 digit numbers,\n" +
                        "Password should less than 20 characters,\n" +
                        "Captcha should be 4 digits or letters.";
                    MessageBox.Show(frmError, "ERROR", MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
        private void cancelBtnClick(object sender, EventArgs e) {
            this.Close();
        }

        private int currentCount = 0;
        private const int waitTime = 5;
        private void banButtonTimerTick(object sender, EventArgs e) {
            currentCount += 1;
            loginBtn.Text = "Login (" + (waitTime - currentCount) + "s)";
            if(currentCount > waitTime) {
                banButtonTimer.Stop();
                loginBtn.Text = "Login";
                currentCount = 0;
                loginBtn.Enabled = true;
            }
        }

        private void Formclose(object sender, FormClosedEventArgs e) {
            try {
                FileInfo fi = new FileInfo("data.ini");
                if (fi.Length == 0) {
                    File.Delete("data.ini");
                }
            } catch {}
        }

        /// <summary>
        ///   First Login to obtain cookie and cache the page
        /// </summary>
        private void OnLoad() {
            string uri = "/Login.aspx";
            request = WebRequest.Create(host + uri) as HttpWebRequest;
            request.CookieContainer = cookies;
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:51.0) Gecko/20100101 Firefox/51.0";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8,zh-CN;q=0.5,zh;q=0.3");
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            request.KeepAlive = true;
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            request.Headers.Add("Cache-Control", "max-age=0");

            response = request.GetResponse() as HttpWebResponse;
            using(Stream stream = response.GetResponseStream()) {
                StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("GB2312"));
                cachePage = reader.ReadToEnd();
                reader.Close();
            }
            request.Abort();
            response.Close();
        }

        /// <summary>
        ///   After Login then request the captcha
        /// </summary>
        private void GetCaptchaImage() {
            string uri = "/ValidateCode.aspx";
            request = WebRequest.Create(host + uri) as HttpWebRequest;
            request.CookieContainer = cookies;
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:51.0) Gecko/20100101 Firefox/51.0";
            request.Accept = "*/*";
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8,zh-CN;q=0.5,zh;q=0.3");
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            request.Referer = "http://jwxt.jnu.edu.cn/Login.aspx";
            request.KeepAlive = true;
            request.Headers.Add("Cache-Control", "max-age=0");

            response = request.GetResponse() as HttpWebResponse;
            using(Stream stream = response.GetResponseStream()) {
                byte[] buffer = new byte[response.ContentLength];

                /** A stronger version to read response stream
                int offset = 0, actuallyRead = 0;
                do {
                    actuallyRead = stream.Read(buffer, offset, buffer.Length - offset);
                    offset += actuallyRead;
                } while(actuallyRead > 0);
                */
                stream.Read(buffer, 0, buffer.Length);  // Naive version

                MemoryStream ms = new MemoryStream(buffer);
                Bitmap captcha = new Bitmap(ms);
                ms.Close();
                request.Abort();
                response.Close();
                capImg.Image = captcha as Image;
            }
        }

        /// <summary>
        ///   Check whether the login information is in correct format
        /// </summary>
        private bool CheckLoginForm() {
            return new Regex("^\\d{10}$").IsMatch(stuNumTxt.Text) &&
                   new Regex(".{1,20}").IsMatch(pwTxt.Text) &&
                   new Regex("^[A-Za-z0-9]{4}$").IsMatch(capTxt.Text);
        }

        /// <summary>
        ///   Login into the IndexPage
        /// </summary>
        private bool LoginRequest() {
            string uri = "/Login.aspx";
            request = WebRequest.Create(host + uri) as HttpWebRequest;
            request.CookieContainer = cookies;
            request.Method = "POST";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:51.0) Gecko/20100101 Firefox/51.0";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8,zh-CN;q=0.5,zh;q=0.3");
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            request.Referer = "http://jwxt.jnu.edu.cn/Login.aspx";
            request.KeepAlive = true;
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            string requestContent = GetLoginRequestContent(cachePage);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = requestContent.Length;
            using(Stream stream = request.GetRequestStream()) {
                byte[] postContent = Encoding.ASCII.GetBytes(requestContent);
                stream.Write(postContent, 0, postContent.Length);
            }

            response = request.GetResponse() as HttpWebResponse;
            using(Stream stream = response.GetResponseStream()) {
                StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("GB2312"));
                cachePage = reader.ReadToEnd();
                reader.Close();
            }
            request.Abort();
            response.Close();
            return cachePage.Contains("areaMiddle.aspx");
        }

        /// <summary>
        ///   Obtain the login post data string
        /// </summary>
        private delegate string GetParameterValue(string s);
        private string GetLoginRequestContent(string page) {
            GetParameterValue getValue = (s) => {
                string ms = Regex.Match(page, "<input(.*?)" + s + "(.*?)value=\"(.*?)\" />").Groups[3].Value;
                return HttpUtility.UrlEncode(ms, Encoding.GetEncoding("GB2312"));
            };
            string __VIEWSTATE = getValue("__VIEWSTATE");
            string __VIEWSTATEGENERATOR = getValue("__VIEWSTATEGENERATOR");
            string __EVENTVALIDATION = getValue("__EVENTVALIDATION");

            string requestContent = string.Format("__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__EVENTVALIDATION={2}&" +
                "txtYHBS={3}&txtYHMM={4}&txtFJM={5}&btnLogin=%B5%C7++++%C2%BC", __VIEWSTATE, __VIEWSTATEGENERATOR,
                __EVENTVALIDATION, stuNumTxt.Text, pwTxt.Text, capTxt.Text);
            return requestContent;
        }

        #region For encryption/decryption
        public string Encrypt(string plain) {
            byte[] key = Encoding.UTF8.GetBytes("youdontknowthepowerofthedarkside");
            byte[] plainBytes = Encoding.UTF8.GetBytes(plain);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = key;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform ictf = rDel.CreateEncryptor();
            byte[] result = ictf.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(result);
        }
        public string Decrypt(string cipher) {
            byte[] key = Encoding.UTF8.GetBytes("youdontknowthepowerofthedarkside");
            byte[] cipherBytes = Convert.FromBase64String(cipher);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = key;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform ictf = rDel.CreateDecryptor();
            byte[] result = ictf.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(result);
        }
        #endregion
        
    }
}
