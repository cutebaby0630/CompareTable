using CompareTable.Template;
using Microsoft.Extensions.Configuration;
using SqlServerHelper.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CompareTable
{
    class Program
    {
        static void Main(string[] args)
        {
            //Step1. 從182取出table 放入#tabl
            //Step1.1 連線到225.17
            var tablename = "CHGMChargeItem";
            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsetting.json", optional: true, reloadOnChange: true).Build();
            string connString = config.GetConnectionString("DefaultConnection");
            SqlServerDBHelper sqlHelper = new SqlServerDBHelper(string.Format(connString, "HISBILLINGDB", "msdba", "1qaz@wsx"));
            SqlServerTableHelper sqltablehelper = new SqlServerTableHelper(string.Format(connString, "HISBILLINGDB", "msdba", "1qaz@wsx"));
            List<SqlServerDBColumnInfo> tableList = sqltablehelper.FillTableList($"{tablename}").FillColumnList().GetTableList().First().SqlServerDBColumnList;
            for (int x = 0; x <= tableList.Count - 1; x++)
            {
                if (tableList[x].DataTypeName == "BIT()")
                {
                    tableList[x].DataTypeName = "BIT";
                }
            }
            //Step1.2 取出222.182相對應table放入#table
            GetTableToTemp getTableToTemp = new GetTableToTemp(tableList);
            string getTableToTemp_sql = getTableToTemp.TransformText();
            DataTable compare_dt = sqlHelper.FillTableAsync(getTableToTemp_sql).Result;
            //Step2. #table跟225.17中比較
            //Step2.1 compare 兩個table
            //Step2.2 將結果發Email
            DatatableToHTML datatableToHTML = new DatatableToHTML();
            var helper = new SMTPHelper("lovemath0630@gmail.com", "koormyktfbbacpmj", "smtp.gmail.com", 587, true, true); //寄出信email
            string subject = $"Initial Data異動 {DateTime.Now.ToString("yyyyMMdd")}"; //信件主旨
            string body = $"Hi All, \r\n\r\n{DateTime.Now.ToString("yyyyMMdd")} {tablename}.csv更改如下表，\r\n\r\n{(datatableToHTML.ToHTML(compare_dt) == null ? string.Empty : datatableToHTML.ToHTML(compare_dt))}\r\n\r\n Best Regards, \r\n\r\n Vicky Yin";//信件內容
            string attachments = null;//附件
            /*var fileName = @"D:\微軟MCS\SchedulerDB_Excel\" + excelname;//附件位置
            if (File.Exists(fileName.ToString()))
            {
                attachments = fileName.ToString();
            }*/
            string toMailList = "v-vyin@microsoft.com";//收件者
            string ccMailList = "";//CC收件者

            helper.SendMail(toMailList, ccMailList, null, subject, body, null);
            //Step3. 覆蓋
            //Step3.1 #table覆蓋SKHDBA中的table
        }
    }
    #region -- DataTable to HTML--
    class DatatableToHTML
    {
        public string ToHTML(DataTable dt)
        {
            try
            {
                string html = "<table>";
                //add header row
                html += @"<tr style=""background - color:#DDDDDD;font-size:12;"">";
                for (int i = 0; i < dt.Columns.Count; i++)
                    html += @"<td style=""font - family: Tahoma; font - size; 10; "">" + dt.Columns[i].ColumnName + "</td>";
                html += "</tr>";
                //add rows
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    html += @"<tr style=""background - color:#DDDDDD;font-size:12;"">";
                    for (int j = 0; j < dt.Columns.Count; j++)
                        html += @"<td style=""font - family: Tahoma; font - size; 10; "">" + dt.Rows[i][j].ToString() + "</td>";
                    html += "</tr>";
                }
                html += "</table>";

                return html;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
    #endregion
    #region -- Send Email --
    public class SendEmail
    {
        public string _Message { get; set; }
        public string _tablename { get; set; }
        public DataTable _compareresult { get; set; }
        public void SendResultEmail(string _tablename, DataTable _compareresult)
        {
            DatatableToHTML datatableToHTML = new DatatableToHTML();
            var helper = new SMTPHelper("lovemath0630@gmail.com", "koormyktfbbacpmj", "smtp.gmail.com", 587, true, true); //寄出信email
            string subject = $"Initial Data異動 {DateTime.Now.ToString("yyyyMMdd")}"; //信件主旨
            string body = $"Hi All, \r\n\r\n{DateTime.Now.ToString("yyyyMMdd")} {_tablename}.csv更改如下表，\r\n\r\n{(datatableToHTML.ToHTML(_compareresult) == null ? string.Empty : datatableToHTML.ToHTML(_compareresult))}\r\n\r\n Best Regards, \r\n\r\n Vicky Yin";//信件內容
            string attachments = null;//附件
            /*var fileName = @"D:\微軟MCS\SchedulerDB_Excel\" + excelname;//附件位置
            if (File.Exists(fileName.ToString()))
            {
                attachments = fileName.ToString();
            }*/
            string toMailList = "lovemath0630@gmail.com;v-vyin@microsoft.com";//收件者
            string ccMailList = "";//CC收件者

            helper.SendMail(toMailList, ccMailList, null, subject, body, null);

        }
    }
        #endregion
    }
