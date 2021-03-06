﻿using Grafika.Configurations;
using Grafika.Emails;
using Grafika.Services.Models;
using Grafika.Utilities;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Grafika.Services.Emails
{
    class TemplatedEmailService : EmailService, ITemplatedEmailService
    {
        public ServerConfiguration ServerConfig { get; private set; }
        public ITemplatedRenderingEngine<string> Engine { get; private set; }

        public TemplatedEmailService(IServiceContext userContext) 
            : base(userContext)
        {
            ServerConfig = Context.ServiceProvider.Get<IOptions<ServerConfiguration>>().Value;
            Engine = Context.ServiceProvider.Get<ITemplatedRenderingEngine<string>>();
        }

        public TEmailModel CreateModel<TEmailModel>(string to, string subject)
            where TEmailModel : BaseEmailModel, new()
        {
            return new TEmailModel
            {
                Email = to,
                HomeUrl = ServerConfig.Url,
                PrivacyUrl = Utility.CombineUrl(ServerConfig.Url, ServerConfig.PrivacyPath),
                Sender = EmailConfig.DefaultFrom,
                Subject = subject
            };
        }

        public virtual Task SendEmail<TEmailModel>(TEmailModel model)
            where TEmailModel : BaseEmailModel, new()
        {
            var emailData = CreateEmailData(model);
            return base.SendEmail(emailData);
        }

        public IEmailData CreateEmailData<TEmailModel>(TEmailModel model)
            where TEmailModel : BaseEmailModel, new()
        {
            var templateName = typeof(TEmailModel).Name;
            return new TemplatedEmailData<TEmailModel>(Engine, templateName, model)
            {
                To = model.Email,
                Subject = model.Subject
            };
        }
    }
}
