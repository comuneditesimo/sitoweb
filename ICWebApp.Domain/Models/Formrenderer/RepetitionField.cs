using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Formrenderer
{
    public class RepetitionField
    {
        public RepetitionField(Guid? ApplicationFieldID, FORM_Definition_Field? DefinitionField, long? RepetitionCount)
        {
            this.ApplicationFieldID = ApplicationFieldID;
            this.DefinitionField = DefinitionField;
            this.RepetitionCount = RepetitionCount;
        }
        public Guid? ApplicationFieldID { get; set; }
        public FORM_Definition_Field? DefinitionField { get; set; }
        public long? RepetitionCount { get; set; }
    }
}
