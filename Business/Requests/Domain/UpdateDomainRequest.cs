﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Requests.Domain
{
    public class UpdateDomainRequest
    {
        public int Id { get; set; }
        public DateTime EndTime { get; set; }
        
    }
}
