using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace efDataExtporter.DTO
{
    public class ExportDataType
    {
        public int ExportDataID { get; set; }
        public string Description { get; set; }

        public enum TYPE
        {
            CSV = 1,
            HTML = 2,
        }
    }
}
