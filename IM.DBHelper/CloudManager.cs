using DBHelper.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace DBHelper
{
    public static class CloudManager
    {
        private static DB _DB;
        public static void Init(DB dB)
        {
            _DB = dB;
        }

        public static string RootConvertion(string rawRoot)
        {
            string pattern = "(-)";
            string[] substrings = Regex.Split(rawRoot, pattern);    // Split on hyphens
            string convertedRoot = ""; ////
            foreach (string s in substrings)
            {
                if (s.Contains("-") != true)
                    convertedRoot = Path.Combine(convertedRoot, s);
            }
            
            return convertedRoot;
        }

        public static List<string> GetDBSubrootsFormDBRoot(string rawRoot)
        {
            string[] substrings = Regex.Split(rawRoot, "-");
            List<string> targetFileRoutSubstrings = new List<string>();
            foreach (string s in substrings)
            {
                if (s != "")
                    targetFileRoutSubstrings.Add(s);
            }
            return targetFileRoutSubstrings;
        }

        public static string GetDBRootFromSubroots(List<string> listOfSubrrots)
        {
            string newRout = "";
            foreach (string str in listOfSubrrots)
            {
                newRout = newRout + "-" + str;
            }
            return newRout;
        }

        public static int SaveThumbnail(int originalFileId)
        {
            return _DB.SaveThumbnail(originalFileId);
        }
        public static int FileUpload(string filename, int userId, string fileCrc, long fileSize, bool isNotlimited, string type, string root)
        {

            return _DB.AddFileData(filename, userId, fileCrc, fileSize, isNotlimited, type, root);
        }

        public static string GetGuidByFileId(int fileId)
        {
           return  _DB.GetGuidByFileId(fileId);
        }

        public static List<Files> GetFileListByUserId(int userId)
        {
            return _DB.GetFileListByUserId(userId);
        }

        public static List<Files> GetFileListByDirectoryRoot( List<String> fulldirectoryRoot)
        {
            return _DB.GetFileListByDirectoryRoot(fulldirectoryRoot);
        }

        public static void CreateDirectory(int userId, string name, string folderRout)
        {
            _DB.CreateDirectory(userId, name, folderRout);
        }
        public static void DeleteFileById(int userId)
        {
            _DB.DeleteFileById(userId);
        }

        public static void MoveFileToDirById(int firstId, int secondId)
        {
            _DB.MoveFileToDirById( firstId,  secondId);
        }

        public static void MoveFileToPreviousDir(int intFirstId)
        {
            _DB.MoveFileToPreviousDir(intFirstId);
        }

        public static Files GetFileById(int fileId, int userId)
        {
            return _DB.GetFileById(fileId, userId);
        }

        public static bool CheckIfFileIsDirectory(int fileId)
        {
            return _DB.CheckIfFileIsDirectory(fileId);
        }
        

        public static void DeleteThubmail(int thumbnailId)
        {
            _DB.DeleteThubmail(thumbnailId);
        }

       
        public static thumbnails GetGetThumbnailByFileId(int fileId)
        {
            return _DB.GetGetThumbnailByFileId(fileId);
        }
        public static int GetThumbnailIdByFileId(int fileId)
        {
           return _DB.GetThumbnailIdByFileId(fileId);
        }

        public static void AddEncryptKeys(int fileId, byte[] key, byte[] iv)
        {
            _DB.AddEncryptKeys(fileId, key, iv);
        }



    }
}
