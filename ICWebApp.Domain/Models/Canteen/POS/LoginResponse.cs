﻿using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Canteen.POS
{
    public class LoginResponse
    {
        public string Response { get; set; }
        public string Code { get; set; }
        public string? SessionToken { get; set; }
    }
}