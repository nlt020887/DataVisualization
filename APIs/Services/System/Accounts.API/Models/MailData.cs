using Microsoft.AspNetCore.Http;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.API.Models
{
    public class MailData
    {
        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public IFormFileCollection Attachments { get; set; }

        public MailData(IEnumerable<EmailAddress> to, string subject, string content, IFormFileCollection attachments)
        {
            To = new List<MailboxAddress>();            
            To.AddRange(to.Select(x => new MailboxAddress(x.DisplayName,x.Address)));
            Subject = subject;
            Content = content;
            Attachments = attachments;
        }
    
    }
    public class EmailAddress
    {
        public string Address { get; set; }
        public string DisplayName { get; set; }
    }
}