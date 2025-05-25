using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD_Assignment.Models;

public class BlockAttemptLog
{
    public string IpAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string CountryCode { get; set; }
    public bool IsBlocked { get; set; }
    public string UserAgent { get; set; }
}
