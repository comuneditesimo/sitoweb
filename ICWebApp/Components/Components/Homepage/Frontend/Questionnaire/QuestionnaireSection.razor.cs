using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Homepage.Frontend.Questionnaire
{
    public partial class QuestionnaireSection
    {
        [Inject] IHOMEProvider HOMEProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }

        private HOME_Questionnaire? Item;
        private int Step = 0;

        protected override void OnInitialized()
        {
            Item = new HOME_Questionnaire();

            base.OnInitialized();
        }

        public async void Save()
        {
            if (Item != null)
            {
                Item.ID = Guid.NewGuid();
                Item.PageTitle = SessionWrapper.PageTitle;
                Item.CreationDate = DateTime.Now;

                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    Item.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;
                }

                await HOMEProvider.SetQuestinnaire(Item);
            }
        }
        public void OnStarsChanged(double value)
        {
            if (Item != null)
            {
                if(Step == 0)
                {
                    Step = 1;
                }

                Item.StarsDouble = value;

                if (Item.Stars != null)
                {
                    if (Item.Stars > 3)
                    {
                        Item.Question_Text_System_Text_Code = "HOME_QUESTIONNAIRE_QUESTION_POSITIV";
                    }
                    else if (Item.Stars <= 3)
                    {
                        Item.Question_Text_System_Text_Code = "HOME_QUESTIONNAIRE_QUESTION_NEGATIV";
                    }
                    else
                    {
                        Item.Question_Text_System_Text_Code = null;
                    }
                }

                StateHasChanged();
            }
        }
        public void Next()
        {
            Step++;

            if(Step == 3)
            {
                Save();
            }

            StateHasChanged();
        }
        public void Previous()
        {
            if (Step == 1)
            {
                Step = 0;

                if (Item != null)
                {
                    Item.Stars = null;
                }
            }
            else
            {
                Step--;
            }

            StateHasChanged();
        }
        public void QuestionSelected(string TEXT_System_Text_Code)
        {
            if (Item != null)
            {
                Item.Response_Text_System_Text_Code = TEXT_System_Text_Code;
                StateHasChanged();
            }
        }
    }
}
