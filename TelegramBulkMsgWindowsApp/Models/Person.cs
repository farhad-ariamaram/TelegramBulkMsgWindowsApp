using System;
using System.Collections.Generic;

#nullable disable

namespace TelegramBulkMsgWindowsApp.Models
{
    public partial class Person
    {
        public long Id { get; set; }
        public string Phone { get; set; }
        public DateTime? SendDate { get; set; }
        public string LastSeen { get; set; }
        public bool? HasTelegram { get; set; }
    }
}
