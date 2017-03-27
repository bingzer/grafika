﻿using Grafika.Services.Emails;
using System;
using System.Collections.Generic;
using System.Text;
using Grafika.Services;
using Grafika.Services.Models;
using Grafika.Emails;
using System.Threading.Tasks;
using Xunit;
using Grafika.Configurations;
using Moq;
using Microsoft.Extensions.Options;
using MailKit;
using MimeKit;
using System.Threading;

namespace Grafika.Test.Services.Emails
{
    public class TemplatedEmailServiceTest
    {
        ContentConfiguration contentConfig = new ContentConfiguration
        {
            Url = "Url",
            PrivacyPath = "PrivacyPath",
        };

        EmailConfiguration emailConfig = new EmailConfiguration
        {
            Host = "host",
            Port = 22,
            DefaultFrom = "defaultFrom"
        };

        [Fact]
        public void TestCreateModel()
        {
            var mockEmail = MockHelpers.Configure(emailConfig);
            var mockContent = MockHelpers.Configure(contentConfig);

            var mockServiceProvider = MockHelpers.ServiceProvider;
            mockServiceProvider.Setup(c => c.GetService(It.Is<Type>(t => t == typeof(IOptions<EmailConfiguration>))))
                .Returns(mockEmail.Object)
                .Verifiable();
            mockServiceProvider.Setup(c => c.GetService(It.Is<Type>(t => t == typeof(IOptions<ContentConfiguration>))))
                .Returns(mockContent.Object)
                .Verifiable();

            var mockServiceContext = MockHelpers.ServiceContext;
            mockServiceContext.Setup(c => c.ServiceProvider)
                .Returns(mockServiceProvider.Object)
                .Verifiable();

            var emailService = new TestingTemplatedEmailService(mockServiceContext.Object);
            var model = emailService.CreateModelPublic<BaseEmailModel>("user@email.com", "Subject");

            Assert.Equal("Url", model.HomeUrl);
            Assert.Equal("Url/PrivacyPath", model.PrivacyUrl);
            Assert.Equal("user@email.com", model.Email);
            Assert.Equal("Subject", model.Subject);
            Assert.Equal(emailConfig.DefaultFrom, model.Sender);
        }

        [Fact]
        public void TestCreateEmailData()
        {
            var mockEmail = MockHelpers.Configure(emailConfig);
            var mockContent = MockHelpers.Configure(contentConfig);

            var mockServiceProvider = MockHelpers.ServiceProvider;
            mockServiceProvider.Setup(c => c.GetService(It.Is<Type>(t => t == typeof(IOptions<EmailConfiguration>))))
                .Returns(mockEmail.Object)
                .Verifiable();
            mockServiceProvider.Setup(c => c.GetService(It.Is<Type>(t => t == typeof(IOptions<ContentConfiguration>))))
                .Returns(mockContent.Object)
                .Verifiable();

            var mockServiceContext = MockHelpers.ServiceContext;
            mockServiceContext.Setup(c => c.ServiceProvider)
                .Returns(mockServiceProvider.Object)
                .Verifiable();

            var emailService = new TestingTemplatedEmailService(mockServiceContext.Object);
            var model = emailService.CreateEmailDataPublic(new BaseEmailModel());

            Assert.IsType<TemplatedEmailData<BaseEmailModel>>(model);
        }

        [Fact]
        public async void TestSendEmail()
        {
            var mockEmail = MockHelpers.Configure(emailConfig);
            var mockContent = MockHelpers.Configure(contentConfig);

            var mockTransport = new Mock<IMailTransport>();
            mockTransport.Setup(c => c.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
                .Returns(Task.FromResult(0))
                .Verifiable();

            var mockServiceProvider = MockHelpers.ServiceProvider;
            mockServiceProvider.Setup(c => c.GetService(It.Is<Type>(t => t == typeof(IOptions<EmailConfiguration>))))
                .Returns(mockEmail.Object);
            mockServiceProvider.Setup(c => c.GetService(It.Is<Type>(t => t == typeof(IOptions<ContentConfiguration>))))
                .Returns(mockContent.Object);
            mockServiceProvider.Setup(c => c.GetService(It.Is<Type>(t => t == typeof(ITemplatedRenderingEngine<string>))))
                .Returns(new Mock<ITemplatedRenderingEngine<string>>().Object);
            mockServiceProvider.Setup(c => c.GetService(It.Is<Type>(t => t == typeof(IMailTransport))))
                .Returns(mockTransport.Object);

            var mockServiceContext = MockHelpers.ServiceContext;
            mockServiceContext.Setup(c => c.ServiceProvider)
                .Returns(mockServiceProvider.Object);

            var emailService = new TestingTemplatedEmailService(mockServiceContext.Object);
            
            await emailService.SendEmailPublic(new BaseEmailModel { Email = "user@email.com", Sender = "sender@email.com", Subject = "Subject" });

            mockTransport.VerifyAll();
        }

        class TestingTemplatedEmailService : TemplatedEmailService
        {

            public TestingTemplatedEmailService(IServiceContext userContext) : base(userContext)
            {
            }

            // -- expose all protected

            public TEmailModel CreateModelPublic<TEmailModel>(string to, string subject)
                where TEmailModel : BaseEmailModel, new() => CreateModel<TEmailModel>(to, subject);
            public IEmailData CreateEmailDataPublic<TEmailModel>(TEmailModel model)
                where TEmailModel : BaseEmailModel, new() => CreateEmailData(model);
            public Task SendEmailPublic<TEmailModel>(TEmailModel model)
                where TEmailModel : BaseEmailModel, new() => SendEmail(model);
        }

    }
}
