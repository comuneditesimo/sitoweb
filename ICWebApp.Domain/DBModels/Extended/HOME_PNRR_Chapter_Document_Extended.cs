using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.DBModels
{
    public partial class HOME_PNRR_Chapter_Document_Extended
    {
        [NotMapped] public FILE_FileInfo? File { get; set; }
    }
}
