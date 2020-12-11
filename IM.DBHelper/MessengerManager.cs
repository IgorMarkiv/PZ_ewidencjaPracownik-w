using DBHelper.Model;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBHelper
{
    public static class MessengerManager
    {
        private static DB _DB;

        public static void Init(DB dB)
        {
            _DB = dB;
        }

        public static List<MessageInfo> GetAllMessageInfoByConversationId(int ConversationId)
        {
            try
            {
                List<MessageInfo> listOfAllMessage = _DB.GetAllMessageInfoByConversationId(ConversationId);
                return listOfAllMessage;
            }
            catch (Exception ex)
            {
                LogManager.LogError("Messanger Helper GetAllMessageByConversationId", ex.Message, "Temporary Empty");
                List<MessageInfo> listOfAllMessage = new List<MessageInfo>();
                return listOfAllMessage;
            }


        }

        public static message GetMessageByMessageInfoId(int MessageInfoId)
        {
            try
            {
                message message = _DB.GetMessageByMessageInfoId(MessageInfoId);
                return message;
            }
            catch (Exception ex)
            {
                LogManager.LogError("Messanger Helper GetAllMessageByConversationId", ex.Message, "Temporary Empty");
                message message = new message() { messageBody = "error", messageSalt = "error", };
                return message;
            }
        }

        public static bool CheckIfUserHaveAccesToConversation(int userId, int convId)
        {
            try
            {
                if (_DB.CheckIfUserHaveAccesToConversation(userId, convId) == true)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                LogManager.LogError("MessangerHelper CheckIfUserHaveAccesToConversation", ex.Message, "Temporary Empty");
                return false;
            }
        }

        public static bool CheckConversationUser(int userId, int convId)
        {
            try
            {
                if (_DB.CheckConversationUser(userId, convId) == true)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                LogManager.LogError("MessangerHelper CheckConversationUser", ex.Message, "Temporary Empty");
                return false;
            }
        }

        public static void SendMessege(int userId, int convId, string message, bool isUser1 )
        {
            try
            { _DB.SendMessege(userId, convId, message, isUser1); }
            catch (Exception ex)
            {
                LogManager.LogError("MessangerHelper SendMessege", ex.Message, "Temporary Empty");
            }
        }

        public static msg_conv GetConversationByConversationId(int conversationId)
        {
            try
            { return _DB.GetConversationByConversationId(conversationId); }
            catch (Exception ex)
            {
                LogManager.LogError("MessangerHelper GetConversationByConversationId", ex.Message, "Temporary Empty");
                msg_conv msg_Conv = new msg_conv() { id = -1 };
                return msg_Conv;
            }
        }

        public static List<msg_conv> GetconvListByUserName(string username, int userId) { return _DB.GetconvListByUserName(username, userId); }

        //public static bool isConversationExist()
        //{

        //}

        //public class HabrDotNetHub : Microsoft.AspNetCore.SignalR.Hub
        //{
        //    public Task BroadcastNlo()
        //    {
        //        return Clients.All.SendAsync()
        //    }
        //}
    }
}
