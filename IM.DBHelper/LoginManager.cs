using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SessionManagerDll;

namespace DBHelper
{

    public static class LoginManager
    {
        private static DB _DB;

        public static void Init(DB dB)
        {
            _DB = dB;
        }

        //public static bool login(string username, string password)
        //{
        //    if (_DB.VerifyUser(username, password) == true){return true;}
        //    else{return false;}
        //}




        public static int UserLogin(string login, string password)
        {
            try
            {
                SessionManagerDll.UserManager.Login(login, password, out int userId);
                return userId;
            }
            catch(Exception ex)
            {
                string errorName = "Error during user login";
                string errorValue = ex.Message;
                string errorCode = "Temporary Empty";
                LogManager.LogError(errorName,errorValue,errorCode );
                return -1;
            }

        }

    }
}
