using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuyam.Database;
using Kuyam.Repository.Interface;
using System.Data.SqlClient;
using System.Data;
using System.Data.Entity;

namespace Kuyam.Domain.SmsServices
{
    public class SmsServices : ISmsServices
    {
        #region Private Fields
        private readonly IRepository<SMSMessage> _smsRepository;
        private readonly IRepository<Cust> _custRepository;
        private readonly DbContext _dbContext;

        #endregion
        public SmsServices(IRepository<SMSMessage> smsRepository, IRepository<Cust> custRepository,
            DbContext dbContext)
        {
            _smsRepository = smsRepository;
            _custRepository = custRepository;
            this._dbContext = dbContext;
        }

        public List<SMSMessage> GetAllMessage(int cusId)
        {
            var query = (from sms in _smsRepository.Table
                         where sms.CustId == cusId && sms.DeliveryStatus != (int)Types.DeliveryStatus.Delete

                         select new
                                 {
                                     sms.CustId,                                    
                                     sms.MessageId,
                                     sms.Message,
                                     sms.DateTime,
                                     sms.DeliveryStatus,
                                     sms.DestinationAddress,
                                     sms.ReceivedStatus,
                                     sms.SenderAddress,
                                     sms.DecriptionStatus,
                                     sms.ErrorCode
                                 }).Distinct();

            var result = query.ToList<dynamic>();
           
            return result.Select(m => new SMSMessage
            {
                DateTime = DateTimeUltility.ConvertToUserTime(m.DateTime, DateTimeKind.Utc),
                CustId = m.CustId,
                Message = m.Message,
                MessageId = m.MessageId,
                DeliveryStatus = m.DeliveryStatus,
                DestinationAddress = m.DestinationAddress,
                ReceivedStatus = m.ReceivedStatus,
                SenderAddress = m.SenderAddress,
                DecriptionStatus = m.DecriptionStatus,
                ErrorCode = m.ErrorCode
            }).OrderBy(m => m.DateTime).ToList();
        }

        public List<string> GetAll(int headerCount)
        {
            var query = (from s in _smsRepository.Table
                         where s.DeliveryStatus != (int)Types.DeliveryStatus.Delete
                         select new { s.MessageId, s.DateTime }).Distinct();

            return query.OrderByDescending(m => m.DateTime).Select(m => m.MessageId).Take(headerCount).ToList();
        }

        public bool AddMessage(SMSMessage smsMessage)
        {
            try
            {
                _smsRepository.Insert(smsMessage);
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }

        }
        public SMSMessage GetMessageByMessageIdAndCustId(string messageId, int cusId)
        {
            return _smsRepository.Table.FirstOrDefault(a => a.MessageId == messageId && a.CustId == cusId && a.DeliveryStatus != (int)Types.DeliveryStatus.Delete);
        }


        public bool UpdateMessage(SMSMessage smsMessage)
        {
            try
            {
                _smsRepository.Update(smsMessage);
                return true;
            }
            catch (Exception exException)
            {
                //write log here
                return false;
            }
        }

        public List<Cust> GetAllCust()
        {
            return _custRepository.Table.Where(m => m.Status == (int)Types.UserStatusType.Active).ToList();

        }

        public int DeleteSmsByStatus(int status = (int)Types.DeliveryStatus.Temp)
        {
            var param = new SqlParameter();
            param.ParameterName = "Status";
            param.Value = status;
            param.DbType = DbType.Int32;
            return _dbContext.ExecuteSqlCommand("exec [DeleteTempSmsByStatus]  @Status", null, param);
        }
    }
}
