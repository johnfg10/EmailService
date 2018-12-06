using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using EmailService.API.Infastructure;
using EmailService.API.Infastructure.EventBus;
using EmailService.API.Interfaces;
using EmailService.API.Models;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MongoDB.Driver;

namespace EmailService.API
{
    public class Startup
    {
        private readonly ILogger _logger;

        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Startup>();
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddUserSecrets<Startup>(true).AddEnvironmentVariables(
                cfg => { cfg.Prefix = "JAUTH_"; });
            Configuration = builder.Build();
            
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //check to make sure all required variables have been set and if not throw a exception
            if (!Configuration.GetSection("smtp").Exists())
            {
                _logger.LogCritical("SMTP has not been configured");
            }
            
            if (!Configuration.GetSection("mongodb").Exists())
            {
                _logger.LogCritical("mongodb has not been configured");
            }
            

            services.AddSingleton<IMongoClient>(it =>
            {
                var config = it.GetService<IConfiguration>();
                var clientSettings = config.GetSection("mongodb").GetValue<MongoClientSettings>("clientSettings", null);
                if (clientSettings != null)
                {
                    return new MongoClient(config.GetValue<MongoClientSettings>("mongodb"));
                }
                
                return new MongoClient(config.GetSection("mongodb").GetValue<string>("connectionString"));
                //return new MongoClient(config.GetValue<MongoClientSettings>("mongodb"));
            });
            
            services.AddScoped<IMongoDatabase>(it =>
            {
                var client = it.GetService<IMongoClient>();
                var config = it.GetService<IConfiguration>();
                
                return client.GetDatabase(config.GetSection("mongodb").GetValue<string>("dbname", "EmailService"));
            });

            services.AddScoped<IMongoCollection<EmailDatabaseModel>>(it =>
            {
                var mongoDb = it.GetService<IMongoDatabase>();
                var config = it.GetService<IConfiguration>();
                return mongoDb.GetCollection<EmailDatabaseModel>(config.GetSection("mongodb").GetValue<string>("emailCollectionName", "emails"));
            });
            `
            services.AddSingleton<IConfiguration>(it => Configuration);

            services.AddScoped<SmtpClient>(it =>
            {
                var config = it.GetService<IConfiguration>();
                var smtpHost = config.GetSection("smtp").GetValue<string>("host", null);
               
                return new SmtpClient
                {
                    Credentials = new NetworkCredential(
                        config.GetSection("smtp").GetSection("credentials").GetValue<string>("username"), 
                        config.GetSection("smtp").GetSection("credentials").GetValue<string>("password")),
                    EnableSsl = true,
                    Host = smtpHost,
                    Port =  config.GetSection("smtp").GetValue<int>("port", 25)
                };
            });
            
            services.AddScoped<IEmailSender, EmailSender>();

            if (Configuration.GetSection("rabbitmq").Exists())
            {
                services.AddSingleton<IBusControl>(it =>
                {
                    var config = it.GetService<IConfiguration>();
                    var bus = Bus.Factory.CreateUsingRabbitMq(configurator =>
                    {
                        var host = configurator.Host(new Uri(config.GetSection("rabbitmq").GetValue<string>("uri", "rabbitmq://localhost")), h =>
                        {
                            h.Username(config.GetSection("rabbitmq").GetSection("credentials").GetValue<string>("username", null));
                            h.Password(config.GetSection("rabbitmq").GetSection("credentials").GetValue<string>("password", null));
                        });
                        configurator.ReceiveEndpoint(host, "email_queue", ep =>
                        {
                            ep.Consumer(typeof(EmailModelConsumer), consumerFactory => new EmailModelConsumer(it.GetService<IEmailSender>()));
                        });
                    });
                    bus.Start();
                    return bus;
                });
            }
            else
            {
                _logger.LogWarning("RabbitMQ is not setup in this instance as such it will not be setup");
            }



            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            
            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
