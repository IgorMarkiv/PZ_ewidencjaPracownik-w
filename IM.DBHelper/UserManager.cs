using System.Collections.Generic;
using SessionManagerDll;
using ExeptionManagerDll;
using DBHelper.Model;
using System;

namespace DBHelper
{
    public static class UserManager
    {

        private static DB _DB;

        public static void Init(DB dB)
        {
            _DB = dB;
        }
        
        //public static List<string> GetSessionList(int userid) 
        //{
        //    return _DB.GetSessionsList(userid);
        //}
        
        public static bool TryGetUserById(int userId,  out Users user)
        {
            if(_DB.TryGetUserById(userId, out user) != true)
            {
                DBHelper.LogManager.LogError("TryGetUserById", "Manual exeption in userManager TryGetUserById", "User id -1 null reference");
                return false;
            }
            else
            {
                return true;
            }
            
        }

        public static List<Users> GetListAllUsers(){return _DB.GetUsers();}

        public static List<msg_conv> GetListOfAllUserConversations(int userId) { return _DB.GetListOfAllUserConversations(userId); }

        public static bool IsUserAdmin(int userId)
        {
            try
            {
                if(_DB.TryGetUserById(userId, out Users user) == true)
                {
                    if (user.role == "admin")
                        return true;
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                LogManager.LogError("IsUserAdmin", ex.ToString(), "TemporaryEmpty");
                return false;
            }
        }

        public static TimeSpan GetRemeinSessionTime(int userId)
        {
            try
            {
                DateTime lastLogin = _DB.GetRemeinSessionTime(userId);
                DateTime now = DateTime.Now;
                return now.Subtract(lastLogin);
            }
            catch (Exception ex)
            {
                LogManager.LogError("IsUserAdmin", ex.ToString(), "TemporaryEmpty");
                return TimeSpan.Zero;
            }
        }

        public static void SaveUseSettingTableView(int userId, bool checkBoxStatus)
        {
            _DB.SaveUseSettingTableView(userId, checkBoxStatus);
        }

        public static bool GetUsertableViewStatusById(int userId)
        {
            return _DB.GetUsertableViewStatusById(userId);
        }

        public static void InactivateInvitationCode(string invitationCode)
        {
            _DB.InactivateInvitationCode(invitationCode);
        }
        
        public static Users GetUserByUserName(string userName)
        {
            return _DB.GetUserByUserName(userName);
        }

        //public static void DeleteUser(int userId, int adminId, string adminSessionKey)
        //{
        //    if (GetLoggedInUserRole(adminId, adminSessionKey) != true)
        //        throw new IMException("Ex13"); //User has no rights to delete
        //    if (userId == adminId)
        //        throw new IMException(IMExceptionCode.Ex00); // Tries to delete himself/herself
        //    if (!_DB.RemoveUser(userId))
        //        throw new IMException(IMExceptionCode.Ex03);
        //}

        //public static void UpdateUserData(int userId, string new_login, string new_firstname, string new_last_name, string newEmail, string newPassword, bool newRole)
        //{
        //    if (!_DB.UpdateUserData(userId, new_login, new_firstname, new_last_name, newEmail, newPassword, newRole))
        //        throw new IMException(IMExceptionCode.Ex04);
        //}

        //public static bool GetLoggedInUserRole(int userId, string sessionKey)
        //{
        //    if (!SessionManager.VerifySession(userId, sessionKey))
        //        throw new IMException(IMExceptionCode.Ex11);
        //    else
        //        //return _DB.GetUserRole(userId).ToLower(); 
        //        return _DB.GetUserRole(userId);
        //}

        //public static bool IsGlobalAdminRegistered()
        //{
        //    List<Users> usersTable = _DB.GetAllUsers();siteVisits

        //    if (usersTable.Count != 0){
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public static bool? CheckUserRole(int user_id)
        //{
        //    bool? user_role = _DB.CheckUserRole(user_id);
        //    if (user_role != null) { return user_role; }
        //    else{ throw new IMException(IMExceptionCode.Ex03 + "Not find UserName in db!"); }
        //}

        //public static void  RegisterFirstAdmin() { _DB.RegisterFirstAdmin(); }

        //public static bool CheckIfLoginNotRepeat(int user_id, string login)
        //{
        //    if (_DB.CheckIfLoginNotRepeat(user_id, login) == true){ return true;}
        //    else{ return false; }
        //}

        //public static bool CheckIfEmailNotRepeat(int user_id,  string email)
        //{
        //    if (_DB.CheckIfEmailNotRepeat(user_id, email) == true){ return true;}
        //    else{return false;}
        //}


        //public static void Register(string login, string password, string email, string role)
        //{
        //    string salt = Util.GenerateSalt();
        //    string encPassword = Util.EncodePassword(password, salt);
        //    if (_DB.CheckIfRegistrationNotRepeated(login, email))
        //    {
        //        if (!_DB.CreateUser(login, encPassword, email, salt, role))
        //            throw new IMException(IMExceptionCode.Ex04);
        //    }
        //    else
        //        throw new IMException(IMExceptionCode.Ex08);
        //}

    }
}
