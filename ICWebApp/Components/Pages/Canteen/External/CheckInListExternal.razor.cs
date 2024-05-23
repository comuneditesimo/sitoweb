using Azure.Core;
using DocumentFormat.OpenXml.Spreadsheet;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Syncfusion.Blazor.Lists.Internal;

namespace ICWebApp.Components.Pages.Canteen.External;

public partial class CheckInListExternal
{
    [Inject] public IBusyIndicatorService? BusyIndicatorService { get; set; }
    [Inject] public ISessionWrapper? SessioWrapper { get; set; }
    [Inject] public ICANTEENProvider? CanteenProvider { get; set; }
    [Inject] public ITEXTProvider? TextProvider { get; set; }

    [Parameter] public string? AccessToken { get; set; }

    CANTEEN_Canteen? Canteen { get; set; }
    List<V_CANTEEN_Subscriber_Movements> CanteenMovements { get; set; } = new List<V_CANTEEN_Subscriber_Movements>();
    List<string> ClassList { get; set; } = new List<string>();
    List<string> SchoolList { get; set; } = new List<string>();
    List<string> SelectedClassList { get; set; } = new List<string>();
    string SelectedSchool { get; set; } = "";
    string SearchTerm { get; set; } = "";

    private List<V_CANTEEN_Subscriber_Movements> _canteenMovementsFiltered = new List<V_CANTEEN_Subscriber_Movements>();

    protected override async Task OnParametersSetAsync()
    {
        if (Canteen == null ||
            string.IsNullOrEmpty(AccessToken) ||
            Canteen.ExternalCheckInAccessToken == null ||
            Canteen.ExternalCheckInAccessToken.Value.ToString().Trim().ToLower() != AccessToken.Trim().ToLower())
        {
            await GetData();

            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = false;
                StateHasChanged();
            }
        }

