using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectZespolowy.PagesMethods
{
    public class UserAccesVeryfication
    {

        public static bool veryfiUser(HttpCookie clientCookie)
        {
            if (clientCookie.Values["Session_Key"] != null && clientCookie.Values["UserId"] != null)
            {
                string session_key = clientCookie.Values["Session_Key"];
                string UserId = clientCookie.Values["UserId"];
                if (SessionManagerDll.SessionManager.VerifySession(Int32.Parse(UserId), session_key) == true) { return true; }
                else { return false; }
            }
            else { return false; }
        }

    }
}