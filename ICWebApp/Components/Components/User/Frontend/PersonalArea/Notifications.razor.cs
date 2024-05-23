using ICWebApp.Application.Interface.Provider;
using ICWebApp.Domain.Models.User;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ICWebApp.Components.Components.User.Frontend.PersonalArea
{
    public partial class Notifications
    {
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Parameter] public List<ServiceDataItem> Data { get; set; }

    }
}
