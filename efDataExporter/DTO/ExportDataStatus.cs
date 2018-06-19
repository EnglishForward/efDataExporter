using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace efDataExtporter.DTO
{
    public class ExportDataStatus
    {

        public enum STATUS
        {
            RUNNING = 1,
            RUN_FAILED,
            RUN_COMPLETE,
            DISABLED
        }
        

    }
}
