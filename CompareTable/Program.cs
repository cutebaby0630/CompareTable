using CompareTable.Template;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SqlServerHelper.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace CompareTable
{

    class Program
    {
        static void Main(string[] args)
        {
            var fliepath = $@"C:\Users\v-vyin\SchedulerDB_ExcelFile\{"CompareTable" + DateTime.Now.ToString("yyyyMMddhhmm")}";
            Directory.CreateDirectory(fliepath);
            //Step1. 從182取出table 放入#tabl
            //Step1.1 連線到225.17
            //string[] tablenames = new string[] { "CHGMChargeItem", "ODRMPackageOrder" };
            string Mailto = null;
            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsetting.json", optional: true, reloadOnChange: true).Build();

            string connString_default = config.GetConnectionString("DefaultConnection");
            string connString_prod = config.GetConnectionString("PRODConnection");
            string[] tablenames = config[$"TargetTable:Tables"].Split(",");

            List<CompareTables> compareTables = new List<CompareTables>();
            foreach (string tablename in tablenames)
            {
                bool tablename_CHG = tablename.Contains("CHG");
                bool tablename_CLA = tablename.Contains("CLA");
                string dbname = "HISDB";
                if (tablename_CHG || tablename_CLA)
                {
                    dbname = "HISBILLINGDB";
                }
                SqlServerDBHelper sqlHelper = new SqlServerDBHelper(string.Format(connString_default, dbname, "msdba", "1qaz@wsx"));
                SqlServerTableHelper sqltablehelper = new SqlServerTableHelper(string.Format(connString_prod, dbname, "msdba", "1qaz@wsx"));
                List<SqlServerDBColumnInfo> tableList = sqltablehelper.FillTableList(tablename).FillColumnList().GetTableList().First().SqlServerDBColumnList;
                for (int x = 0; x <= tableList.Count - 1; x++)
                {
                    if (tableList[x].DataTypeName == "BIT()")
                    {
                        tableList[x].DataTypeName = "BIT";
                    }
                }
                //Step1.2 取出222.182相對應table放入#table
                GetTableToTemp getTableToTemp = new GetTableToTemp(tableList, dbname);
                string getTableToTemp_sql = getTableToTemp.TransformText();
                DataTable compare_dt = sqlHelper.FillTableAsync(getTableToTemp_sql).Result;

                //Step3. 覆蓋
                //Step3.1 #table覆蓋SKHDBA中的table
                OverrideTable overrideTable = new OverrideTable(tableList, dbname);
                string overrideTable_sql = overrideTable.TransformText();
                DataTable resault_dt = sqlHelper.FillTableAsync(overrideTable_sql).Result;

                compareTables.Add(new CompareTables()
                {
                    tableName = tablename,
                    compareTable = compare_dt,
                    resultTable = resault_dt,
                    toMail = $"{config[$"TargetTable:{tablename}:toMail"]}",
                    ccMail = $"{config[$"TargetTable:{tablename}:ccMail"]}"

                });
            }

            var importDBData = new ImportDBData();


            //ExcelPackage.LicenseContext = LicenseContext.Commercial;
            for (int count = 0; count <= compareTables.Count - 1; count++)
            {
                var excelname = new FileInfo(compareTables[count].tableName +"_" + DateTime.Now.ToString("yyyyMMddhhdd") + ".xlsx");
                //ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var excel = new ExcelPackage(excelname))
                {
                        //Step 3.將對應的List 丟到各Sheet中
                        ExcelWorksheet sheet = excel.Workbook.Worksheets.Add(compareTables[count].tableName);
                        //抽function
                        int rowIndex = 2;
                        int colIndex = 1;
                        importDBData.ImportData(compareTables[count].compareTable, sheet, rowIndex, colIndex, compareTables[count].tableName);
                    
                    // Step 4.Export EXCEL
                    Byte[] bin = excel.GetAsByteArray();
                    File.WriteAllBytes(fliepath.ToString() + @"\" + excelname, bin);

                }

                //Step2. #table跟225.17中比較
                //Step2.1 compare 兩個table
                //Step2.2 將結果發Email
                var helper = new SMTPHelper("lovemath0630@gmail.com", "koormyktfbbacpmj", "smtp.gmail.com", 587, true, true); //寄出信email
                string subject = $"{compareTables[count].tableName}表格異動 {DateTime.Now.ToString("yyyyMMdd")}"; //信件主旨
                string body = $"Hi All, \r\n\r\n{compareTables[count].tableName}_{DateTime.Now.ToString("yyyyMMdd")} 表格異動如附件，\r\n\r\n Best Regards, \r\n\r\n Vicky Yin";//信件內容
                string attachments = null;//附件
                var fileName = fliepath + @"\" + excelname;//附件位置
                if (File.Exists(fileName.ToString()))
                {
                    attachments = fileName.ToString();
                }
                string toMailList = compareTables[count].toMail;//收件者
                string ccMailList = compareTables[count].ccMail;//CC收件者

                helper.SendMail(toMailList, ccMailList, null, subject, body, attachments);
            }


        }
    }

    public class CompareTables
    {
        public string tableName { get; set; }
        public DataTable resultTable { get; set; }
        public DataTable compareTable { get; set; }
        public string toMail { get; set; }
        public string ccMail { get; set; }
    }
    #region -- Data to excel
    public class ImportDBData
    {
        private ExcelWorksheet _sheet { get; set; }
        private int _rowIndex { get; set; }
        private int _colIndex { get; set; }
        private DataTable _dt { get; set; }
        public void ImportData(DataTable dt, ExcelWorksheet sheet, int rowIndex, int colIndex, string tablename)
        {
            _sheet = sheet;
            _rowIndex = rowIndex;
            _colIndex = colIndex;
            _dt = dt;
            _sheet.Cells[_rowIndex - 1, _colIndex].Value = "返回目錄";
            _sheet.Cells[_rowIndex - 1, _colIndex].SetHyperlink(new Uri($"#'目錄'!A1", UriKind.Relative));

            //3.1塞columnName 到Row 
            for (int columnNameIndex = 0; columnNameIndex <= _dt.Columns.Count - 1; columnNameIndex++)
            {
                _sheet.Cells[_rowIndex, _colIndex++].Value = (_dt.Columns[columnNameIndex].ColumnName == null ? string.Empty : _dt.Columns[columnNameIndex].ColumnName);
            }
            _sheet.Cells[_rowIndex, 1, _rowIndex, _colIndex - 1]
                 .SetQuickStyle(Color.Black, Color.LightPink, ExcelHorizontalAlignment.Center);
            if (_sheet.ToString() == tablename)
            {
                //將對應值放入
                foreach (DataRow row in _dt.Rows)
                {
                    _rowIndex++;
                    _colIndex = 1;
                    for (int num = 0; num <= _dt.Columns.Count - 1; num++)
                    {
                        _sheet.Cells[_rowIndex, _colIndex++].Value = row[num].ToString();
                    }
                }
            }



            //Autofit
            int startColumn = _sheet.Dimension.Start.Column;
            int endColumn = _sheet.Dimension.End.Column;
            for (int count = startColumn; count <= endColumn; count++)
            {
                _sheet.Column(count).AutoFit();
            }


        }
        public void GenFirstSheet(ExcelPackage excel, string[] list)
        {
            int rowIndex = 1;
            int colIndex = 1;

            int maxCol = 0;

            ExcelWorksheet firstSheet = excel.Workbook.Worksheets.Add("目錄");

            firstSheet.Cells[rowIndex, colIndex++].Value = "";
            firstSheet.Cells[rowIndex, colIndex++].Value = "異動表格";

            firstSheet.Cells[rowIndex, 1, rowIndex, colIndex - 1]
                .SetQuickStyle(Color.Black, Color.LightPink, ExcelHorizontalAlignment.Center);

            maxCol = Math.Max(maxCol, colIndex - 1);

            foreach (string info in list)
            {
                rowIndex++;
                colIndex = 1;

                firstSheet.Cells[rowIndex, colIndex++].Value = rowIndex - 1;
                firstSheet.Cells[rowIndex, colIndex++].Value = info;
                firstSheet.Cells[rowIndex, colIndex - 1].SetHyperlink(new Uri($"#'{(string.IsNullOrEmpty(info) ? "Blank" : info)}'!A1", UriKind.Relative));
            }

            for (int i = 1; i <= maxCol; i++)
            {
                firstSheet.Column(i).AutoFit();
            }
        }

    }
    #endregion
    #region -- excel cell style --
    public static class ExcelExtensions
    {
        // SetQuickStyle，指定前景色/背景色/水平對齊
        public static void SetQuickStyle(this ExcelRange range,
            Color fontColor,
            Color bgColor = default(Color),
            ExcelHorizontalAlignment hAlign = ExcelHorizontalAlignment.Left)
        {
            range.Style.Font.Color.SetColor(fontColor);
            if (bgColor != default(Color))
            {
                range.Style.Fill.PatternType = ExcelFillStyle.Solid; // 一定要加這行..不然會報錯
                range.Style.Fill.BackgroundColor.SetColor(bgColor);
            }
            range.Style.HorizontalAlignment = hAlign;
        }

        //讓文字上有連結
        public static void SetHyperlink(this ExcelRange range, Uri uri)
        {
            range.Hyperlink = uri;
            range.Style.Font.UnderLine = true;
            range.Style.Font.Color.SetColor(Color.Blue);
        }
    }
    #endregion
}
