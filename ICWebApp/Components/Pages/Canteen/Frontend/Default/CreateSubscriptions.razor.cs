﻿using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Canteen.Frontend.Default
{
    public partial class CreateSubscriptions
    {
        [Inject] IMyCivisService MyCivisService { get; set; }

        [Parameter] public string? FamilyID { get; set; }
        [Parameter] public string? Edit { get; set; }

        protected override void OnInitialized()
        {
            MyCivisService.Enabled = false;
            StateHasChanged();

            base.OnInitialized();
        }
    }
}