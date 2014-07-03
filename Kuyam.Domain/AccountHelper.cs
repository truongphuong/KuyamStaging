using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;
using Kuyam.Utility;


namespace Kuyam.Domain
{
    public static class AccountHelper
    {
        /*
        public static int CreateAccount(bool active, int accountStatus)
        {
            Account a = new Account();
            a.Active = active;
            a.Created = DateTime.Now;
            a.AccountStatusID = accountStatus;
            return DAL.CreateAccount(a);
        }
        */
        /*
        public static int GetAccountStatus(string username)
        {
            return DAL.GetAccount(username).AccountStatusID;
        }
        
        public static void ForcePasswordChange(string username, bool forceChange)
        {
            Account a = DAL.GetAccount(username);
            a.ForcePasswordChange = forceChange;
            DAL.UpdateRec(a, a.AccountID);
        }
        */

        public static void UpdateInviteUsage(string code, int custId)
        {
            Invite i = DAL.GetInvite(code);
            if (i != null)
            {
                i.Uses++;
                i.Active = true;
                i.CustID = custId;
                DAL.UpdateRec(i, i.InviteID);
            }
        }
        public static void DeleteInviteCode(string email)
        {
            Invite i = DAL.GetInviteByEmail(email);
            if (i != null)
            {
                try
                {

                    DAL.DeleteRec(i, i.InviteID);
                    LogHelper.Info(String.Format("Delete Invite code: InviteID= {0}", i.InviteID));
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Delete invite code fail:", ex);
                }

            }
        }

        public static int VerifyInviteCode(string code)
        {
            Invite i = DAL.GetInvite(code);
            if (i == null || i.Uses >= i.MaxUses)
                return 0;
            else
                return i.AccountTypeID;
        }

        public static string SMSVerifyInviteCode(string code, int inviteType)
        {
            Invite invite = DAL.GetInvite(code, inviteType);
            if (invite != null)
            {
                Invite i = DAL.GetInvite(code, (int)Types.InviteType.SMSVerify);
                if (i != null)
                {
                    i.Uses++;
                    i.Active = true;                   
                    DAL.UpdateRec(i, i.InviteID);
                }
                return invite.PhoneNumber;
            }
            return string.Empty;
        }

        public static bool InviteCodeIsValid(string code)
        {
            Invite invite = DAL.GetInvite(code);

            return (invite != null
                    && (invite.Status == (int)Types.UserInviteCodeStatusType.Approved)
                    && invite.Active == false
                   );
        }

        public static bool InviteCodeIsValid(string code, string email)
        {
            Invite invite = DAL.GetInvite(code, email);
            return (invite != null);
        }

        public static string InviteCodeName(string code, string email)
        {
            Invite invite = DAL.GetInvite(code, email);
            return invite.Name;
        }
        public static bool InviteCodeCheckStatus(string email)
        {

            bool result = false;
            Invite invite = DAL.GetInviteCodeByEmailForCheck(email);
            if (invite != null)
            {
                result = true;
            }
            return result;
        }

        public static string AddInviteCode(string email, string name, string lname,string phone=null, int inviteType = (int)Types.InviteType.User)
        {
            if (inviteType == (int)Types.InviteType.User){
                var i = DAL.GetInviteByPhoneNumber(email, false);
                if (i != null)
                    return i.Key;
            }
            else {
                var i = DAL.GetInviteForSMSVerify(phone,email);
                if (i != null){
                    if (i.Active ==false ){
                        return i.Key;
                    }
                    else {
                        return Types.FlagInvite.Verified.ToString();
                    }
                }
            }
            name = name == null ? string.Empty : name;
            lname = lname == null ? string.Empty : lname;
            phone = phone == null ? string.Empty : phone;
            email = email == null ? string.Empty : email;

            Invite invite = new Invite();          
            invite.InviteType = inviteType;
            invite.Key = GetRandomKey();
            invite.MaxUses = 1;
            invite.Uses = 0;
            invite.AccountTypeID = 115;
            invite.Email = email;
            invite.Name = name;
            invite.LName = lname;
            invite.Active = false;
            invite.FacebookToken = string.Empty;
            invite.CreateDate = DateTime.UtcNow;
            invite.PhoneNumber = phone;
            invite.Note = "verified on kuyam Web";
            return DAL.Add(invite).Key;
        }

        //Trong edit
        //public static string AddInviteCodeForSms(string email, string name, string lname, string phone, int inviteType = (int)Types.InviteType.SMSVerify){
        //    var i = DAL.GetInviteForSMSVerify(email, phone);
        //    if (i != null){
        //        return i.Key;
        //    }

        //    Invite invite = new Invite();
        //    invite.AccountID = 0;
        //    invite.InviteType = inviteType;
        //    invite.Key = GetRandomKey();
        //    invite.MaxUses = 1;
        //    invite.Uses = 0;
        //    invite.AccountTypeID = 115;
        //    invite.Email = email;
        //    invite.Name = name;
        //    invite.LName = lname;
        //    invite.Active = false;
        //    invite.FacebookToken = string.Empty;
        //    invite.CreateDate = DateTime.UtcNow;
        //    invite.PhoneNumber = phone;
        //    invite.Note = "verified on kuyam Web";
        //    return DAL.Add(invite).Key;
        //}
        

        public static Invite GetInviteCode(string email)
        {
            Invite invite = DAL.GetInviteByEmailForSignup(email);
            return invite;
        }

        public static Invite GetInviteCodeForLoadData(string email)
        {
            Invite invite = DAL.GetInviteByEmailForLoadData(email);
            return invite;
        }

        public static void UpdateInviteStatus(string email)
        {
            Invite i = DAL.GetInviteCodeByEmail(email);
            if (i != null)
            {
                i.Status = (int)Types.UserInviteCodeStatusType.Approved;
                DAL.UpdateRec(i, i.InviteID);
            }
        }

        public static void UpdateInviteStatusV2(string email)
        {
            Invite i = DAL.GetInviteCodeByEmail(email);
            if (i != null)
            {
                i.Status = (int)Types.UserInviteCodeStatusType.Active;
                DAL.UpdateRec(i, i.InviteID);
            }
        }

        public static void UpdateInviteType(string email)
        {
            Invite i = DAL.GetInviteCodeByEmail(email);
            if (i != null)
            {
                i.InviteType = -1;
                DAL.UpdateRec(i, i.InviteID);
            }
        }
        //--------------
        private static string GetRandomKey()
        {
            //not super thread safe, but whatev, this is just for beta
            string key = "";
            Invite invite = new Invite();
            while (invite != null)
            {
                Random random = new Random();
                key = random.Next(1, 16777215).ToString("x").ToUpper(); //max FFFFFF value
                invite = DAL.GetInvite(key);
            }
            return key;
        }

        public static void MakeSupport(string username)
        {
            DAL.AddUserToRoles(username, "support");
        }

        public static void MakeAdmin(string username)
        {
            MakeSupport(username);
            DAL.AddUserToRoles(username, "admin");
        }

        public static void MakeGod(string username)
        {
            MakeAdmin(username);
            DAL.AddUserToRoles(username, "god");
        }

    }
}
