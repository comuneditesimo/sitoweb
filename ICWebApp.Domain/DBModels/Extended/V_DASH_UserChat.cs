using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class V_DASH_UserChat
    {
        [NotMapped]
        public string ShortMessage
        {
            get
            {
                if (!string.IsNullOrEmpty(this.ChatMessage) && this.ChatMessage.Length > 50)
                {
                    return ChatMessage.Take(50) + "...";
                }
                else
                {
                    if (!string.IsNullOrEmpty(this.ChatMessage))
                    {
                        return ChatMessage;
                    }

                    return "";
                }
            }
        }
        [NotMapped]
        public string MessageCSS
        {
            get
            {
                if (ChatMessageUnread == true)
                    return "unread-chat";

                return "";
            }
        }
    }
}
