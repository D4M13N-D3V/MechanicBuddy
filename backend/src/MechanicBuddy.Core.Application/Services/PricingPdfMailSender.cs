using Carmasters.Core.Application.Configuration;
using Carmasters.Core.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NHibernate.Id;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static NHibernate.Engine.Query.CallableParser;

namespace Carmasters.Core.Application.Services
{

    public class PricingPdfMailSender : IPricingSender
    {
        private readonly ITenantConfigService tenantConfigService;
        private readonly ILogger<PricingPdfMailSender> logger;
        private readonly IPdfGenerator pdfGenerator;
        private readonly ISmtpClientFactory smtp;

        public PricingPdfMailSender(
            ILogger<PricingPdfMailSender> logger,
            IPdfGenerator pdfGenerator,
            ITenantConfigService tenantConfigService,
            ISmtpClientFactory smtpClientFactory)
        {
            this.tenantConfigService = tenantConfigService;
            this.logger = logger;
            this.pdfGenerator = pdfGenerator;
            smtp = smtpClientFactory;
        }

        public async Task Send(Pricing pricing)
        {
            // Get tenant-specific configurations
            var requisites = await tenantConfigService.GetRequisitesAsync();
            var pricingConfig = await tenantConfigService.GetPricingAsync();

            using (var mail = smtp.CreateClient())
            {
                if (string.IsNullOrWhiteSpace(pricing.Email))
                    throw new UserException("Cannot send an email, recipient email not provided.");

                var message = new MailMessage(
                    new MailAddress(requisites.Email, requisites.Name, Encoding.UTF8),
                    new MailAddress(pricing.Email)
                );

                var isInvoice = pricing is Invoice;
                message.Subject = pricing.GetDisplayName();
                message.BodyEncoding = message.SubjectEncoding = Encoding.UTF8;

                message.Body = isInvoice ?
                    pricingConfig.Invoice.EmailContent :
                    pricingConfig.Estimate.EmailContent;

                var pdfBytes = await pdfGenerator.Generate(pricing);
                message.Attachments.Add(new Attachment(new MemoryStream(pdfBytes), pricing.GetFileName(), "application/pdf"));

                mail.Send(message);
                logger.LogInformation("Pricing email sent {smtp}:{port} {subject}", mail.Host, mail.Port, message.Subject);
            }
        }
    }

}
