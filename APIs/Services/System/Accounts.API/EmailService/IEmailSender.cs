using Accounts.API.Models;
using JwtAuthenticationManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.API.EmailService
{
    public interface IEmailSender
    {
        void SendEmail(MailData message);
        Task SendEmailAsync(MailData message);
    }
}
