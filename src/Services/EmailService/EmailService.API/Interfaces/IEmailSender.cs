using System.Threading.Tasks;
using EmailService.API.Models;

namespace EmailService.API.Interfaces
{
    public interface IEmailSender
    {
        Task SendMail(EmailModel emailModel);
    }
}