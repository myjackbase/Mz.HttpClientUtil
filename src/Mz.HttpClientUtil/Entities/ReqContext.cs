using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mz.HttpClientUtil
{
    public class ReqContext
    {
        public DateTime SendStartTime { get; set; }

        public DateTime? ReceiveEndTime { get; set; }

        public DateTime? ExceptionTime { get; set; }

        public object RequestModel { get; set; }

        public object ResponseModel { get; set; }

        public long ResponseSize { get; set; }

        public bool ExceptionHandled { get; set; }

        public Dictionary<string, object> Items { get; set; }

        public ReqContext()
        {
            Items = new Dictionary<string, object>();
            ExceptionHandled = false;
        }
    }
}
