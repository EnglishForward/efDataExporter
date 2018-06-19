using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace efDataExtporter
{
    public class DataConverter
    {
        /// <summary>
        /// convert datatable to csv
        /// </summary>
        /// <param name="xData">data</param>
        /// <param name="xWithHeader">true if headers to be displayed</param>
        /// <returns>csv string</returns>
        public static string ConvertToCSV(DataTable xData, bool xWithHeader = false )
        {
            StringBuilder sbCSV = new StringBuilder();
            StringBuilder sbCSVLine = new StringBuilder();

            if ((xData != null))// && (xData.Rows.Count > 0)
            {
                if (xWithHeader)
                {
                    int colCount = 0;
                    //header required so loop through all the column names
                    foreach (DataColumn Column in xData.Columns)
                    {
                        if (!Column.ColumnName.StartsWith("_"))
                        {
                            if (colCount > 0)
                            {
                                sbCSV.Append(",");
                            }

                            sbCSV.Append(Column.ColumnName);
                            colCount++;
                        }
                    }

                    //add new line
                    sbCSV.Append(Environment.NewLine);
                }

                //loop through data

                int xLoopCounter = 0;
                foreach (DataRow Row in xData.Rows)
                {
                    sbCSVLine.Clear();
                    foreach (DataColumn Column in xData.Columns)
                    {
                        if (!Column.ColumnName.StartsWith("_"))
                        {
                            if (sbCSVLine.Length > 0)
                            {
                                sbCSVLine.Append(",");
                            }


                            sbCSVLine.Append(Row[Column].ToString().Replace(',', ' ').Replace(" 00:00:00", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", ""));
                        }
                    }

                        sbCSVLine.Append(Environment.NewLine);

                        sbCSV.Append(sbCSVLine);

                    xLoopCounter++;
                }
            }

            return sbCSV.ToString().TrimEnd(Environment.NewLine.ToCharArray());
        }

        /// <summary>
        /// convert datatable to html
        /// </summary>
        /// <param name="xData">data</param>
        /// <returns>csv string</returns>
        public static string ConvertToHTML(DataTable xData)
        {
            HtmlConverter htmlConverter = new HtmlConverter();
            return htmlConverter.Convert(xData);
        }
    }
}
