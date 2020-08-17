using System;
using System.Collections.Generic;

using SqlServerHelper.Core;

namespace CompareTable.Template
{
    public partial class ClassModelTemplate
    {
        /// <summary>
        /// Gets or sets the area.
        /// </summary>
        /// <value>
        /// The area.
        /// </value>
        private String Area { set; get; }

        private String ClassName { set; get; }

        /// <summary>
        /// Gets or sets the table information.
        /// </summary>
        /// <value>
        /// The table information.
        /// </value>
        private List<SqlServerDBColumnInfo> SqlServerDBColumnList { set; get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="DataProviderInterfaceTemplate"/> class.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="tableInfo">The table information.</param>
        public ClassModelTemplate(String area, string className, List<SqlServerDBColumnInfo> sqlServerDBColumnList)
        {
            Area = area;
            ClassName = className;
            SqlServerDBColumnList = sqlServerDBColumnList;
        }
    }

    public partial class SqlDataTemplate
    {
        private Dictionary<int, Dictionary<string, string>> DataList { set; get; }

        private List<SqlServerDBColumnInfo> DataColList { set; get; }

        public SqlDataTemplate(List<SqlServerDBColumnInfo> dataColList, Dictionary<int, Dictionary<string, string>> dataList)
        {
            DataColList = dataColList;

            DataList = dataList;
        }
    }

    public partial class InitDataTemplate
    {
        private Dictionary<int, Dictionary<string, string>> DataList { set; get; }

        private List<SqlServerDBColumnInfo> DataColList { set; get; }

        public InitDataTemplate(List<SqlServerDBColumnInfo> dataColList, Dictionary<int, Dictionary<string, string>> dataList)
        {
            DataColList = dataColList;

            DataList = dataList;
        }
    }

    public partial class ClassDataTemplate
    {
        private Dictionary<int, Dictionary<string, string>> DataList { set; get; }


        public ClassDataTemplate(Dictionary<int, Dictionary<string, string>> dataList)
        {
            DataList = dataList;
        }
    }

    public partial class ClassJsonTemplate
    {
        private Dictionary<int, Dictionary<string, string>> DataList { set; get; }


        public ClassJsonTemplate(Dictionary<int, Dictionary<string, string>> dataList)
        {
            DataList = dataList;
        }
    }

    public partial class BackupTemplate
    {
        private List<SqlServerDBColumnInfo> DataColList { set; get; }

        public BackupTemplate(List<SqlServerDBColumnInfo> dataColList)
        {
            DataColList = dataColList;
        }
    }

    public partial class GetTableToTemp
    {
        private List<SqlServerDBColumnInfo> DataColList { set; get; }

        public GetTableToTemp(List<SqlServerDBColumnInfo> dataColList)
        {
            DataColList = dataColList;
        }
    }


    #region ENumsTemplate

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ModelGenerator.ModelTemplate.ENumsTemplate" />
    public partial class ENumsTemplate
    {
        private List<PROMCodeTypes> TypeList { set; get; }

        private Dictionary<int, List<PROMCodes>> CodeList { set; get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ENumsTemplate"/> class.
        /// </summary>
        /// <param name="tableInfo">The table information.</param>
        public ENumsTemplate(List<PROMCodeTypes> typeList, Dictionary<int, List<PROMCodes>> codeList)
        {
            TypeList = typeList;
            CodeList = codeList;
        }
    }

    #endregion
}
