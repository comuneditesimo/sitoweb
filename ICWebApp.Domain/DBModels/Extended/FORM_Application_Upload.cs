using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class FORM_Application_Upload
    {
        [NotMapped] public List<FILE_FileInfo>? CACH_UploadFiles { get; set; }
        [NotMapped] public string? ERROR_CODE { get; set; }
    }
}
