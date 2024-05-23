using ICWebApp.Application.Interface.Helper;
using ICWebApp.Domain.DBModels;

namespace ICWebApp.Application.Helper;

public class CreateFormDefinitionHelper : ICreateFormDefinitionHelper
{
    //On save formbuilder
    private readonly List<FormDefinitionCacheElement> _cache = new List<FormDefinitionCacheElement>();
    
    private class FormDefinitionCacheElement
    {
        public FormDefinitionCacheElement(FORM_Definition data,
            List<FORM_Definition_Property> properties,
            List<FORM_Definition_Event> events, List<FORM_Definition_Tasks> tasks,
            List<FORM_Definition_Deadlines> deadlines, List<FORM_Definition_Upload> uploads,
            List<FORM_Definition_Ressources> resources, List<FORM_Definition_Additional_FORM> additionalForms,
            List<FORM_Definition_Reminder> reminders, List<FORM_Definition_Signings> signings,
            List<FORM_Definition_Payment_Position> paymentPositions, List<V_FORM_Definition_Municipal_Field> munFields)
        {
            _id = data.ID;
            _formDefinition = data;
            _properties = properties;
            _events = events;
            _tasks = tasks;
            _deadlines = deadlines;
            _uploads = uploads;
            _resources = resources;
            _additionalForms = additionalForms;
            _reminders = reminders;
            _signings = signings;
            _paymentPositions = paymentPositions;
            _munFields = munFields;
        }

        private Guid _id;
        private FORM_Definition _formDefinition;
        private List<FORM_Definition_Property> _properties;
        private List<FORM_Definition_Event> _events;
        private List<FORM_Definition_Tasks> _tasks;
        private List<FORM_Definition_Deadlines> _deadlines;
        private List<FORM_Definition_Upload> _uploads;
        private List<FORM_Definition_Ressources> _resources;
        private List<FORM_Definition_Additional_FORM> _additionalForms;
        private List<FORM_Definition_Reminder> _reminders;
        private List<FORM_Definition_Signings> _signings;
        private List<FORM_Definition_Payment_Position> _paymentPositions;
        private List<V_FORM_Definition_Municipal_Field> _munFields;
    }
}