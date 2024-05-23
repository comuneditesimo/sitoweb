using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class V_FORM_Application
    {
        [NotMapped]
        public bool ArchivedBool
        {
            get
            {
                if (Archived != null)
                    return true;


                return false;
            }
            set
            {
                if(value == true)
                {
                    Archived = DateTime.Now;
                }
                else
                {
                    Archived = null;
                }
            }
        }
        [NotMapped]
        public bool HasUnreadChatMessages { get; set; } = false;
        [NotMapped]
        public string PaymentStatusIcon
        {
            get
            {
                if (this.AllPayments <= 0)
                {
                    return "";
                }
                else if (this.OpenPayments <= 0)
                {
                    // Vollständig bezahlt
                    return "fa-regular fa-square-check";
                }
                else
                {
                    // Zu bezahlen
                    return "fa-regular fa-euro-sign";
                }
            }
        }
        [NotMapped]
        public string PaymentStatusText
        {
            get
            {
                if (this.AllPayments <= 0)
                {
                    return "";
                }
                else if (this.OpenPayments <= 0)
                {
                    // Vollständig bezahlt
                    // Pagato completamente
                    return "FORM_APPLICATION_PAYMENTSTATUS_COMPLETLY_PAYED";
                }
                else if (this.AllPayments == 1)
                {
                    // 1 Zahlung ausständig
                    // 1 pagamento aperto
                    // Nachdem Text geholt wurde {allPayments} mit dem Wert ersetzen 
                    return "FORM_APPLICATION_PAYMENTSTATUS_ONE_PAYMENT_TO_PAY";
                }
                else
                {
                    // 1 von 2 Zahlungen ausständig
                    // 2 di 3 pagamenti aperti
                    // Nachdem Text geholt wurde {allPayments} und {openPayments} mit dem Wert ersetzen 
                    return "FORM_APPLICATION_PAYMENTSTATUS_MORE_PAYMENTS_TO_PAY";
                }
            }
        }
    }
}
