using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Telerik.Blazor.Components.Editor;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Navigations;
using ICWebApp.Application.Settings;
using Syncfusion.Blazor.Schedule;
using Telerik.Blazor.Components;
using Syncfusion.Blazor.Popups;
using ICWebApp.Domain.DBModels;
using Stripe;

namespace ICWebApp.Components.Pages.Homepage.Backend.Appointments
{
    public partial class Index
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }

        private V_HOME_Appointment_Dates? EventData { get; set; }
        private SfSchedule<V_HOME_Appointment_Dates>? ScheduleRef { get; set; }
        private bool IsDataBusy { get; set; } = true;
        private List<V_HOME_Appointment_Dates> Data = new List<V_HOME_Appointment_Dates>();
        private List<V_HOME_Organisation> Organisations = new List<V_HOME_Organisation>();
        private List<V_HOME_Appointment_Type> Types = new List<V_HOME_Appointment_Type>();
        private List<V_HOME_Appointment_Kategorie> Kategories = new List<V_HOME_Appointment_Kategorie>();
        private Guid? CurrentLanguage { get; set; }
        private List<LANG_Languages>? Languages { get; set; }
        private bool EditWindowVisible = false;
        private HOME_Appointment? CurrentAppointment;
        private List<Guid>? SelectedThemes;
        private List<IEditorTool> Tools { get; set; } =
           new List<IEditorTool>()
           {
                new EditorButtonGroup(new Bold(), new Italic(), new Underline()),
                new EditorButtonGroup(new AlignLeft(), new AlignCenter(), new AlignRight()),
                new UnorderedList(),
                new EditorButtonGroup(new CreateLink(), new Unlink()),
                new InsertTable(),
                new DeleteTable(),
                new EditorButtonGroup(new AddRowBefore(), new AddRowAfter(), new DeleteRow(), new MergeCells(), new SplitCell())
           };
        public List<string> RemoveAttributes { get; set; } = new List<string>() { "data-id" };
        public List<string> StripTags { get; set; } = new List<string>() { "font" };

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_APPOINTMENTS");
            SessionWrapper.PageSubTitle = null;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Appointments", "MAINMENU_BACKEND_HOMEPAGE_APPOINTMENTS", null, null, true);

            await GetData();

            Languages = await LangProvider.GetAll();

            if (CurrentLanguage == null)
                CurrentLanguage = LanguageSettings.German;

            IsDataBusy = false;
            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private async Task<bool> GetData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Types = await HomeProvider.GetAppointment_Type(LangProvider.GetCurrentLanguageID());
                Kategories = await HomeProvider.GetAppointment_Kategories(LangProvider.GetCurrentLanguageID());
                Data = await HomeProvider.GetAppointmentsWithDates(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
                Organisations = await HomeProvider.GetOrganisations(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            return true;
        }
        private void CloseEditWindow()
        {
            EditWindowVisible = false;
            StateHasChanged();
        }
        private async void ActionBegin(ActionEventArgs<V_HOME_Appointment_Dates> args)
        {
            if(args.ActionType == ActionType.EventRemove)
            {
                args.Cancel = true;

                foreach (var dr in args.DeletedRecords) 
                {
                    var dates = await HomeProvider.GetAppointment_Dates(dr.HOME_AppointmentID);

                    if (dates.Count > 1)
                    {
                        await HomeProvider.RemoveAppointmentDate(dr.Dates_ID);
                    }
                    else
                    {
                        await HomeProvider.RemoveAppointment(dr.HOME_AppointmentID);
                    }
                }

                await GetData();
                StateHasChanged();
            }
            else if(args.ActionType == ActionType.EventChange ||
                    args.ActionType == ActionType.EventCreate)
            {
                args.Cancel = true;
            }
        }
        private void CellClick(CellClickEventArgs args)
        {
            args.Cancel = true;
        }
        private void EventClick(EventClickArgs<V_HOME_Appointment_Dates> args)
        {
            args.Cancel = true;
        }
        private async void CellDoubleClick(CellClickEventArgs args)
        {
            args.Cancel = true;
            IsDataBusy = true;
            StateHasChanged();

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                CurrentAppointment = new HOME_Appointment();

                CurrentAppointment.ID = Guid.NewGuid();
                CurrentAppointment.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;
                CurrentAppointment.CreationDate = DateTime.Now;

                SelectedThemes = new List<Guid>();
                if (Languages != null)
                {
                    foreach (var l in Languages)
                    {
                        if (CurrentAppointment.HOME_Appointment_Extended == null)
                        {
                            CurrentAppointment.HOME_Appointment_Extended = new List<HOME_Appointment_Extended>();
                        }

                        if (CurrentAppointment.HOME_Appointment_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                        {
                            var dataE = new HOME_Appointment_Extended()
                            {
                                ID = Guid.NewGuid(),
                                HOME_Appointment_ID = CurrentAppointment.ID,
                                LANG_Language_ID = l.ID
                            };

                            CurrentAppointment.HOME_Appointment_Extended.Add(dataE);
                        }
                    }
                }

                CurrentAppointment.HOME_Appointment_Dates = new List<HOME_Appointment_Dates>
                {
                    new HOME_Appointment_Dates()
                    {
                        ID = Guid.NewGuid(),
                        HOME_Appointment_ID = CurrentAppointment.ID,
                        DateFrom = new DateTime(args.StartTime.Year, args.StartTime.Month, args.StartTime.Day, DateTime.Now.Hour, 0, 0),
                        DateTo = new DateTime(args.StartTime.Year, args.StartTime.Month, args.StartTime.Day, DateTime.Now.AddHours(2).Hour, 0, 0)
                    }
                };

                IsDataBusy = false;
                EditWindowVisible = true;
                StateHasChanged();
            }
        }
        private async void EventDoubleClick(EventClickArgs<V_HOME_Appointment_Dates> args)
        {
            args.Cancel = true;

            var app = args.Event;

            await EditEvent(app);
        }
        private async void EventSave()
        {
            EditWindowVisible = false;
            IsDataBusy = true;
            StateHasChanged();

            if (CurrentAppointment != null && CurrentAppointment.HOME_Appointment_Type_ID != null)
            {
                CurrentAppointment.LastChangeDate = DateTime.Now;

                if(CurrentAppointment.Image != null)
                {
                    CurrentAppointment.FILE_FileImage_ID = CurrentAppointment.Image.ID;

                    await FileProvider.SetFileInfo(CurrentAppointment.Image);
                }

                if (Data.Where(p => p.DateTo <= DateTime.Now && p.HOME_Appointment_Type_ID == CurrentAppointment.HOME_Appointment_Type_ID && p.Highlight == true).Any())
                {
                    foreach (var d in Data.Where(p => p.DateTo <= DateTime.Now && p.HOME_Appointment_Type_ID == CurrentAppointment.HOME_Appointment_Type_ID && p.Highlight == true))
                    {
                        var dbItem = await HomeProvider.GetAppointment(d.HOME_AppointmentID);

                        if (dbItem != null)
                        {
                            dbItem.Highlight = false;

                            await HomeProvider.SetAppointment(dbItem);
                        }
                    }
                }

                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    var allowed = await HomeProvider.CheckAppointmentHighlight(CurrentAppointment.ID, CurrentAppointment.HOME_Appointment_Type_ID.Value, SessionWrapper.AUTH_Municipality_ID.Value);

                    if (!allowed && CurrentAppointment.Highlight)
                    {

                        if (CurrentAppointment.HOME_Appointment_Type_ID == HOMEAppointmentTypes.Appointment)
                        {
                            if (!await Dialogs.ConfirmAsync(TextProvider.Get("BACKEND_HOMEPAGE_APPOINTMENT_HIGHLIGHT_NOT_POSSIBLE"), TextProvider.Get("WARNING")))
                                return;
                        }
                        else
                        {
                            if (!await Dialogs.ConfirmAsync(TextProvider.Get("BACKEND_HOMEPAGE_EVENT_HIGHLIGHT_NOT_POSSIBLE"), TextProvider.Get("WARNING")))
                                return;
                        }

                        CurrentAppointment.Highlight = false;
                    }
                }

                await HomeProvider.SetAppointment(CurrentAppointment);

                foreach(var ext in CurrentAppointment.HOME_Appointment_Extended)
                {
                    await HomeProvider.SetAppointmentExtended(ext);
                }

                var existingDates = await HomeProvider.GetAppointment_Dates(CurrentAppointment.ID);

                foreach (HOME_Appointment_Dates date in CurrentAppointment.HOME_Appointment_Dates)
                {
                    if (date.DateFrom != null && date.DateTo != null && date.DateTo.Value < date.DateFrom.Value.AddMinutes(15))
                    {
                        date.DateTo = null;
                    }
                }

                if(existingDates != null)
                {
                    var dToDelete = existingDates.Where(p => !CurrentAppointment.HOME_Appointment_Dates.Select(x => x.ID).Contains(p.ID)).ToList();
                    var dToAdd = CurrentAppointment.HOME_Appointment_Dates.Where(p => !existingDates.Select(x => x.ID).Contains(p.ID)).ToList();

                    foreach(var d in dToDelete)
                    {
                        await HomeProvider.RemoveAppointmentDate(d);
                    }

                    foreach(var d in dToAdd)
                    {
                        await HomeProvider.SetAppointmentDate(d);
                    }
                }
                
                existingDates = await HomeProvider.GetAppointment_Dates(CurrentAppointment.ID);

                if (existingDates != null)
                {
                    foreach (var d in existingDates)
                    {
                        if (d.DateFrom == null || d.DateTo == null)
                        {
                            await HomeProvider.RemoveAppointmentDate(d);
                        }
                    }
                }

                foreach (var d in CurrentAppointment.HOME_Appointment_Dates)
                {
                    await HomeProvider.SetAppointmentDate(d);
                }

                var existingThemes = await HomeProvider.GetAppointment_Themes(CurrentAppointment.ID);

                if (existingThemes != null && SelectedThemes != null)
                {
                    var themesToDelete = existingThemes.Where(p => p.HOME_Theme_ID != null && !SelectedThemes.Contains(p.HOME_Theme_ID.Value)).ToList();
                    var themesToAdd = SelectedThemes.Where(p => !existingThemes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID).Contains(p)).ToList();

                    foreach (var t in themesToDelete)
                    {
                        await HomeProvider.RemoveAppointmentTheme(t);
                    }

                    foreach (var t in themesToAdd)
                    {
                        var newItem = new HOME_Appointment_Theme();

                        newItem.ID = Guid.NewGuid();
                        newItem.HOME_Theme_ID = t;
                        newItem.HOME_Appointment_ID = CurrentAppointment.ID;

                        await HomeProvider.SetAppointmentTheme(newItem);
                    }
                }
            }

            await GetData();

            IsDataBusy = false;
            StateHasChanged();
        }
        private async Task EditEvent(V_HOME_Appointment_Dates? Date)
        {
            if (Date != null)
            {
                CurrentAppointment = await HomeProvider.GetAppointment(Date.HOME_AppointmentID);

                if (CurrentAppointment != null)
                {
                    CurrentAppointment.HOME_Appointment_Extended = await HomeProvider.GetAppointment_Extended(Date.HOME_AppointmentID);
                    var dates = await HomeProvider.GetAppointment_Dates(Date.HOME_AppointmentID);

                    CurrentAppointment.HOME_Appointment_Dates = dates.OrderBy(p => p.DateFrom).ToList();

                    var themes = await HomeProvider.GetAppointment_Themes(Date.HOME_AppointmentID);

                    SelectedThemes = themes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID!.Value).ToList();

                    if (CurrentAppointment.FILE_FileImage_ID != null)
                    {
                        FILE_FileInfo? fileInfo = await FileProvider.GetFileInfoAsync(CurrentAppointment.FILE_FileImage_ID.Value);
                        if (fileInfo != null)
                        {
                            CurrentAppointment.Image = fileInfo;
                        }
                    }
                }

                EditWindowVisible = true;
                StateHasChanged();
            }
        }
        private async Task DeleteEvent(V_HOME_Appointment_Dates? Date)
        {
            if (!await Dialogs.ConfirmAsync(TextProvider.Get("HOME_APPOINTMENT_DELETE_ARE_YOU_SURE").Replace("{0}", Date?.Title ?? ""), TextProvider.Get("WARNING")))
            {
                return;
            }

            BusyIndicatorService.IsBusy = true;
            StateHasChanged();

            if (Date != null)
            {
                await HomeProvider.RemoveAppointment(Date.HOME_AppointmentID);
                await GetData();
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
        }
        private async Task OnOpen(BeforeOpenCloseMenuEventArgs<MenuItem> args)
        {
            V_HOME_Appointment_Dates? selectedCell = null;
            if (args.ParentItem == null && ScheduleRef != null)
            {
                ElementInfo<V_HOME_Appointment_Dates> ElementData = await ScheduleRef.GetElementInfoAsync((int)args.Left, (int)args.Top);
                if (ElementData.ElementType == ElementType.Event)
                {
                    if (ElementData.EventData == null)
                    {
                        args.Cancel = true;
                    }
                    if (ElementData.EventData != null)
                    {
                        selectedCell = ElementData.EventData;
                    }
                }
                else
                {
                    args.Cancel = true;
                }
            }
            EventData = selectedCell;
        }
        private async Task OnItemSelected(MenuEventArgs<MenuItem> args)
        {
            var SelectedMenuItem = args.Item.Id;
            switch (SelectedMenuItem)
            {
                case "Edit":
                    await EditEvent(EventData);
                    break;
                case "Delete":
                    await DeleteEvent(EventData);
                    break;
            }
        }
    }
}
