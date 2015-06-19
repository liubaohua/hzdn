﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Reflection;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop;
using System.Runtime.InteropServices;
using System.IO.Ports;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Threading;
//using Microsoft.Office.Core;
using Microsoft.Office;

namespace Print
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            InitDatabaseSetting();
            
           // tbFinishTime.Text = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
            LoadExcelTabs();
            
            
            //EnableItems();
        }
        private String CurrentBarcode = null;

        private string cinvaliascode = "";
        private string barcodestd = "";
        private string custinvcode = "";

        String XMLFILENAME = "UserData.xml";

        private void InitDatabaseSetting()
        {
            if (!File.Exists(XMLFILENAME))
                WriteXml();
            string ConnString = "data source=.;user id=sa;password=ufida123456;initial catalog=UFDATA_009_2015;Connect Timeout=10;Persist Security Info=True ;Current Language=Simplified Chinese;";
            string Server = ReadXmlData("SqlServer", "Server");
            string User = ReadXmlData("SqlServer", "User");
            string Password = ReadXmlData("SqlServer", "Password");
            string DataBase = ReadXmlData("SqlServer", "DataBase");
            ConnString = "data source=" + RC4.Decrypt("1",Server) + ";";
            ConnString += "user id=" + RC4.Decrypt("1",User) + ";";
            ConnString += "password=" + RC4.Decrypt("1",Password) + ";";
            ConnString += "initial catalog=" + RC4.Decrypt("1",DataBase) + ";";
            ConnString += "Connect Timeout=10;Persist Security Info=True ;Current Language=Simplified Chinese;";
            this.sqlConnection1.ConnectionString = ConnString;
        }

        String ReadXmlData(String ElementName,String ElementName2)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XMLFILENAME);
            XmlNode root = doc.DocumentElement[ElementName];
            if (root != null && root.SelectSingleNode(ElementName2)!=null)
                return root.SelectSingleNode(ElementName2).InnerText;
            return "";
        }

        String ReadXmlData(String ElementName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XMLFILENAME);
            XmlNode root = doc.DocumentElement[ElementName];
            if (root != null)
                return root.InnerText;
            return "";
        }

        void ModifyXml(UserData ud)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XMLFILENAME);
            XmlNode root = doc.DocumentElement[ud.ElementName];
            List<NameValuePair> udList = ud.ValueList;
            if(udList!=null)
            {
                for (int i = 0; i <= udList.Count - 1; i++)
                    root.SelectSingleNode(udList[i].Name).InnerText = udList[i].Value;
            }
            doc.Save(XMLFILENAME);
        }

        void ModifyXml(String Name, String Value)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XMLFILENAME);
            XmlNode root = doc.DocumentElement[Name];
            if (root != null)
            {
                root.InnerText = Value;
                doc.Save(XMLFILENAME);
            }
        }


        void WriteXml()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            XmlWriter writer = XmlWriter.Create(XMLFILENAME, settings);
            writer.WriteStartDocument();
            writer.WriteComment("This file is generated by the program.");
            writer.WriteStartElement("Information");
            writer.WriteElementString("ScalePort", "");
            writer.WriteElementString("isLog", "");
            writer.WriteElementString("DivisionFactor", "1000");
            writer.WriteElementString("Template", "");
            writer.WriteStartElement("SqlServer");
            writer.WriteElementString("Server", "");
            writer.WriteElementString("User", "");
            writer.WriteElementString("Password", "");
            writer.WriteElementString("DataBase", "");
            writer.WriteEndElement();
            writer.WriteStartElement("PrinterSetting");
            writer.WriteElementString("PrinterName", "");
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Flush();
            writer.Close();

        }

        bool isReadScaleStarted = false;//读电子称线程是否已经启动，只需执行一次
        string isLog = "";
        float DivisionFactor = 1000;

        private void LoadExcelTabs()
        {
            tbGross.BackColor = Color.FromKnownColor(KnownColor.Control);
            tbNet.BackColor = Color.FromKnownColor(KnownColor.Control);
            tbTare.BackColor = Color.FromKnownColor(KnownColor.Control);

            ComboxItem cb = new ComboxItem();
            cb.Value = "A4";
            cb.Text = "镀膜成品重量标签（40*20）";
            cbLabelNo.Items.Add(cb);

            cb = new ComboxItem();
            cb.Value = "A5";
            cb.Text = "镀膜拉丝/镀膜成品重量标签（70*22）";
            cbLabelNo.Items.Add(cb);

            cb = new ComboxItem();
            cb.Value = "A1";
            cb.Text = "导体绞线重量标签（70*90）";
            cbLabelNo.Items.Add(cb);

            cb = new ComboxItem();
            cb.Value = "A2";
            cb.Text = "导体镀锡拉丝重量标签（45*65）";
            cbLabelNo.Items.Add(cb);

            cb = new ComboxItem();
            cb.Value = "A3";
            cb.Text = "导体镀锡拉丝重量标签（40*28）";
            cbLabelNo.Items.Add(cb);

            cb = new ComboxItem();
            cb.Value = "A6";
            cb.Text = "复膜成品重量标签";
            cbLabelNo.Items.Add(cb);

            cb = new ComboxItem();
            cb.Value = "A7";
            cb.Text = "导体绞线重量标签（45*65）";
            cbLabelNo.Items.Add(cb);


            cbLabelNo.DropDownStyle = ComboBoxStyle.DropDownList;// not allowed to enter value

            cb = new ComboxItem();
            cb.Value = "COM1";
            cb.Text = "COM1";
            cbPort.Items.Add(cb);
            
            cb = new ComboxItem();
            cb.Value = "COM2";
            cb.Text = "COM2";
            cbPort.Items.Add(cb);

            cb = new ComboxItem();
            cb.Value = "COM3";
            cb.Text = "COM3";
            cbPort.Items.Add(cb);

            cb = new ComboxItem();
            cb.Value = "COM4";
            cb.Text = "COM4";
            cbPort.Items.Add(cb);

            cb = new ComboxItem();
            cb.Value = "COM5";
            cb.Text = "COM5";
            cbPort.Items.Add(cb);

            cbPort.DropDownStyle = ComboBoxStyle.DropDownList;
            

            try
            {
                if (!File.Exists(XMLFILENAME))
                    WriteXml();
                else
                {
                    string portindex = ReadXmlData("ScalePort");
                    string templateindex = ReadXmlData("Template");
                    
                    if (portindex != null && portindex.Length > 0)
                        cbPort.SelectedIndex = int.Parse(portindex);
                    if (templateindex != null && templateindex.Length > 0)
                        cbLabelNo.SelectedIndex = int.Parse(templateindex);
                }
                isLog = ReadXmlData("isLog");
                DivisionFactor = float.Parse(ReadXmlData("DivisionFactor"));
                //cbUnit.SelectedIndex = int.Parse(ReadXmlData("UnitSelIdx"));
                //TextReader tr = new StreamReader("port");
                //string portindex = tr.ReadLine();
                //tr.Close();
                //if(portindex!=null && portindex.Length>0)
                //    cbPort.SelectedIndex = int.Parse(portindex);
                
                
            
            }catch(Exception)
            {
                //MessageBox.Show(e11.Message);
            }
        }

        private void EnableItems()
        {
            tbCustCode.ReadOnly = false;
            tbProdNo.ReadOnly = false;
            tbGross.ReadOnly = false;
            tbTare.ReadOnly = false;
            tbNet.ReadOnly = false;
            tbMacNo.ReadOnly = false;
            tbProdTime.ReadOnly = false;
            tbBarcode.ReadOnly = false;
        }

        private object missing = Missing.Value;
