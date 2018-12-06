using System.Collections.Generic;

namespace EmailService.API.Models
{
    public class EmailModel
    {
        public List<string> SendTo { get; set; }
        
        public string SentFrom { get; set; }
        
        public string Subject { get; set; }
        
        public string Body { get; set; }

        public bool IsBodyHtml { get; set; } = true;
        
        public static string SignOff { get; set; }
    }
}