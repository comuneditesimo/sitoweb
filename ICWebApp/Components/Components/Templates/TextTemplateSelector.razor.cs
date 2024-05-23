using DocumentFormat.OpenXml.Presentation;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Textvorlagen;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.RichTextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telerik.Blazor.Components;
using Telerik.Blazor.Components.Editor;

namespace ICWebApp.Components.Components.Templates
{
    public partial class TextTemplateSelector
    {
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }

        [Parameter] public string ExternalContext { get; set; }
        [Parameter] public string ExternalID { get; set; }
        [Parameter] public TextItem TextItem { get; set; }
        [Parameter] public bool IsDocument { get; set; } = false;

        private List<V_TEXT_Template> Data = new List<V_TEXT_Template>();
        private V_TEXT_Template? SelectedTemplate;
        private string? PreviousExternalID;
        private List<LANG_Languages>? Languages { get; set; }
        private Guid? CurrentLanguage { get; set; }
        
        private List<ToolbarItemModel> Tools = new List<ToolbarItemModel>()
        {
            new ToolbarItemModel() { Command = ToolbarCommand.Bold },
            new ToolbarItemModel() { Command = ToolbarCommand.Italic },
            new ToolbarItemModel() { Command = ToolbarCommand.Underline },
            new ToolbarItemModel() { Command = ToolbarCommand.Separator },
            new ToolbarItemModel() { Command = ToolbarCommand.OrderedList },
            new ToolbarItemModel() { Command = ToolbarCommand.UnorderedList },
            new ToolbarItemModel() { Command = ToolbarCommand.Separator },
            new ToolbarItemModel() { Command = ToolbarCommand.Outdent },
            new ToolbarItemModel() { Command = ToolbarCommand.Indent },
            new ToolbarItemModel() { Command = ToolbarCommand.Separator },
            new ToolbarItemModel() { Command = ToolbarCommand.CreateLink },
            new ToolbarItemModel() { Command = ToolbarCommand.CreateTable },
            new ToolbarItemModel() { Command = ToolbarCommand.Separator },
            new ToolbarItemModel() { Command = ToolbarCommand.Undo },
            new ToolbarItemModel() { Command = ToolbarCommand.Redo }
        };
        private bool IsDataBusy = true;
        TelerikEditor? GermanEditor;
        TelerikEditor? ItalianEditor;

