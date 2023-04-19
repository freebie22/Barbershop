using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;
using Newtonsoft.Json.Linq;

namespace Barbershop.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(email, subject, htmlMessage);
        }

        public async Task Execute(string email, string subject, string body)
        {
            MailjetClient client = new MailjetClient("63308da312553ed4a633075861e03f70", "618c9b4f3f53d238bb8b1f42b70e3020")
            {
                Version = ApiVersion.V3_1,
            };
            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
             .Property(Send.Messages, new JArray {
     new JObject {
      {
       "From",
       new JObject {
        {"Email", "barbershop.oasis@ukr.net"},
        {"Name", "Barbershop Oasis"}
       }
      }, {
       "To",
       new JArray {
        new JObject {
         {
          "Email",
          email
         }, {
          "Name",
          "Barbershop Oasis"
         }
        }
       }
      }, {
       "Subject",
       subject
      }, 
         {
       "HTMLPart",
       body
      }, 
     }
             });
             await client.PostAsync(request);
        }
    }
}
