using System;
using MongoDB.Bson;

namespace EmailService.API.Models
{
    /// <summary>
    /// Store all emails sent for checcking in future
    /// </summary>
    public class EmailDatabaseModel
    {
        public ObjectId Id { get; set; }
        
        public EmailModel EmailModel { get; set; }
        
        public DateTime SentTimeUtc { get; set; } = DateTime.UtcNow;

        public EmailDatabaseModel(EmailModel emailModel)
        {
            Id = ObjectId.GenerateNewId();
            EmailModel = emailModel;
        }
    }
}