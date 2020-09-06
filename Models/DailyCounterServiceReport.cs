using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class DailyCounterServiceReport
    {
        public int CounterNo { get; set; }
        public string ServiceType { get; set; }
        public int ServiceCount { get; set; }
        public string Status { get; set; }
    }
}
