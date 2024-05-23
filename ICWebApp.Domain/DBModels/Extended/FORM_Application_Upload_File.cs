using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class FORM_Application_Upload_File
    {
        [NotMapped] public FILE_FileInfo? FILE_FileInfo { get; set; }
    }
}
