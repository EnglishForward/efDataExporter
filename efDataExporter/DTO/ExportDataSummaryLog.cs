using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace efDataExtporter.DTO
{
    public class ExportDataSummaryLog
    {
        public int ExportDataID { get; set; }
        public int ExportDataStatusID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public void Get(int xLogID)
        {

        }

        public int Save()
        {
            return 1;
        }

    }
}
