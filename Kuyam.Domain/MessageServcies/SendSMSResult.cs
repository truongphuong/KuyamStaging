using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.MessageServcies
{
    public class SendSMSResult
    {

        public SendSMSResult()
        {
        }


        public SendSMSResult(EResultStatus statusEnum, string message, object dataReturn = null)
        {
            this.Status = (int)statusEnum;
            this.Message = message;
            this.Data = dataReturn;
        }

        #region Public Properties
        public int Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        #endregion
    }
}
