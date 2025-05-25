using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD_Assignment.Models;

public class TemporalBlock : BlockedCountry
{
    public DateTime ExpiryTime { get; set; }
}
