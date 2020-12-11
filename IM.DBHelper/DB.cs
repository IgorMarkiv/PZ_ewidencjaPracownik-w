using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using DBHelper.Model;
using System.Data.Entity;
using System.Security.Cryptography.X509Certificates;
using SessionManagerDll;
using System.Web.Hosting;
using System.IO;
using System.Text.RegularExpressions;

namespace DBHelper
{
    public class DB : IDB
    {
        //public bool VerifyUser(string username, string password)
        //{
        //    using (var context = new IMWebDBEntities())
        //    {
        //        Users user = context.Users.Where(u => u.user == username && u.password == password).FirstOrDefault();
        //        if (user != null) { return true; }
        //        else { return false; }
        //    }
        //}

        public void LogVisit()
        {
            using (var Context = new IgorMarkivMessengerDBEntities())
            {
                siteVisits siteVisits = new siteVisits()
                {
                    isSession = false,
                    VisiteTime = DateTime.Now

                };
                Context.siteVisits.Add(siteVisits);
                Context.SaveChanges();
            }
        }

        public void LogError(string errorName, string ErrorValue, string ErrorCode)
        {
            using (var Context = new IgorMarkivMessengerDBEntities())
            {

                ErrorLogs errorLogs = new ErrorLogs
                {
                    errorName = errorName,
                    errorValue = ErrorValue,
                    errorCode = ErrorCode,
                    DateTime = DateTime.UtcNow
                };

                Context.ErrorLogs.Add(errorLogs);
                Context.SaveChanges();

            }
        }

        public bool CheckIfUserExists(string login, out int userId)
        {
            using (var Context = new IgorMarkivMessengerDBEntities())
            {
                Users user = Context.Users.Where(u => u.userName == login).FirstOrDefault();
                if (user != null)
                {
                    userId = user.id;
                    return true;
                }
                else
                {
                    userId = -1;
                    return false;
                }
            }
        }

        public DateTime GetLastLoginAttempt(int userId, out int count)
        {
            using (var Context = new IgorMarkivMessengerDBEntities())
            {
                Users user = Context.Users.Where(u => u.id == userId).FirstOrDefault();
                if (user.lastLogin != null)
                {
                    count = user.invalidLogin;
                    return user.lastLogin;
                }
                else
                {
                    count = -1;
                    return DateTime.MinValue;
                }
            }
        }

        public bool CheckIfPasswordCorrect(int userId, string password)
        {
            using (var Context = new IgorMarkivMessengerDBEntities())
            {
                var user = Context.Users.Where(u => u.id == userId).FirstOrDefault();
                if (user.password == SessionManagerDll.Util.EncodePassword(password, user.salt))
                    return true;
                else
                    return false;
            }
        }

        public bool ResetInvalidLoginAttempts(int userId)
        {
            using (var Context = new IgorMarkivMessengerDBEntities())
            {
                Users user = Context.Users.Where(u => u.id == userId).FirstOrDefault();
                if (user == null) return false;
                user.invalidLogin = 0;
                Context.SaveChanges();
                return true;
            }
        }