        await base.OnParametersSetAsync();
    }
    private async Task<CANTEEN_Canteen?> GetData()
    {
        Guid? accessTokenID = null;
        DateTime date = DateTime.Today;
        CANTEEN_Canteen? canteen = null;
        List<V_CANTEEN_Subscriber_Movements> canteenMovements = new List<V_CANTEEN_Subscriber_Movements>();
        List<string> schoolList = new List<string>();
        List<string> classList = new List<string>();
        string pageTitel = "";
        string pageSubTitel = "";

#if DEBUG
        date = DateTime.Parse("2023-09-07 00:00:00.000");
#endif
        try
        {
            // Titel setzen (Falls nichts gefunden wurde
            if (TextProvider != null)
            {
                pageTitel = TextProvider.Get("EXTERNALACCESSLINK_SUBTITEL_ERROR_CANTEENNOTFOUND");
                pageSubTitel = string.Empty;
            }
            if (CanteenProvider != null)
            {
                // Mensa lesen
                if (!string.IsNullOrEmpty(AccessToken))
                {
                    accessTokenID = Guid.Parse(AccessToken);
                }
                if (accessTokenID != null)
                {
                    canteen = await CanteenProvider.GetCanteenByAccessToken(accessTokenID.Value);
                }
                // Liste von Mensa befüllen
                if (canteen != null)
                {
                    canteenMovements = await CanteenProvider.GetTodaysCanteenMovements(canteen.ID, date);
                    // Filterlisten setzen
                    if (canteenMovements.Any())
                    {
                        schoolList = canteenMovements.Select(p => p.SchoolName).Distinct().ToList();
                        classList = canteenMovements.Select(p => p.SchoolClassNumber).Distinct().ToList();
                    }
                    // Titelleiste setzen
                    if (TextProvider != null)
                    {
                        pageTitel = TextProvider.Get("EXTERNALACCESSLINK_TITEL_CANTEEN").Replace("{MunicipalName}", canteen.Name);
                        pageSubTitel = TextProvider.Get("EXTERNALACCESSLINK_SUBTITEL_CANTEEN").Replace("{Date}", date.ToString("d"));
                    }
                }
            }
        }
        catch
        {
        }
        if (CanteenProvider != null)
        {
            // Mensa lesen
            if (!string.IsNullOrEmpty(AccessToken))
            {
                accessTokenID = Guid.Parse(AccessToken);
            }
            if (accessTokenID != null)
            {
                canteen = await CanteenProvider.GetCanteenByAccessToken(accessTokenID.Value);
            }
            // Liste von Mensa befüllen
            if (canteen != null)
            {
                canteenMovements = await CanteenProvider.GetTodaysCanteenMovements(canteen.ID, date);
                // Filterlisten setzen
                if (canteenMovements.Any())
                {
                    schoolList = canteenMovements.Select(p => p.SchoolName).Distinct().ToList();
                    classList = canteenMovements.Select(p => p.SchoolClassNumber).Distinct().ToList();
                }
                // Titelleiste setzen
                if (TextProvider != null)
                {
                    pageTitel = TextProvider.Get("EXTERNALACCESSLINK_TITEL_CANTEEN").Replace("{MunicipalName}", canteen.Name);
                    pageSubTitel = TextProvider.Get("EXTERNALACCESSLINK_SUBTITEL_CANTEEN").Replace("{Date}", date.ToString("d"));
                }
            }
        }
        // Variablen setzen
        Canteen = canteen;
        CanteenMovements = canteenMovements;
        SchoolList = schoolList;
        ClassList = classList;
        SelectedSchool = "";
        SelectedClassList = new List<string>();

        if (SessioWrapper != null)
        {
            SessioWrapper.PageTitle = pageTitel;
            SessioWrapper.PageSubTitle = pageSubTitel;
        }

        await FilterMovements();

        return canteen;
    }
    private async Task OnFilterChanged()
    {
        await FilterMovements();
    }
    private async Task FilterMovements()
    {
        await Task.Run(async () =>
        {
            List<V_CANTEEN_Subscriber_Movements> canteenMovementsFiltered = new List<V_CANTEEN_Subscriber_Movements>();
            canteenMovementsFiltered.AddRange(CanteenMovements);
            if (!string.IsNullOrEmpty(SelectedSchool))
            {
                canteenMovementsFiltered = canteenMovementsFiltered.Where(p => p.SchoolName.Trim().ToLower() == SelectedSchool.Trim().ToLower()).ToList();
            }
            if (SelectedClassList != null && SelectedClassList.Any())
            {
                canteenMovementsFiltered = canteenMovementsFiltered.Where(p => SelectedClassList.Contains(p.SchoolClassNumber)).ToList();
            }
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                canteenMovementsFiltered = canteenMovementsFiltered.Where(p => p.FirstName.Trim().ToLower().Contains(SearchTerm.Trim().ToLower()) ||
                                                                               p.LastName.Trim().ToLower().Contains(SearchTerm.Trim().ToLower()) ||
                                                                               p.FirstNameLastName.Trim().ToLower().Contains(SearchTerm.Trim().ToLower()) ||
                                                                               p.LastNameFirstName.Trim().ToLower().Contains(SearchTerm.Trim().ToLower()) ||
                                                                               (p.Child_DateOfBirth != null && p.Child_DateOfBirth.Value.ToString("d").Trim().ToLower().Contains(SearchTerm.Trim().ToLower())) ||
                                                                               p.SchoolName.Trim().ToLower().Contains(SearchTerm.Trim().ToLower()) ||
                                                                               p.SchoolClass.Trim().ToLower().Contains(SearchTerm.Trim().ToLower()) ||
                                                                               p.School.Trim().ToLower().Contains(SearchTerm.Trim().ToLower())).ToList();
            }
            _canteenMovementsFiltered = canteenMovementsFiltered;
            await InvokeAsync(() => StateHasChanged());
        });
    }
    private async Task CheckIn(Guid movementId)
    {
        if (CanteenProvider != null)
        {
            await CanteenProvider.ManualCheckIn(movementId);
            await GetData();
        }
    }
    private async Task CheckOut(Guid movementId)
    {
        if (CanteenProvider != null)
        {
            await CanteenProvider.ManualCheckOut(movementId);
            await GetData();
        }
    }
    private async Task ClearSearchTermFilter()
    {
        SearchTerm = "";
        StateHasChanged();

        await FilterMovements();
    }
}