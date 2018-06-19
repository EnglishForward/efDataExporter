using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using efDataExtporter.DTO;

namespace efDataExtporter
{
    public class QueueDataToExportParameters
    {
        public CancellationTokenSource CancelToken;
        public BlockingCollection<ExportData> ProcessingQueue;
        public int ExportDataCategoryID;
    }
}