        protected override async Task OnInitializedAsync()
        {
            Languages = await LangProvider.GetAll();

            if (Languages != null && Languages.FirstOrDefault() != null)
            {
                CurrentLanguage = Languages.FirstOrDefault().ID;
            }

            StateHasChanged();
            await base.OnInitializedAsync();
        }
        protected override async Task OnParametersSetAsync()
        {
            if(PreviousExternalID != ExternalID)
            {
                PreviousExternalID = ExternalID;
                Data = new List<V_TEXT_Template>();
                SelectedTemplate = null;

                if (SessionWrapper.AUTH_Municipality_ID != null && !string.IsNullOrEmpty(ExternalID))
                {
                    var templates = await TextProvider.GetTemplates(SessionWrapper.AUTH_Municipality_ID.Value, ExternalContext, ExternalID, IsDocument);

                    if(templates != null && templates.Count() > 0)
                    {
                        Data = templates;
                        SelectedTemplate = templates.FirstOrDefault();

                        await SelectionChanged(SelectedTemplate);
                    }
                    else
                    {
                        IsDataBusy = true;
                        StateHasChanged();

                        List<TemplateItem> TemplateList = new List<TemplateItem>();
                        var defaultTextDE = await TextProvider.GetDefaultTemplate(LanguageSettings.German, IsDocument);
                        var defaultTextIT = await TextProvider.GetDefaultTemplate(LanguageSettings.Italian, IsDocument);
                        var defaultTextLA = await TextProvider.GetDefaultTemplate(LanguageSettings.Ladinisch, IsDocument);

                        if (defaultTextDE != null) 
                        {
                            TemplateItem itemDE = new TemplateItem();

                            itemDE.LANG_Language_ID = LanguageSettings.German;
                            itemDE.Content = defaultTextDE.Content;

                            TemplateList.Add(itemDE);
                        }

                        if (defaultTextIT != null)
                        {
                            TemplateItem itemIT = new TemplateItem();

                            itemIT.LANG_Language_ID = LanguageSettings.Italian;
                            itemIT.Content = defaultTextIT.Content;

                            TemplateList.Add(itemIT);
                        }

                        if (defaultTextLA != null)
                        {
                            TemplateItem itemLA = new TemplateItem();

                            itemLA.LANG_Language_ID = LanguageSettings.Ladinisch;
                            itemLA.Content = defaultTextLA.Content;

                            TemplateList.Add(itemLA);
                        }

                        TemplateSelectionChanged(TemplateList);   
                    }
                }

                IsDataBusy = false;
                StateHasChanged();
            }
            await base.OnParametersSetAsync();
        }
        private async void SelectionChanged(SelectEventArgs<V_TEXT_Template> args)
        {
            V_TEXT_Template? _value = args.ItemData;

            if (_value != null)
            {
                await SelectionChanged(_value);
            }
        }
        private async Task SelectionChanged(V_TEXT_Template? Value)
        {
            if (Value != null)
            {
                IsDataBusy = true;
                StateHasChanged();

                List<TemplateItem> TemplateList = new List<TemplateItem>();
                TEXT_Template_Extended? _templateExtendedDE = null;
                TEXT_Template_Extended? _templateExtendedIT = null;
                TEXT_Template_Extended? _templateExtendedLA = null;

                var dataExt = await TextProvider.GetTemplateExtended(Value.ID);
                if (dataExt != null)
                {
                    _templateExtendedDE = dataExt.FirstOrDefault(p => p.LANG_Languages_ID == LanguageSettings.German);
                    _templateExtendedIT = dataExt.FirstOrDefault(p => p.LANG_Languages_ID == LanguageSettings.Italian);
                    _templateExtendedLA = dataExt.FirstOrDefault(p => p.LANG_Languages_ID == LanguageSettings.Ladinisch);
                }

                if (_templateExtendedDE != null)
                {
                    TemplateItem itemDE = new TemplateItem();

                    itemDE.LANG_Language_ID = LanguageSettings.German;
                    itemDE.Content = _templateExtendedDE.Content;

                    TemplateList.Add(itemDE);
                }
                else
                {
                    var defaultTextDE = await TextProvider.GetDefaultTemplate(LanguageSettings.German, IsDocument);

                    if (defaultTextDE != null)
                    {
                        TemplateItem itemDE = new TemplateItem();

                        itemDE.LANG_Language_ID = LanguageSettings.German;
                        itemDE.Content = defaultTextDE.Content;

                        TemplateList.Add(itemDE);
                    }
                }

                if (_templateExtendedIT != null)
                {
                    TemplateItem itemIT = new TemplateItem();

                    itemIT.LANG_Language_ID = LanguageSettings.Italian;
                    itemIT.Content = _templateExtendedIT.Content;

                    TemplateList.Add(itemIT);
                }
                else
                {
                    var defaultTextIT = await TextProvider.GetDefaultTemplate(LanguageSettings.Italian, IsDocument);

                    if (defaultTextIT != null)
                    {
                        TemplateItem itemIT = new TemplateItem();

                        itemIT.LANG_Language_ID = LanguageSettings.Italian;
                        itemIT.Content = defaultTextIT.Content;

                        TemplateList.Add(itemIT);
                    }
                }

                if (_templateExtendedLA != null)
                {
                    TemplateItem itemLA = new TemplateItem();

                    itemLA.LANG_Language_ID = LanguageSettings.Ladinisch;
                    itemLA.Content = _templateExtendedLA.Content;

                    TemplateList.Add(itemLA);
                }
                else
                {
                    var defaultTextLA = await TextProvider.GetDefaultTemplate(LanguageSettings.Ladinisch, IsDocument);

                    if (defaultTextLA != null)
                    {
                        TemplateItem itemLA = new TemplateItem();

                        itemLA.LANG_Language_ID = LanguageSettings.Ladinisch;
                        itemLA.Content = defaultTextLA.Content;

                        TemplateList.Add(itemLA);
                    }
                }

                TemplateSelectionChanged(TemplateList);
                IsDataBusy = false;
                StateHasChanged();
            }
        }
        private bool TemplateSelectionChanged(List<TemplateItem> Templates)
        {
            if (Templates != null && Templates.Count() > 0)
            {
                var templateDE = Templates.FirstOrDefault(p => p.LANG_Language_ID == LanguageSettings.German);
                var templateIT = Templates.FirstOrDefault(p => p.LANG_Language_ID == LanguageSettings.Italian);
                var templateLA = Templates.FirstOrDefault(p => p.LANG_Language_ID == LanguageSettings.Ladinisch);

                if (templateDE != null)
                {
                    TextItem.German = templateDE.Content;
                }
                else
                {
                    TextItem.German = null;
                }

                if (templateIT != null)
                {
                    TextItem.Italian = templateIT.Content;
                }
                else
                {
                    TextItem.Italian = null;
                }

                if (templateLA != null)
                {
                    TextItem.Ladinisch = templateLA.Content;
                }
                else
                {
                    TextItem.Ladinisch = null;
                }

                StateHasChanged();
            }

            return true;
        }
    }
}