//        private System.Data.SqlClient.SqlCommand sqlCommand1;
        private System.Data.SqlClient.SqlConnection sqlConnection1;



        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbLabelNo.Text == null || cbLabelNo.Text.Equals(""))
                    throw new Exception("未指定标签模板");
                if (CurrentBarcode == null || CurrentBarcode.Equals(""))
                    throw new Exception("产品条码信息为空");
                if(PrintExcel((cbLabelNo.SelectedItem as ComboxItem).Value.ToString()))
                    SaveData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(("y".Equals(isLog)?ex.StackTrace:ex.Message), "打印时错误");
            }
        }

        /// <summary>
        /// 获取条码档案
        /// </summary>
        private void FetchBarcodeInfo(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar== (char)13)
            try
            {
                ComboxItem ci = (cbPort.SelectedItem as ComboxItem);
                if (ci == null || ci.Value == null || ci.Value.ToString().Equals(""))
                {
                    MessageBox.Show("请选择电子称端口", "提示");
                    return;
                }
                SqlCommand cmdSelect = new SqlCommand("select i.cinvname,i.cinvcode,i.cInvAddCode as cinvaliascode,i.cinvstd,b.cdefine2 as ccuscode,b.cSrcCode as mocode,b.cdefine3 as ordercode,b.cdefine10 as vencode,b.cdefine6 as prodtime,b.cdefine4 as finishtime,b.cBarcodeDefine1 as macno,b.cBarcodeDefine2 as prodno,b.cDefine13 as custinvcode,b.cDefine14 as barcodestd,b.cdefine9 as axisbrand,b.cBarcodeDefine3 as staffno,b.cdefine16 as tare,b.cBarcodeDefine8 as totallen from HY_BarCodeMain b,Inventory i where b.Cinvcode=i.cinvcode and b.barcode='" + tbBarcode.Text + "'", this.sqlConnection1);
                //cmdSelect.Parameters.Add("@ID", SqlDbType.Int, 4);
                //cmdSelect.Parameters["@ID"].Value = InvCode
                this.sqlConnection1.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmdSelect);
                System.Data.DataTable dt = new System.Data.DataTable();
                da.Fill(dt);
                DisplayData(dt);
                if (!isReadScaleStarted)
                {
                    Thread t = new Thread(new ThreadStart(ReadDigitScale));
                    t.Start();
                    isReadScaleStarted = true;
                }
                CurrentBarcode = tbBarcode.Text;
                tbBarcode.Clear();
                tbBarcode.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"错误");
                CurrentBarcode = null;
                tbBarcode.Clear();
                tbBarcode.Focus();
            }
            finally
            {
                this.sqlConnection1.Close();
            }
        }

        private void DisplayData(System.Data.DataTable dt)
        {
            if (dt.Rows.Count > 0)
            {
                tbInvName.Text = dt.Rows[0]["cinvname"].ToString();
                tbProdTime.Text = dt.Rows[0]["prodtime"].ToString();
                tbInvStd.Text = dt.Rows[0]["cinvstd"].ToString();
                tbMoCode.Text = dt.Rows[0]["mocode"].ToString();
                dtFinishDate.Value = DateTime.Now;// ;
                //tbFinishTime.Text = dt.Rows[0]["finishtime"].ToString();
                tbInvCode.Text = dt.Rows[0]["cinvcode"].ToString();
                tbCustCode.Text = dt.Rows[0]["ccuscode"].ToString();
                tbMacNo.Text = dt.Rows[0]["macno"].ToString();
                tbOrderCode.Text = dt.Rows[0]["ordercode"].ToString();
                tbVenCode.Text = dt.Rows[0]["vencode"].ToString();
                tbStaffCode.Text = dt.Rows[0]["staffno"].ToString();
                tbAxisBrand.Text = dt.Rows[0]["axisbrand"].ToString();
                tbProdNo.Text = dt.Rows[0]["prodno"].ToString();
                //tbLabelNo.Text = dt.Rows[0]["ccuscode"].ToString();
                //tbBarcodeStd.Text = dt.Rows[0]["barcodestd"].ToString();
                barcodestd = dt.Rows[0]["barcodestd"].ToString();
                custinvcode = dt.Rows[0]["custinvcode"].ToString();
                cinvaliascode = dt.Rows[0]["cinvaliascode"].ToString();
                    //tbGross.Text = dt.Rows[0]["ccuscode"].ToString(); read from digital scale
                tbTare.Text = dt.Rows[0]["tare"].ToString();
                tbTotalLen.Text = dt.Rows[0]["totallen"].ToString();
                //tbPlanbillCode.Text = dt.Rows[0]["planbillcode"].ToString();//计划单号
                //tbNet.Text = dt.Rows[0]["net"].ToString();//毛重—皮重/轴重=净重
            }
            else
            {
                tbInvName.Text = "";
                tbProdTime.Text = "";
                tbInvStd.Text = "";
                tbMoCode.Text = "";
                //tbFinishTime.Text = "";
                tbInvCode.Text = "";
                tbCustCode.Text = "";
                tbMacNo.Text = "";
                tbOrderCode.Text = "";
                tbVenCode.Text = "";
                tbStaffCode.Text = "";
                tbAxisBrand.Text = "";
                tbProdNo.Text = "";
                barcodestd = "";
                custinvcode = "";
                cinvaliascode = "";
                //tbBarcodeStd.Text = "";
                //tbLabelNo.Text = dt.Rows[0]["ccuscode"].ToString();
                tbTotalLen.Text = "";
                tbGross.Text = "";// read from digital scale
                tbTare.Text = "";
                tbNet.Text = "";//毛重—皮重/轴重=净重

                //tbPlanbillCode.Text = "";//计划单号
            }

            
        }

        private DateTime FormatTime(string p)
        {
            return DateTime.Parse(p);
        }

        /// <summary>
        /// 保存条码档案
        /// </summary>
        private void SaveData()
        {
            float gross = tbGross.Text == null || tbGross.Text.Equals("") ? 0 : float.Parse(tbGross.Text);
            float net = tbNet.Text == null || tbNet.Text.Equals("") ? 0 : float.Parse(tbNet.Text);
            try
            {
                string date = "CONVERT(datetime, '" + String.Format("{0:yyyy-MM-dd}", dtFinishDate.Value) + "')";
                //string totoallen = "";
                //if (cbLabelNo.SelectedIndex == 2)
                //    totoallen = ",cBarcodeDefine8='" + tbTotalLen.ToString() + "'";
                this.sqlConnection1.Open();

                SqlCommand cmdSelect = new SqlCommand("update HY_BarCodeMain set isaleqty=" + gross + ",qty=" + net + " ,cdefine4= " + date + ",cdefine37= " + date + ",cdefine26=" + gross + " where barcode='" + CurrentBarcode + "'", this.sqlConnection1);
                //cmdSelect.Connection = this.sqlConnection1;
                int iresult = cmdSelect.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally {
                this.sqlConnection1.Close();            
            }
            
        }

        private bool PrintExcel(String LabelNo)
        {
            bool bExecuted = false;
            Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook wb = null;
            Microsoft.Office.Interop.Excel.Worksheet ws = null;
            try
            {
                wb = excelApp.Workbooks.Open(Directory.GetCurrentDirectory() + @"\条码模板.xls",
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                // Get the first worksheet.
                // (Excel uses base 1 indexing, not base 0.)
                for (int i = 1; i <= wb.Worksheets.Count; i++)
                {
                    Microsoft.Office.Interop.Excel.Worksheet ws1 = (Microsoft.Office.Interop.Excel.Worksheet)wb.Worksheets[i];
                    if (ws1.Name.Equals(LabelNo))
                    {
                        ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.Worksheets[LabelNo];
                        break;
                    }
                }
                if (ws == null)
                    throw new Exception("找不到EXCEL页签："+LabelNo);

                SetExcelCellsValues(wb, LabelNo);

                //ws.Shapes.AddPicture("C:\\ls0927-2.jpg", Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, 10, 10, 400, 100);
                
                //bool flag = excelApp.Dialogs[XlBuiltInDialog.xlDialogPrinterSetup].Show(
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing,
                //    Type.Missing
                //    );//显示对话框

                //if (flag == true)
                //{ 
                //}

                object ActivePrinter = Type.Missing;
                string PrinterName = ReadXmlData("PrinterSetting", "PrinterName");
                if (PrinterName != null && PrinterName.Length > 0)
                    ActivePrinter = PrinterName;
                //ws.PrintPreview(Type.Missing);
                ws.PrintOut(Type.Missing, Type.Missing, Type.Missing, Type.Missing, ActivePrinter, Type.Missing, Type.Missing, Type.Missing);
                bExecuted = true;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();

                Marshal.FinalReleaseComObject(ws);
                wb.Close(false, Type.Missing, Type.Missing);
                Marshal.FinalReleaseComObject(wb);

                excelApp.Quit();
                Marshal.FinalReleaseComObject(excelApp);
            }
            return bExecuted;
        }

        private object[] GetLabelTemplate001Data()
        {
            object[] obj = {
                           tbVenCode.Text,
                           tbCustCode.Text,
                           tbOrderCode.Text,
                           tbProdNo.Text,
                           String.Format("{0:yyyy-MM-dd}", dtFinishDate.Value),
                           tbMacNo.Text,
                           tbAxisBrand.Text,
                           tbStaffCode.Text,
                           tbGross.Text,
                           tbTare.Text,
                           tbNet.Text,
                           CurrentBarcode,
                           tbTotalLen.Text,
                           tbInvStd.Text
                           //DateTime.Now.ToString().Replace(" ", "").Replace(":", "").Replace("-", "").Substring(2)
                           };
            return obj;
        }

        private object[] GetLabelTemplate002Data()
        {
            object[] obj = {
                           tbVenCode.Text,
                           tbCustCode.Text,
                           tbOrderCode.Text,
                           tbProdNo.Text,
                           tbProdTime.Text,
                           tbGross.Text,
                           tbTare.Text,
                           tbNet.Text,
                           tbMacNo.Text,
                           tbStaffCode.Text,
                           CurrentBarcode,
                           tbInvStd.Text
                           };
            return obj;
        }

        private object[] GetLabelTemplate003Data()
        {
            object[] obj = {
                           tbCustCode.Text,
                           tbProdNo.Text,
                           tbGross.Text,
                           tbTare.Text,
                           tbNet.Text,
                           tbProdTime.Text,
                           CurrentBarcode
                           };
            return obj;
        }

        private object[] GetLabelTemplate004Data()
        {
            object[] obj = {
                           CurrentBarcode,
                           tbMoCode.Text,
                           //tbPlanbillCode.Text,
                           tbGross.Text,
                           tbTare.Text,
                           tbNet.Text
                           
                           };
            return obj;
        }


        /// <summary>
        /// 设置excel的值
        /// </summary>
        /// <param name="ws"></param>
        private void SetExcelCellsValues(Microsoft.Office.Interop.Excel.Workbook wb,String LabelNo)
        {
            Microsoft.Office.Interop.Excel.Worksheet ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.Worksheets["取数定义"];
            //Range r = ws.Cells[1];            
            int columnIndex = -1;
            for (int i = 1; i <=30; i++)
            {
                Range r = (ws.Cells[1,i] as Range);
                if (r!=null && r.get_Value(null)!=null && r.get_Value(null).ToString().Equals(LabelNo))
                {
                    columnIndex = i+1 ;
                    break;
                }
            }
            if (columnIndex == -1)
                throw new Exception("取数定义标签没有找到" + LabelNo + "相关数据");
            object[] datas = null;
            if (LabelNo.Equals("A1") || LabelNo.Equals("A7"))
                datas = GetLabelTemplate001Data();
            if (LabelNo.Equals("A2"))
                datas = GetLabelTemplate002Data();
            if (LabelNo.Equals("A3"))
                datas = GetLabelTemplate003Data();
            if (LabelNo.Equals("A4") || LabelNo.Equals("A5"))
                datas = GetLabelTemplate004Data();
            if(LabelNo.Equals("A6"))
                datas = GetLabelTemplate006Data();
            if (datas == null)
                throw new Exception("程序需要更新标签" + LabelNo + "");
            for(int i=0;i<datas.Length;i++)
                ws.Cells[2+i,columnIndex]=datas[i];
        }

        private object[] GetLabelTemplate006Data()
        {
            object[] obj = {
//客户料号
//物料号
//产品型号
//规格
//工号
//毛重
//净重
//轴重：
//生产编号
//生产日期
//条形码
                           
                           tbCustCode.Text,
//custinvcode ,
                           cinvaliascode,
                           //tbInvCode.Text,
                           tbInvStd.Text,
                           barcodestd,
                           tbStaffCode.Text,
                           tbGross.Text,
                           tbNet.Text,
                           tbTare.Text,
                           //tbAxisBrand.Text,
                           tbProdNo.Text,
                           tbProdTime.Text,
                            CurrentBarcode,
                           };
            return obj;
        }

        /// <summary>
        /// 读电子称信息
        /// </summary>
        private void ReadDigitScale()
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                    _serialPort.Close();
                if (_serialPort != null)
                    _serialPort.Dispose();

                _serialPort = new SerialPort((cbPort.SelectedItem as ComboxItem).Value.ToString(), 9600, Parity.None, 8, StopBits.One);
                _serialPort.Handshake = Handshake.None;
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);
                _serialPort.RtsEnable = true;
                _serialPort.DtrEnable = true;

                // Open the port for communications
                _serialPort.Open();
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "读电子称错误");
            }
            // Instantiate the communications
            // port with some basic settings
            //if (tbInvName.Text == null || tbInvName.Text.Equals(""))
              //  return;
            
            
        }

        private void SendCommand()
        {
            System.Configuration.AppSettingsReader configurationAppSettings = new System.Configuration.AppSettingsReader();
            string command = ((string)(configurationAppSettings.GetValue("Command", typeof(string))));
            if(command!=null&&command.Length>0)
                _serialPort.Write(command+"\r\n");//查询稳定的重量

        }

        public static string StringToBinary(string data)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in data.ToCharArray())
            {
                sb.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
            }
            return sb.ToString();
        }

        private SerialPort _serialPort;


       // private delegate void Closure();
        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort.BytesToRead > 0) //<-- repeats until the In-Buffer is empty
            {
                System.Threading.Thread.Sleep(1500);

                String data = _serialPort.ReadExisting();
                string[] op = { "" };//起始位 
                string[] arr = data.Split(op, StringSplitOptions.None);
                //MessageBox.Show("测试：缓冲区大小:"+size+"读取到的数据："+data);

                if (arr != null && arr.Length > 0)
                {
                    for (int i = arr.Length - 1; i >= 0; i--)
                    {
                        if ("y".Equals(isLog))
                       {
                        TextWriter tw = new StreamWriter("log.txt", true);
                        tw.Write(String.Format("{0:MM-dd}", DateTime.Now) + "\t" + arr[i] + "\r\n");
                        tw.Close();
                       }
                        if (arr[i].Length >= 11)
                        {
                            float tare = tbTare.Text == null || tbTare.Text.Equals("") ? 0 : float.Parse(tbTare.Text);
                            string data1 = arr[i].Substring(4, 6);
                            float weight = 0;
                            float.TryParse(data1.Trim(), out weight);
                            weight = weight / DivisionFactor;
                            //float weight = float.Parse(data.Trim()) / 100;
                            //tbGross.Text = weight.ToString();//毛重
                            //tbNet.Text = (weight - tare).ToString();//毛重—皮重/轴重=净重

                            this.tbGross.Text = weight.ToString();
                            this.tbNet.Text = (weight - tare).ToString();

                            SetWeights(weight.ToString(), (weight - tare).ToString());

                            //break;
                        }
                    }
                }
            }
        }

        delegate void SetTextCallback(string text, string text2);

        private void SetWeights(string p, string p_2)
        {
            if (this.tbGross.InvokeRequired && this.tbNet.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetWeights);
                //this.tbGross.Invoke(d, new object[]{ p});
                //this.tbNet.Invoke(d, new object[] { p_2 });
                this.Invoke(d, new object[] { p, p_2 });
            }
            else
            {
                this.tbGross.Text = p;
                this.tbNet.Text = p_2;
            }
        }



        /**//// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool PrintInit(string templetFile, string outputFile)
        {
            return true;
        }


        public void GcCollect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void CloseApp_Click(object sender, EventArgs e)
        {
            WritePortSelection();
            Environment.Exit(0);
        }

        private void ReadScale_Click(object sender, EventArgs e)
        {

        }

        public void FormClosed1(object sender, FormClosedEventArgs e)
        {
            WritePortSelection();
        }

        private void WritePortSelection()
        {
            UserData ud = new UserData();
            ud.ElementName = "Information";
            ud.NodeName = "ScalePort";

            ModifyXml("ScalePort", cbPort.SelectedIndex.ToString());
            ModifyXml("Template", cbLabelNo.SelectedIndex.ToString());
            //TextWriter tw = new StreamWriter("port");
            //tw.Write(cbPort.SelectedIndex);
            //tw.Close();
        }

        private void Config_Click(object sender, EventArgs e)
        {
            try
            {
                String PrinterName = ReadXmlData("PrinterSetting", "PrinterName");

                PrintDialog pd = new PrintDialog();
                if (PrinterName != null)
                    pd.PrinterSettings.PrinterName = PrinterName;
                DialogResult dr = pd.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    UserData ud = new UserData();
                    ud.ElementName = "PrinterSetting";
                    List<NameValuePair> list = new List<NameValuePair>();
                    NameValuePair pair = new NameValuePair();
                    pair.Name = "PrinterName";
                    pair.Value = pd.PrinterSettings.PrinterName;
                    list.Add(pair);
                    ud.ValueList = list;

                    ModifyXml(ud);
                    
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message,"错误");
            }

        }

        private void DbSetting_Click(object sender, EventArgs e)
        {
            DbSettingForm form = new DbSettingForm();
            DialogResult dr = form.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                UserData ud = new UserData();
                ud.ElementName = "SqlServer";
                List<NameValuePair> list = new List<NameValuePair>();
                NameValuePair np = new NameValuePair();
                np.Name = "Server";
                np.Value = RC4.Encrypt("1",form.getIP());
                list.Add(np);

                np = new NameValuePair();
                np.Name = "User";
                np.Value = RC4.Encrypt("1",form.getUser());
                list.Add(np);

                np = new NameValuePair();
                np.Name = "Password";
                np.Value = RC4.Encrypt("1",form.getPassword());
                list.Add(np);

                np = new NameValuePair();
                np.Name = "DataBase";
                np.Value = RC4.Encrypt("1", form.getDatabasename());
                list.Add(np);


                ud.ValueList = list;
                ModifyXml(ud);
                InitDatabaseSetting();
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control & e.KeyCode == Keys.S)//CTRL + S
                DbSetting_Click(null, null);
            if (e.Control & e.KeyCode == Keys.T)
                TestDb();
            if (e.Control & e.KeyCode == Keys.R)
                TestScale();

        }

        private void TestScale()
        {
            Thread t = new Thread(new ThreadStart(ReadDigitScale));
            t.Start();
        }

        private void TestDb()
        {
            try
            {
                SqlCommand cmdSelect = new SqlCommand("select 1 as mydata", this.sqlConnection1);
                //cmdSelect.Parameters.Add("@ID", SqlDbType.Int, 4);
                //cmdSelect.Parameters["@ID"].Value = InvCode
                this.sqlConnection1.Open();
                MessageBox.Show("数据库连接成功","提示");
            }
            catch
                (Exception e)
            {
                MessageBox.Show(e.Message, "提示");
            }
            finally
            {
                this.sqlConnection1.Close();
            }
            
        }
    }
}