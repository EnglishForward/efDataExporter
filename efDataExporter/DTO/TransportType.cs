using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace efDataExtporter.DTO
{

    public class TransportType 
    {
        //public TransportType();
        //public TransportType(int xTransportTypeID);

        //[DBPrimaryKey]
        public int TransportTypeID { get; set; }
        //[DBField(FieldName = "TransportType")]
        public string TransportTypeName { get; set; }

        public enum TYPE
        {
            FTP = 1,
            EMAIL = 2,
        }
    }
}
