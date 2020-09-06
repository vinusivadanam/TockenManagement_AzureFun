using System;

namespace Models
{
    public class Customer
    {
        public string CustomerId { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string AccountNo { get; set; }
        public string SocialNo { get; set; }
        public CustomerTypeEnume CustomerType { get; set; }
    }
}
