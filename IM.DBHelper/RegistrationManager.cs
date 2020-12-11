using SessionManagerDll;
using ExeptionManagerDll;
using DBHelper;
using System;

namespace DBHelper
{
    public static class RegistrationManager
    {

        private static DB _DB;

        public static void Init(DB dB)
        {
            _DB = dB;
        }

        public static bool CheckUserInvitationCode(string invitationCode)
        {
            try
            {
                if (_DB.CheckUserInvitationCode(invitationCode) == true)
                    return true;
                else
                    return false;
            }
            catch(Exception ex)
            {
                LogManager.LogError("CheckUserInvitationCode", ex.Message, "Temorary Empty");
                return false;
            }
        }

        //public static void RegisterWithRole(string login, string inputFirstname, string InputLastname, string password, string email, bool role)
        //{
        //    string salt = Util.GenerateSalt();
        //    string encPassword = Util.EncodePassword(password, salt);
        //    if (_DB.CheckIfRegistrationNotRepeated(login, email))
        //    {
        //        if (!_DB.CreateUser(login, inputFirstname, InputLastname,  encPassword, email, salt, role))
        //            throw new IMException(IMExceptionCode.Ex04);
        //    }
        //    else
        //        throw new IMException(IMExceptionCode.Ex08);
        //}

        //public static bool IsFirstRegistrationNeeded()
        //{
        //    return _DB.CheckIfGlobalAdminExists();
        //}
    }
}
