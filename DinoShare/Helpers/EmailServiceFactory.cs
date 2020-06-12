using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using DinoShare.Models;
using DinoShare.TemplateParser;
using DinoShare.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DinoShare.Helpers.EmailServiceFactory
{
    public interface IEmailService
    {
        Task SendEmailAsync(List<string> toEmailAddresses, String subject,
            PublicEnums.EmailTemplateList emailTemplate, Dictionary<string, PropertyMetaData> variableValues,
            ClaimsPrincipal user, List<string> ccEmailAddresses = null, List<EmailAttachment> attachments = null);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;
        private readonly AppDBContext _context;

        public EmailService(AppDBContext context, IOptions<EmailOptions> emailOptions)
        {
            _context = context;
            this._emailOptions = emailOptions.Value;
        }

        public async Task SendEmailAsync(List<string> toEmailAddresses, String subject,
            PublicEnums.EmailTemplateList emailTemplate, Dictionary<string, PropertyMetaData> variableValues,
            ClaimsPrincipal user, List<string> ccEmailAddresses = null, List<EmailAttachment> attachments = null)
        {
            try
            {
                if (_emailOptions.EmailEnabled)
                {
                    var emailMessage = new MimeMessage();

                    emailMessage.From.Add(new MailboxAddress(_emailOptions.FromName, _emailOptions.FromAddress));

                    foreach (var toEmailAddress in toEmailAddresses)
                    {
                        emailMessage.To.Add(new MailboxAddress("", toEmailAddress));
                    }
                    if (ccEmailAddresses != null)
                    {
                        foreach (var ccEmailAddress in ccEmailAddresses)
                        {
                            emailMessage.Cc.Add(new MailboxAddress("", ccEmailAddress));
                        }
                    }

                    //Process email template
                    string htmlMessage = await ProcessEmailTemplate(emailTemplate, variableValues);

                    emailMessage.Subject = subject;

                    //Build body
                    var builder = new BodyBuilder();
                    builder.HtmlBody = htmlMessage;

                    //Add attachments
                    if (attachments != null && attachments.Count > 0)
                    {
                        foreach (var item in attachments)
                        {
                            builder.Attachments.Add(item.AttachmentName, item.AttachmentData, ContentType.Parse(item.ContentType));
                        }
                    }

                    emailMessage.Body = builder.ToMessageBody();

                    using (var client = new SmtpClient())
                    {
                        client.LocalDomain = _emailOptions.LocalDomain;

                        await client.ConnectAsync(_emailOptions.MailServerAddress, Convert.ToInt32(_emailOptions.MailServerPort), SecureSocketOptions.Auto).ConfigureAwait(false);
                        if (_emailOptions.RequireLogin)
                        {
                            await client.AuthenticateAsync(new NetworkCredential(_emailOptions.Username, _emailOptions.UserPassword));
                        }
                        await client.SendAsync(emailMessage).ConfigureAwait(false);
                        await client.DisconnectAsync(true).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(_context, PublicEnums.LogLevel.LEVEL_EXCEPTION, "Helpers.EmailServiceFactory.EmailService.SendEmailAsync", ex.Message, user, ex);
            }
        }

        private async Task<string> ProcessEmailTemplate(PublicEnums.EmailTemplateList emailTemplate, Dictionary<string, PropertyMetaData> variableValues)
        {
            //Get email template
            string templateText = _context.EmailTemplates.Where(x => x.EventCode == emailTemplate.ToString()).First().TemplateBody;

            return TemplateParser.TemplateParser.Render(templateText, variableValues, Placeholder.Bracket);
        }
    }

    public class EmailAttachment
    {
        public string AttachmentName { get; set; }
        public byte[] AttachmentData { get; set; }
        public string ContentType { get; set; }
    }
}
