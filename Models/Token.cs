using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class Token
    {
        public string TokenNo { get; set; }
        public TokenStatusEnume Status { get; set; }
        public int InitialEstimatedWaitingTime { get; set; }
        public int CurrentEstimatedWaitingTime { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public TransactionTypeEnume TransactionType { get; set; }
        public Customer CustomerInfo { get; set; }
        public int? CounterNo { get; set; }
    }
}
