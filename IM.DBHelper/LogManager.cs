using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBHelper
{
    public static class LogManager
    {
        private static DB _DB;

        public static void Init(DB dB)
        {
            _DB = dB;
        }

        public static void LogVisit()
        {
            _DB.LogVisit();
        }

        public static void LogError( string errorName, string errorValue, string errorCode )
        {
            _DB.LogError(errorName, errorValue, errorCode);
        }




    }
}
