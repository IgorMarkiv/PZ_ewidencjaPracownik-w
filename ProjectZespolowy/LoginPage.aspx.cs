using DBHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ProjectZespolowy
{
    public partial class LoginPage : System.Web.UI.Page
    {
        private void Page_PreLoad(object sender, System.EventArgs e)
        {
            if (HttpContext.Current.Request.Cookies["SessionCookie"] != null)
            {

                string session_key = Request.Cookies["SessionCookie"].Values["Session_Key"];
                string StringUserId = Request.Cookies["SessionCookie"].Values["UserId"];
                int UserId = Int32.Parse(StringUserId);

                if (SessionManagerDll.SessionManager.VerifySession(UserId, session_key) == true)
                {
                    Response.Redirect("UserProfile.aspx?id=" + UserId, false); Context.ApplicationInstance.CompleteRequest();
                }
            }

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            (Master as SiteMaster).HideNavbar();
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            string login = inputUsername.Text;
            string password = inputPassword.Text;


            try
            {
                int userId = DBHelper.LoginManager.UserLogin(login, password);

                //if (1 == 1)
                //{
                //    SessionManager.CloseSession(UserId, session_key);
                //    Response.Redirect("LoginPage.aspx");
                //}

                string session = SessionManagerDll.SessionManager.OpenSession(userId);

                HttpCookie SessionCookie;
                SessionCookie = new HttpCookie("SessionCookie");
                SessionCookie.Values["Session_Key"] = session;
                //int32.parse
                string CookieUserId = userId.ToString();
                SessionCookie.Values["UserId"] = CookieUserId;
                Response.Cookies.Add(SessionCookie);



                Response.Redirect("UserProfile.aspx?id=" + userId, false); Context.ApplicationInstance.CompleteRequest();

            }
            catch (ExeptionManagerDll.IMException E)
            {
                Console.WriteLine(E);
                //login error reason
                if (E.Message == "E05") { ErrLabel.Text = "Użytkownik o tej nazwie nie istnieje!"; }
                else if (E.Message == "E04") { ErrLabel.Text = "Hasło hasło niepoprawne!"; }
                else { ErrLabel.Text = E.Message; }

                return;
            }


        }
    }
}