﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.DBModels
{
    public partial class V_TASK_Bucket
    {
        [NotMapped] public bool ShowCompleted { get; set; } = false;
    }
}