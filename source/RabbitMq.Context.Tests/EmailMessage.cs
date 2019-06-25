using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using StoneAge.System.Utils.Email;

namespace RabbitMq.Context.Tests
{
    public class EmailMessage
    {
        public Guid Id { get; set; }
        public string To { get; set; }
        public string Bcc { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public List<Attachment> AttachmentFiles { get; private set; }

        public EmailMessage()
        {
            AttachmentFiles = new List<Attachment>();
            Id = Guid.NewGuid();
        }

        public void Add_Attachment(Attachment attachment)
        {
            if (attachment == null) return;

            AttachmentFiles.Add(attachment);
        }

        public IEnumerable<string> To_Email_Addresses()
        {
            if (To == null)
            {
                return new string[0];
            }

            return To.Replace(" ", "").Split(',');
        }

        public IEnumerable<string> Bcc_Email_Addresses()
        {
            if (Bcc == null)
            {
                return new string[0];
            }

            return Bcc.Replace(" ", "").Split(',');
        }

        public bool To_Emails_Are_Valid()
        {
            var addresses = To_Email_Addresses();
            return addresses.Any() && addresses.All(email => email.Is_Valid_Email());
        }

        public bool Bcc_Emails_Are_Valid()
        {
            var addresses = Bcc_Email_Addresses();

            return addresses.All(email => email.Is_Valid_Email());
        }
    }
}
