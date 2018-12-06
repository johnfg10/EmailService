using System.Threading.Tasks;
using EmailService.API.Interfaces;
using EmailService.API.Models;
using MassTransit;

namespace EmailService.API.Infastructure.EventBus
{
    public class EmailModelConsumer : IConsumer<EmailModel>
    {
        private readonly IEmailSender _emailSender;
        
        public EmailModelConsumer(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }
        
        public Task Consume(ConsumeContext<EmailModel> context)
        {
            return _emailSender.SendMail(context.Message);
        }
    }
}