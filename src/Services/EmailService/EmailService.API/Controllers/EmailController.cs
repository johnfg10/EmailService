using System;
using System.Threading.Tasks;
using EmailService.API.Interfaces;
using EmailService.API.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace EmailService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailDashboardController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private readonly IMongoCollection<EmailDatabaseModel> _mongoCollection;

        public EmailDashboardController(IConfiguration configuration, IEmailSender emailSender,  IMongoCollection<EmailDatabaseModel> mongoCollection)
        {
            _configuration = configuration;
            _emailSender = emailSender;
            _mongoCollection = mongoCollection;
        }

        [HttpPost]
        public async Task<IActionResult> PostEmail([FromBody] EmailModel emailModel)
        {
            await _emailSender.SendMail(emailModel);
            return Accepted();
        }

        [HttpGet]
        public async Task<IActionResult> GetEmailSentFrom([FromQuery]DateTime dateTime)
        {
            return Ok(await _mongoCollection.FindAsync(it => it.SentTimeUtc >= dateTime));
        }
    }
}