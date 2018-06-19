using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace efDataExtporter
{
    public class HtmlConverter
    {
        public string cssTableStyle;
        public string cssTDStyle;
        public string cssTHStyle;
        public string cssTRStyle;
        public string cssTRAlternativeStyle;

        public HtmlConverter()
        {
            //wnDirect specifc table style
            cssTableStyle = "text-align:left;border:1px solid #ececec;border-collapse: collapse;font-family:Calibri;font-size:14px";
            cssTRAlternativeStyle = "background-color: #ececec;";
            cssTHStyle = "padding-right: 10px;text-align:left;background-color: #669ca5;font-weight: normal; color: #ffffff;";
            cssTDStyle = "padding-right: 10px;";
        }

        public string Convert(DataTable xData)
        {
            StringBuilder html = new StringBuilder("<html>" + Environment.NewLine);

            html.Append("<table style=\"" + cssTableStyle + "\">" + Environment.NewLine);
            html.Append("<thead>" + Environment.NewLine);
            html.Append("<tr>" + Environment.NewLine);

            foreach(DataColumn column in xData.Columns)
            {
                if (!column.ColumnName.StartsWith("_"))
                {
                    html.Append("<th style=\"" + cssTHStyle + "\">" + column.ColumnName + "</th>" + Environment.NewLine);
                }
                
            }
            html.Append("</tr>" + Environment.NewLine);

            html.Append("</thead>" + Environment.NewLine);

            html.Append("<tbody>" + Environment.NewLine);


            if (String.IsNullOrWhiteSpace(cssTRAlternativeStyle))
            {
                cssTRAlternativeStyle = cssTRStyle;
            }

            bool isOdd = true;

            string rowStyle = null;

            foreach (DataRow Row in xData.Rows)
            {

                if(isOdd)
                {
                    rowStyle = cssTRStyle;
                }
                else
                {
                    rowStyle = cssTRAlternativeStyle;
                }

                html.Append("<tr style=\"" + rowStyle + "\">" + Environment.NewLine);

                foreach (DataColumn column in xData.Columns)
                {
                    if (!column.ColumnName.StartsWith("_"))
                    {
                        html.Append("<td style=\"" + cssTDStyle + "\">" + Row[column.ColumnName].ToString().Replace(" 00:00:00","") + "</td>" + Environment.NewLine);
                    }
                }

                html.Append("</tr>" + Environment.NewLine);

                isOdd = !isOdd;
            }

            

            html.Append("</tbody>" + Environment.NewLine);
            html.Append("</table>" + Environment.NewLine);
            html.Append("</html>");
            return html.ToString();
        }
    }
}
