using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Components.Homepage.Backend.Person
{
    public partial class PersonAvatar
    {
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IImageHelper ImageHelper { get; set; }
        [Parameter] public V_HOME_Person? Person { get; set; }
        [Parameter] public EventCallback<Guid> OnClick { get; set; }
        [Parameter] public bool EnableClicking { get; set; } = true;
        [Parameter] public string? SubTitle { get; set; }

        private async void AvatarClicked()
        {
            if (Person != null)
            {
                await OnClick.InvokeAsync(Person.ID);
            }
        }

    }
}
