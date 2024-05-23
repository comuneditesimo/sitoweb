using ICWebApp.Application.Interface.Services;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Services
{
    public class DialogService : IDialogService
    {
        // Properties
        public bool ShowDialogWindow
        {
            get
            {
                return _showDialogWindow;
            }
            set
            {
                if (value == false)
                {
                    ConfirmDialog();
                }
            }
        }
        public string HeaderTitle
        {
            get
            {
                return _headerTitle;
            }
        }
        public string Content
        {
            get
            {
                return _content;
            }
        }
        public bool ShowCloseButton
        {
            get
            {
                return _showCloseButton;
            }
        }
        public string Confirm_Button_Text
        {
            get
            {
                return _confirm_Button_Text;
            }
        }
        public string Cancel_Button_Text
        {
            get
            {
                return _cancel_Button_Text;
            }
        }
        // Variablen
        private bool _showDialogWindow { get; set; } = false;
        private string _headerTitle { get; set; } = String.Empty;
        private string _content { get; set; } = String.Empty;
        private string _confirm_Button_Text { get; set; } = "OK";
        public string _cancel_Button_Text { get; set; } = "Abbrechen";
        private bool _showCloseButton { get; set; } = true;
        private CancellationTokenSource _dialogConfirmed = new CancellationTokenSource();
        private bool _dialogResponse = false;
        private bool _dialogAlreadyResponsed = false;

        public event Action<bool>? OnStateChange;
        // Methoden
        public async Task<bool> OpenDialogWindow(string Content, string HeaderTitle = "Information", string ConfirmButtonText = "OK", string CancelButtonText = "Abbrechen", bool ShowCloseButton = true)
        {
            _dialogConfirmed = new CancellationTokenSource();
            _dialogAlreadyResponsed = false;
            _showDialogWindow = true;
            _headerTitle = HeaderTitle;
            _content = Content;
            _confirm_Button_Text = ConfirmButtonText;
            _cancel_Button_Text = CancelButtonText;
            _showCloseButton = ShowCloseButton;
            _dialogResponse = false;
            OnDialogWindowStateChanged();

            try
            {
                await Task.Delay(-1, _dialogConfirmed.Token);
            }
            catch
            {
            }

            OnDialogWindowStateChanged();

            return _dialogResponse;
        }
        public void ConfirmDialog(bool _confirmationResponse = false)
        {
            if (_dialogConfirmed.Token.CanBeCanceled && !_dialogAlreadyResponsed)
            {
                _dialogAlreadyResponsed = true;
                _showDialogWindow = false;
                _dialogResponse = _confirmationResponse;
                _dialogConfirmed.Cancel();
            }
        } 
        private void OnDialogWindowStateChanged()
        {
            if (OnStateChange != null)
            {
                OnStateChange.Invoke(ShowDialogWindow);
            }
        }
    }
}
