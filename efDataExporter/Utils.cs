using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace efDataExtporter
{
    public class Utils
    {
        public static bool IsLocalPath(string xFTPHostOrLocalPath)
        {
            bool isLocal = false;
            const string UNC_PATH = @"\\";
            const string LOCAL_DRIVE = @":";

            if(!String.IsNullOrWhiteSpace(xFTPHostOrLocalPath))
            {
                if(xFTPHostOrLocalPath.StartsWith(UNC_PATH))
                {
                    isLocal = true;
                }
                else if(xFTPHostOrLocalPath.Substring(1,1) == LOCAL_DRIVE)
                {
                    isLocal = true;
                }
            }
            return isLocal;
        }

    }
}
