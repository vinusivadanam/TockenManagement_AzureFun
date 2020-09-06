using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class WaitingTimeDisplay
    {
        public string TokenNo { get; set; }
        public int EstimatedWaitingTime { get; set; }
        public TransactionTypeEnume TransactionType { get; set; }
    }
}