        public bool AddInvalidLoginAttempt(int userId)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                Users user = context.Users.Where(u => u.id == userId).FirstOrDefault();
                if (user == null) return false;
                user.invalidLogin++;
                context.SaveChanges();
                return true;
            }
        }

        public bool CheckIfRegistrationNotRepeated(string login, string email)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                Users user = context.Users.Where(u => u.userName == login || u.userEmail == email).FirstOrDefault();
                if (user == null)
                    return true;
                else
                    return false;
            }
        }

        public bool IsUserLoggedIn(int userId, out List<string> sessions)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                sessions = GetSessionsList(userId);
                if (sessions.Count > 0)
                    return true;
                else
                    return false;
            }

        }

        public DateTime GetUserLastActivity(int userId, string session)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                var sessionEntity = context.Sessions.Where(s => s.userId == userId && s.sessionKey == session).FirstOrDefault();
                if (sessionEntity != null)
                    return sessionEntity.lastActivity;
                else
                    return DateTime.MinValue;
            }

        }

        public bool SetUserLastActivity(int userId, string session, DateTime activityTime)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                var sessionEntity = context.Sessions.Where(s => s.userId == userId && s.sessionKey == session).FirstOrDefault();
                if (sessionEntity != null)
                {
                    sessionEntity.lastActivity = activityTime;
                    context.SaveChanges();
                    return true;
                }
                else
                    return false;
            }

        }

        public bool CreateUser(string login, string password, string email, string salt, string role)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                var user = new Users();
                user.userName = login;
                user.password = password;
                user.userEmail = email;
                user.salt = salt;
                user.invalidLogin = 0;
                user.role = role;
                user.lastLogin = DateTime.UtcNow;
                context.Users.Add(user);
                context.SaveChanges();
            }
            return true;
        }

        public bool CreateSession(int userId, string sessionKey)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                var sessionEntity = new Sessions()
                {
                    lastActivity = DateTime.UtcNow,
                    sessionKey = sessionKey,
                    userId = userId
                };
                context.Sessions.Add(sessionEntity);
                context.SaveChanges();
            }
            return true;
        }

        public bool RemoveSession(string sessionKey)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                var sessionEntity = context.Sessions.Where(s => s.sessionKey == sessionKey).FirstOrDefault();
                if (sessionEntity != null)
                {
                    context.Sessions.Remove(sessionEntity);
                    context.SaveChanges();
                    return true;
                }
                else
                    return false;
            }

        }

        public bool RemoveAllUsersSessions(int userId)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                bool result = true;
                var sessionList = context.Sessions.Where(s => s.userId == userId);
                foreach (var session in sessionList)
                    context.Sessions.Remove(session);
                context.SaveChanges();
                return result;
            }

        }

        public bool RemoveAllExpiredSessions(double hours)
        {
            DateTime baselineDate = DateTime.UtcNow.AddHours(-hours);
            try
            {
                using (var context = new IgorMarkivMessengerDBEntities())
                {
                    bool result = true;
                    List<Sessions> expiredSessions = context.Sessions.Where(s => s.lastActivity < baselineDate).ToList();
                    if (expiredSessions.Count != 0)
                    {
                        foreach (Sessions s in expiredSessions)
                            context.Sessions.Remove(s);
                        context.SaveChanges();
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                DBHelper.LogManager.LogError("DBHelper db RemoveAllExpiredSessions", ex.Message, "Temporary Empty");
                return false;
            }

        }

        public List<string> GetSessionsList(int userId)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                var sessionEntities = context.Sessions.Where(s => s.userId == userId);
                var sessionKeys = new List<string>();
                foreach (Sessions s in sessionEntities)
                    sessionKeys.Add(s.sessionKey);
                return sessionKeys;
            }
        }

        public bool TryGetUserById(int userId, out Users user)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                user = context.Users.FirstOrDefault(s => s.id == userId);
                if (user != null)
                    return true;
                else
                {
                    return false;
                }

            }
        }

        public List<MessageInfo> GetAllMessageInfoByConversationId(int ConversationId)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                List<MessageInfo> listOfmessageInfo = context.MessageInfo.Where(ms => ms.convId == ConversationId).ToList();
                return listOfmessageInfo;
            }
        }

        public message GetMessageByMessageInfoId(int messageId)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                message message = context.message.Where(ms => ms.id == messageId).FirstOrDefault();
                return message;
            }
        }

        public bool CheckIfUserHaveAccesToConversation(int userId, int convId)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                msg_conv conv = context.msg_conv.Where(cv => cv.id == convId).FirstOrDefault();
                if (conv.user1 == userId || conv.user2 == userId)
                    return true;
                else
                    return false;

            }
        }

        public bool CheckConversationUser(int userId, int convId)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                msg_conv conv = context.msg_conv.Where(cv => cv.id == convId).FirstOrDefault();
                if (conv.user1 == userId)
                    return true;
                else
                    return false;

            }
        }

        public void SendMessege(int userId, int convId, string messege, bool isUser1)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                DBHelper.Model.MessageInfo messageInfo = new MessageInfo()
                {
                    id=1,
                    isUser1 = isUser1,
                    convId = convId,
                    time = DateTime.UtcNow,
                    changed = false
                };

                List<MessageInfo> listOfmessageInfos = new List<MessageInfo>();
                listOfmessageInfos.Add(messageInfo);

                message message = new message()
                {
                    messageBody = messege,
                    messageSalt = "temporaryEmpty",
                    MessageInfo = listOfmessageInfos
                };

                messageInfo.message = message;
                context.MessageInfo.Add(messageInfo);
                context.message.Add(message);
                context.SaveChanges();

            }
        }

        public msg_conv GetConversationByConversationId(int converationId)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                msg_conv msg_Conv = context.msg_conv.Where(mc => mc.id == converationId).FirstOrDefault();
                return msg_Conv;
            }
        }

        public List<Users> GetUsers(){using (var context = new IgorMarkivMessengerDBEntities()){return context.Users.ToList();}}

        public List<msg_conv> GetListOfAllUserConversations(int userId){using (var context = new IgorMarkivMessengerDBEntities()){return context.msg_conv.Where( ms => ms.user1 == userId || ms.user2 == userId).ToList();}}

        public List<msg_conv> GetconvListByUserName(string username, int userId)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                List<Users> usersList = context.Users.Where(usr => usr.userName == username).ToList();
                List<msg_conv> Emptyconv = new List<msg_conv>();

                if (usersList.Count == 0)
                    return Emptyconv;
                else
                {
                    int tergetUserId = usersList[0].id;
                    List<msg_conv> convList = context.msg_conv.Where(conv => (conv.user1 == tergetUserId && conv.user2 == userId) || (conv.user1 == userId && conv.user2 == tergetUserId)).ToList();

                    if(convList.Count == 0)
                    {
                        msg_conv conv = CreateConversation(userId, usersList[0].id);
                        convList.Add(conv);
                        return convList;
                    }
                    else
                    {
                        return convList;
                    }
                }
            }
        }

        public static msg_conv CreateConversation(int user1, int user2)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                msg_conv conv = new msg_conv() { user1 = user1, user2 = user2 };
                context.msg_conv.Add(conv);
                context.SaveChanges();
                return conv;
            }
        }

        public bool CheckUserInvitationCode(string invitationCode)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                InvitationCode invitatiionCode = context.InvitationCode.Where(invc => invc.code == invitationCode).FirstOrDefault();
                if(invitatiionCode != null)
                {
                    if (invitatiionCode.used == false)
                        return true;
                    else
                        return false;
                }
                else
                {
                    return false;
                }

            }
        }

        public DateTime GetRemeinSessionTime(int userId)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                Sessions userSesion = context.Sessions.Where(s => s.userId == userId).FirstOrDefault();
                if(userSesion != null)
                {
                    return userSesion.lastActivity;
                }
                else
                {
                    return DateTime.Now;
                }
            }
        }

        public int AddFileData(string filename, int userId, string fileCrc, long fileSize, bool isNotlimited, string type, string root)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                var fileCheck = context.Files.Where(fm => fm.name == filename && fm.rout == root).FirstOrDefault();
                if (fileCheck != null) { return -1; }
                    //throw new ExeptionManagerDll.IMException(ExeptionManagerDll.IMExceptionCode.Ex00 + " | File with this name already exists");

                string newGuid = Guid.NewGuid().ToString();
                var file = new Files()
                {
                    name = filename,
                    size = fileSize,
                    uploadDate = DateTime.UtcNow,
                    user = userId,
                    md5 = fileCrc,
                    guid = newGuid,
                    type = type.ToLower(),
                    rout = root
                };
                if (isNotlimited == true)
                    file.expireDate = DateTime.UtcNow.AddYears(100);
                else
                    file.expireDate = DateTime.UtcNow.AddDays(1);
                context.Files.Add(file);
                context.SaveChanges();
                return file.id;
            }
        }

        public List<Files> GetFileListByUserId(int userId)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                List<Files> files = context.Files.Where(f => f.user == userId).ToList();
                return files;
            }
        }

        public List<Files> GetFileListByDirectoryRoot( List<string> fulldirectoryroot)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                List<Files> userdirectories = new List<Files>();
                int userId = Int32.Parse(fulldirectoryroot[0]);
                fulldirectoryroot.Remove(fulldirectoryroot[0]);

                string mainUserRoot = "-" + userId.ToString();

                userdirectories = context.Files.Where(f =>  f.user == userId && f.rout == mainUserRoot ).ToList();

                string userDirectoryRoot = mainUserRoot;
                foreach (string subroot in fulldirectoryroot)
                {
                    userDirectoryRoot = userDirectoryRoot + "-" + subroot;
                    userdirectories = context.Files.Where(f => f.rout == userDirectoryRoot && f.user == userId).ToList();
                }

                return userdirectories;
            }
        }
        

        public void CreateDirectory(int userId, string name, string folderRout)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                Files Folder = new Files { name = name, rout = folderRout, uploadDate = DateTime.UtcNow, user = userId, type="directory" };
                context.Files.Add(Folder);
                context.SaveChanges();
            }
        }

        public void DeleteFileById(int targetFileId)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                Files targetFile = context.Files.Where(f => f.id == targetFileId).FirstOrDefault();
                string pathCombined = "";
                if (targetFile.type == "directory")
                {
                    pathCombined = targetFile.rout + "-" + targetFile.name;
                    List<Files> fileInDeletedFolder = context.Files.Where(f => f.rout == pathCombined).ToList();
                    foreach (Files file in fileInDeletedFolder)
                    {
                        string innerFilesroot = file.rout + "-" + file.name;
                        List<Files> innerFiles = context.Files.Where(innrf => innrf.user == file.user && innrf.rout == innerFilesroot).ToList();
                        if (innerFiles.Count > 0)
                        {
                            foreach (Files innerFile in innerFiles)
                            {
                                DeleteFileById(innerFile.id);
                            }
                        }
                        if (file.type == "directory")
                        {
                            string diskSaveRoot = Path.Combine(@"D:\cloud", CloudManager.RootConvertion(file.rout), file.name);
                            if (Directory.Exists(diskSaveRoot) == true)
                                Directory.Delete(diskSaveRoot);
                        }
                        else
                        {
                            string diskSaveRoot = Path.Combine(@"D:\cloud", CloudManager.RootConvertion(file.rout), file.guid);
                            if (File.Exists(diskSaveRoot) == true)
                                File.Delete(diskSaveRoot);
                            if (file.type == ".png" || file.type == ".jpg" || file.type == ".mp4" || file.type == ".webm")
                                DeleteThumbnailFromDBbyFileId(file.id);
                        }
                        context.Files.Remove(file);
                    }
                    if (targetFile.type == "directory")
                    {
                        string diskSaveRoot = Path.Combine(@"D:\cloud", CloudManager.RootConvertion(targetFile.rout), targetFile.name);
                        if (Directory.Exists(diskSaveRoot) == true)
                            Directory.Delete(diskSaveRoot);
                    }
                    else
                    {
                        string diskSaveRoot = Path.Combine(@"D:\cloud", CloudManager.RootConvertion(targetFile.rout), targetFile.guid);
                        if (File.Exists(diskSaveRoot) == true)
                            File.Delete(diskSaveRoot);
                        if (targetFile.type == ".png" || targetFile.type == ".jpg" || targetFile.type == ".mp4" || targetFile.type == ".webm")
                            DeleteThumbnailFromDBbyFileId(targetFile.id);
                    }
                    context.Files.Remove(targetFile);
                }
                else
                {
                    string diskSaveRoot = Path.Combine(@"D:\cloud", CloudManager.RootConvertion(targetFile.rout), targetFile.guid);
                    if (File.Exists(diskSaveRoot) == true)
                        File.Delete(diskSaveRoot);
                    if (targetFile.type == ".png" || targetFile.type == ".jpg" || targetFile.type == ".mp4" || targetFile.type == ".webm")
                        DeleteThumbnailFromDBbyFileId(targetFile.id);
                    context.Files.Remove(targetFile);
                    
                }
                context.SaveChanges();
            }
        }

        public void MoveFileToDirById(int firstId, int secondId)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                Files targetFile = context.Files.Where(tf => tf.id == firstId).FirstOrDefault();
                Files destinationDir = context.Files.Where(df => df.id == secondId).FirstOrDefault();

                if (targetFile != null && destinationDir != null )
                {
                    if(targetFile.type == "directory")
                    {
                        List<Files> innerFiles = context.Files.Where(innrf => innrf.rout == targetFile.rout + "-" + targetFile.name).ToList();
                        if (innerFiles.Count != 0)
                        {
                            foreach (Files innerFile in innerFiles)
                            {
                                MoveFileToNewDir(innerFile.id, destinationDir.name, targetFile.name);
                            }
                        }

                        string oldDiskRoot = Path.Combine(CloudManager.RootConvertion(targetFile.rout), targetFile.name);
                        targetFile.rout = destinationDir.rout + "-" + destinationDir.name;
                        context.SaveChanges();
                        string newDiskRoot = Path.Combine(CloudManager.RootConvertion(targetFile.rout), targetFile.name);
                        Directory.Move(Path.Combine(@"D:\cloud", oldDiskRoot), Path.Combine(@"D:\cloud", newDiskRoot));

                    }
                    else
                    {
                        string oldDiskRoot = Path.Combine(CloudManager.RootConvertion(targetFile.rout), targetFile.guid);
                        targetFile.rout = destinationDir.rout + "-" + destinationDir.name;
                        context.SaveChanges();
                        string newDiskRoot = Path.Combine(CloudManager.RootConvertion(targetFile.rout), targetFile.guid);
                        File.Move(Path.Combine(@"D:\cloud", oldDiskRoot), Path.Combine(@"D:\cloud", newDiskRoot));
                    }

                }
                else
                {
                    throw new ExeptionManagerDll.IMException(ExeptionManagerDll.IMExceptionCode.Ex00);
                }
            }
        }

        public void MoveFileToPreviousDir(int firstId)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                Files targetFile = context.Files.Where(tf => tf.id == firstId).FirstOrDefault();

                if (targetFile != null)
                {
                    string oldDiskRoot = Path.Combine(@"D:\cloud", CloudManager.RootConvertion(targetFile.rout));
                    List<string> targetFileRoutSubstrings = CloudManager.GetDBSubrootsFormDBRoot(targetFile.rout);
                    string moovingSubroot = targetFileRoutSubstrings[targetFileRoutSubstrings.Count - 1];

                    if (targetFile.type == "directory")
                    { 
                        List<Files> innerFiles = context.Files.Where(innrf => innrf.rout == targetFile.rout + "-" + targetFile.name).ToList();
                        if (innerFiles.Count != 0)
                        {
                            foreach( Files innerFile in innerFiles)
                            {
                                MoveFileToPreviousDir(innerFile.id, moovingSubroot);
                            }
                        }
                        targetFileRoutSubstrings.Remove(moovingSubroot);
                        string nweRoot = CloudManager.GetDBRootFromSubroots(targetFileRoutSubstrings);
                        targetFile.rout = nweRoot;
                        string newDiskRoot = Path.Combine(@"D:\cloud", CloudManager.RootConvertion(nweRoot));
                        Directory.Move(Path.Combine(oldDiskRoot, targetFile.name), Path.Combine(newDiskRoot, targetFile.name));
                    }
                    else
                    {
                        targetFileRoutSubstrings.Remove(moovingSubroot);
                        string nweRoot = CloudManager.GetDBRootFromSubroots(targetFileRoutSubstrings);
                        targetFile.rout = nweRoot;
                        string newDiskRoot = Path.Combine(@"D:\cloud", CloudManager.RootConvertion(nweRoot));
                        File.Move(Path.Combine(oldDiskRoot, targetFile.guid), Path.Combine(newDiskRoot, targetFile.guid));
                    }
                    context.SaveChanges();
                }
                else
                {
                    throw new ExeptionManagerDll.IMException(ExeptionManagerDll.IMExceptionCode.Ex00);
                }
            }
        }


        public void MoveFileToPreviousDir(int fileID, string moovingSubroot)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                Files targetFile = context.Files.Where(tf => tf.id == fileID).FirstOrDefault();

                List<string> targetFileRoutSubstrings = CloudManager.GetDBSubrootsFormDBRoot(targetFile.rout);
                string oldDiskRoot = Path.Combine(@"D:\cloud", CloudManager.RootConvertion(targetFile.rout));
                if (targetFile.type == "directory")
                { 
                    List<Files> innerFiles = context.Files.Where(innrf => innrf.rout == targetFile.rout + "-" + targetFile.name).ToList();
                    if (innerFiles.Count != 0)
                    {
                        foreach (Files innerFile in innerFiles)
                        {
                            MoveFileToPreviousDir(innerFile.id, moovingSubroot);
                        }
                    }
                    targetFileRoutSubstrings.Remove(moovingSubroot);
                    string nweRoot = CloudManager.GetDBRootFromSubroots(targetFileRoutSubstrings);
                    targetFile.rout = nweRoot;


                }
                else
                {
                    targetFileRoutSubstrings.Remove(moovingSubroot);
                    string nweRoot = CloudManager.GetDBRootFromSubroots(targetFileRoutSubstrings);
                    targetFile.rout = nweRoot;

                }
                context.SaveChanges();
            }
        }

        public void MoveFileToNewDir(int fileID, string moovingSubroot, string mainDir)/////////////////////////////
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                Files targetFile = context.Files.Where(tf => tf.id == fileID).FirstOrDefault();

                List<string> targetFileRoutSubstrings = CloudManager.GetDBSubrootsFormDBRoot(targetFile.rout);
                
                string oldDiskRoot = Path.Combine(@"D:\cloud", CloudManager.RootConvertion(targetFile.rout));
                if (targetFile.type == "directory")
                {
                    List<Files> innerFiles = context.Files.Where(innrf => innrf.rout == targetFile.rout + "-" + targetFile.name).ToList();
                    if (innerFiles.Count != 0)
                    {
                        foreach (Files innerFile in innerFiles)
                        {
                            MoveFileToNewDir(innerFile.id, moovingSubroot, mainDir);
                        }
                    }
                }
                string newRoot = "";
                foreach (string subRoot in targetFileRoutSubstrings)
                {
                    if (subRoot == mainDir)
                        newRoot = newRoot + "-" + moovingSubroot + "-" + subRoot;
                    else
                        newRoot = newRoot + "-" + subRoot;
                 }
                targetFile.rout = newRoot;
                context.SaveChanges();
            }
        }

        public Files GetFileById(int fileId, int userId){using (var context = new IgorMarkivMessengerDBEntities()){return context.Files.Where(tf => tf.user == userId && tf.id == fileId).FirstOrDefault();}}
        public bool CheckIfFileIsDirectory(int fileId)
        {
            using (var context = new IgorMarkivMessengerDBEntities()) 
            {  
                Files file = context.Files.Where(tf => tf.id == fileId).FirstOrDefault();
                if (file.type == "directory")
                    return true;
                else
                    return false;

            } 
        }
        public int SaveThumbnail(int originalFileId)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                thumbnails thumbnail = new thumbnails() { fileId = originalFileId };
                context.thumbnails.Add(thumbnail);
                context.SaveChanges();
                return context.thumbnails.Where(thmb => thmb.fileId == originalFileId).FirstOrDefault().id;
                
            }
        }

        public string GetGuidByFileId(int fileId){using (var context = new IgorMarkivMessengerDBEntities()){return context.Files.Where(fl => fl.id == fileId).FirstOrDefault().guid;}}

        public void DeleteThubmail(int thimbnailId)
        {
            using (var context = new IgorMarkivMessengerDBEntities()) 
            {
                thumbnails thumbnail = context.thumbnails.Where(thmb => thmb.id == thimbnailId).FirstOrDefault();
                context.thumbnails.Remove(thumbnail);
                context.SaveChanges();
            }
        }

        public int GetThumbnailIdByFileId(int fileId)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                return context.thumbnails.Where(thmb => thmb.fileId == fileId).FirstOrDefault().id;
            }
        }

        public thumbnails GetGetThumbnailByFileId(int fileId){using (var context = new IgorMarkivMessengerDBEntities()){return context.thumbnails.Where(thmb => thmb.fileId == fileId).FirstOrDefault();}}


        public void AddEncryptKeys(int fileId, byte[] key, byte[] iv)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                thumbnails thumbnail = context.thumbnails.Where(thmb => thmb.fileId == fileId).FirstOrDefault();
                thumbnail.key = key;
                thumbnail.iv = iv;
                context.SaveChanges();
            }
        }

        public void SaveUseSettingTableView(int userId, bool tableViewStatus)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                userSettings userSetting = context.userSettings.Where(us => us.userId == userId).FirstOrDefault();
                if(userSetting != null)
                    userSetting.fileTableView = tableViewStatus;
                else
                {
                    userSettings newUserSetting = new userSettings() { userId = userId, fileTableView = tableViewStatus };
                    context.userSettings.Add(newUserSetting);
                }
                context.SaveChanges();
            }
        }

        public bool GetUsertableViewStatusById(int userId)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                userSettings userSetting = context.userSettings.Where(us => us.userId == userId).FirstOrDefault();
                if (userSetting != null)
                    return userSetting.fileTableView.Value;
                else
                    return false;
            }
        }

        public void InactivateInvitationCode(string invitationCode)
        {
            using (var context = new IgorMarkivMessengerDBEntities())
            {
                InvitationCode invitation = context.InvitationCode.Where(inv => inv.code == invitationCode).FirstOrDefault();
                invitation.used = true;
                context.SaveChanges();
            }
        }

        public Users GetUserByUserName(string userName){using (var context = new IgorMarkivMessengerDBEntities()){ return context.Users.Where(usr => usr.userName == userName).FirstOrDefault();}}
        public void DeleteThumbnailFromDBbyFileId(int fileId) {
            {
                using (var context = new IgorMarkivMessengerDBEntities()) {
                    thumbnails tmbnl = context.thumbnails.Where(tmb => tmb.fileId == fileId).FirstOrDefault(); 
                    if(tmbnl != null)
                    {
                        context.thumbnails.Remove(tmbnl);
                        context.SaveChanges();
                    }

                } 
            } 
        }
        
    }
}



