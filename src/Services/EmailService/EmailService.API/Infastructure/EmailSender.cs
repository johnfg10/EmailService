using System;
using System.Net.Mail;
using System.Threading.Tasks;
using EmailService.API.Interfaces;
using EmailService.API.Models;
using MongoDB.Driver;

namespace EmailService.API.Infastructure
{
    public class EmailSender : IEmailSender
    {
        private readonly IMongoCollection<EmailDatabaseModel> _emailCollection;
        private readonly SmtpClient _client;

        public EmailSender(IMongoCollection<EmailDatabaseModel> emailCollection, SmtpClient client)
        {
            _emailCollection = emailCollection;
            _client = client;
        }

        public Task SendMail(EmailModel emailModel)
        {
            var sendTask = SendEmail(emailModel);
            var mongoTask = _emailCollection.InsertOneAsync(new EmailDatabaseModel(emailModel));
            return Task.WhenAll(sendTask, mongoTask);
        }
        
        private Task SendEmail(EmailModel emailModel)
        {
            var mailMessage = new MailMessage();
            emailModel.SendTo.ForEach(it => mailMessage.To.Add(it));
            mailMessage.Subject = emailModel.Subject;
            mailMessage.From = new MailAddress(emailModel.SentFrom);
            mailMessage.Body = emailModel.Body + Environment.NewLine + EmailModel.SignOff;
            mailMessage.IsBodyHtml = emailModel.IsBodyHtml;
            return _client.SendMailAsync(mailMessage);
        }
    }
}