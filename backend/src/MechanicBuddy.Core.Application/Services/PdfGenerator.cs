using AutoMapper;
using Carmasters.Core.Application;
using Carmasters.Core.Application.Configuration;
using Carmasters.Core.Application.Model;
using Carmasters.Core.Application.Printing;
using Carmasters.Core.Domain;
using FluentNHibernate.Conventions.Inspections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using NHibernate.Criterion;

namespace Carmasters.Core.Application.Services
{
    public interface IPdfGenerator
    {
        Task<byte[]> Generate(Pricing pricing);
        IPricingHtmlGenerator GetBodyGenerator();
        IPricingHtmlGenerator GetFooterGenerator();
    }

    public interface IPricingHtmlGenerator
    {
        Task<string> Generate(Pricing pricing); 
    }

    public class PricingFooterHtmlGenerator : PricingHtmlBaseGenerator
    {
        public PricingFooterHtmlGenerator(
            ITemplateService templateService,
            ITenantConfigService tenantConfigService)
            : base(templateService, tenantConfigService)
        {
        }

        public override async Task<string> Generate(Pricing pricing)
        {
            var model = await CreatePricingModelAsync(pricing);
            var footerHtml = await templateService.RenderAsync("Print/Footer", model);
            return footerHtml;
        }
    }


    public class PricingBodyHtmlGenerator : PricingHtmlBaseGenerator
    {
        public PricingBodyHtmlGenerator(
            ITemplateService templateService,
            ITenantConfigService tenantConfigService)
            : base(templateService, tenantConfigService)
        {
        }

        public override async Task<string> Generate(Pricing pricing)
        {
            var model = await CreatePricingModelAsync(pricing);
            var html = await templateService.RenderAsync("Print/PricingOutput", model);
            return html;
        }
    }



    public abstract class PricingHtmlBaseGenerator : IPricingHtmlGenerator
    {
        protected readonly ITemplateService templateService;
        protected readonly ITenantConfigService tenantConfigService;

        public PricingHtmlBaseGenerator(
            ITemplateService templateService,
            ITenantConfigService tenantConfigService)
        {
            this.templateService = templateService;
            this.tenantConfigService = tenantConfigService;
        }

        protected async Task<PricingPrintModel> CreatePricingModelAsync(Pricing pricing)
        {
            var requisites = await tenantConfigService.GetRequisitesAsync();
            var pricingOptions = await tenantConfigService.GetPricingAsync();

            var model = new PricingPrintModel
            {
                Pricing = pricing,
                RequisitesOptions = requisites,
                PricingOptions = pricingOptions
            };

            return model;
        }

        public abstract Task<string> Generate(Pricing pricing);
    }
    //does not work in another assembly
    public class PdfGenerator : IPdfGenerator
    {
        private readonly IWebHostEnvironment env;
        private readonly IMapper mapper;
       
        private readonly IConfiguration configuration;
        private readonly PricingBodyHtmlGenerator bodyHtmlGenerator;
        private readonly PricingFooterHtmlGenerator footerHtmlGenerator;
        private readonly ILogger<PdfGenerator> logger;
        private readonly Uri serverUri;

        public PdfGenerator(IWebHostEnvironment env,IMapper mapper,  IConfiguration configuration,
             PricingBodyHtmlGenerator bodyHtmlGenerator,
             PricingFooterHtmlGenerator footerHtmlGenerator,
             IServer server,
             ILogger<PdfGenerator> logger)
        {
            this.env = env;
            this.mapper = mapper;
           
            this.configuration = configuration;
            this.bodyHtmlGenerator = bodyHtmlGenerator;
            this.footerHtmlGenerator = footerHtmlGenerator;
            this.logger = logger;
            var addressFeature = server.Features.Get<IServerAddressesFeature>();
            serverUri=  new Uri(addressFeature.Addresses.ToList().SingleOrDefault());
            logger.LogDebug("Pdf service reachable at : " + serverUri); 
        }

        IPricingHtmlGenerator IPdfGenerator.GetBodyGenerator()
        {
            return bodyHtmlGenerator;
        }

        IPricingHtmlGenerator IPdfGenerator.GetFooterGenerator()
        {
            return footerHtmlGenerator;
        }

        public async Task<byte[]> Generate(Pricing pricing ) 
        {  
            var stream = default(MemoryStream);
            var pdfLocalFile = new FileInfo(Path.Combine(configuration["PdfDirectory"], pricing.GetFileName()));
            stream = await Print(pricing);
            using (stream)
            {
                var pdfBytes = stream.ToArray();
                if (pdfLocalFile.Exists) pdfLocalFile.Delete();
                File.WriteAllBytes(pdfLocalFile.FullName, pdfBytes);
                return pdfBytes;
            }
        }

        private static string _executablePath; 
        private async Task PreparePuppeteerAsync( )
        {
            if (!string.IsNullOrWhiteSpace(_executablePath)) return;//TODO is it threadsafe?


            var downloadPath = configuration["PuppeteerPath"];
            var browserOptions = new BrowserFetcherOptions { 
                Path = downloadPath , 
            };
            var browserFetcher = new BrowserFetcher(browserOptions);

            var stableVersion = await browserFetcher.DownloadAsync(BrowserTag.Stable);

            _executablePath = browserFetcher.GetExecutablePath(stableVersion.BuildId);
            logger.LogDebug("Puppeteer downloaded browser : " + _executablePath);

        }

        private async Task<MemoryStream> Print(Pricing pricing )
        {
             
            var html = await bodyHtmlGenerator.Generate(pricing); 

            await PreparePuppeteerAsync();

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" },
                ExecutablePath = _executablePath
            });
             
            var page = await browser.NewPageAsync(); 

            await page.SetViewportAsync(new ViewPortOptions() { DeviceScaleFactor = 1, Width = 1440, Height = 2880, IsMobile = false, HasTouch = false });
            await page.SetContentAsync(html, options: new NavigationOptions() { WaitUntil = new [] { WaitUntilNavigation.Load  } });
            var tailWindCss = $"{serverUri.Scheme}://localhost:{serverUri.Port}/tailwind.css";
            var printCss = $"{serverUri.Scheme}://localhost:{serverUri.Port}/print.css";
            await page.AddStyleTagAsync(tailWindCss);
            await page.AddStyleTagAsync(printCss);
           
             
            var pdfContent = await page.PdfStreamAsync(new PdfOptions
            {
                PrintBackground = false,
                Format = PaperFormat.A4, 
                DisplayHeaderFooter = false  
            });
            return (MemoryStream)pdfContent;
        } 
    }
}
