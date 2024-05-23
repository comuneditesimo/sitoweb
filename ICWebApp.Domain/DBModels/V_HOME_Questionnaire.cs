﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Keyless]
public partial class V_HOME_Questionnaire
{
    public Guid ID { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreationDate { get; set; }

    public string Question_Text_System_Text_Code { get; set; }

    public string Response_Text_System_Text_Code { get; set; }

    public string AdditionalInformation { get; set; }

    public int? Stars { get; set; }

    public string PageTitle { get; set; }

    public Guid LANG_LanguagesID { get; set; }

    public string Question { get; set; }

    public string Reponse { get; set; }
}