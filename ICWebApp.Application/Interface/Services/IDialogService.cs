using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface IDialogService
    {
        public bool ShowDialogWindow { get; set; }
        public string HeaderTitle { get; }
        public string Content { get; }
        public bool ShowCloseButton { get; }
        public string Confirm_Button_Text { get; }
        public string Cancel_Button_Text { get; }
        public event Action<bool>? OnStateChange;
        public Task<bool> OpenDialogWindow(string Content, string HeaderTitle = "Information", string ConfirmButtonText = "OK", string CancelButtonText = "Abbrechen", bool ShowCloseButton = true);
        public void ConfirmDialog(bool _confirmationResponse = false);
    }
}
