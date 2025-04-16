using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;

namespace Wkhtmltopdf.NetCore;

public static class WkhtmltopdfConfiguration
{
    public static string WkHtmlToPdfPath { get; set; }

    /// <summary>
    /// Setup Rotativa library
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="wkHtmlToPdfRelativePath">Optional. Relative path in the root</param>
    /// <param name="wkHtmlToPdfFileName">Optionsl. WkHtmlToPdf executable name</param>
    public static IServiceCollection AddWkhtmltopdf(this IServiceCollection services, string wkHtmlToPdfRelativePath = null, string wkHtmlToPdfFileName = null)
    {
        WkHtmlToPdfPath = string.IsNullOrWhiteSpace(wkHtmlToPdfFileName)
            ? RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "wkhtmltopdf.exe" : "wkhtmltopdf"
            : wkHtmlToPdfFileName;
        if (!string.IsNullOrWhiteSpace(wkHtmlToPdfRelativePath))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WkHtmlToPdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, wkHtmlToPdfRelativePath, "Windows", WkHtmlToPdfPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                WkHtmlToPdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, wkHtmlToPdfRelativePath, "Mac", WkHtmlToPdfPath);
            }
            else
            {
                WkHtmlToPdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, wkHtmlToPdfRelativePath, "Linux", WkHtmlToPdfPath);
            }
        }

        var updateableFileProvider = new UpdateableFileProvider();
        var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");
        services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        services.TryAddSingleton<DiagnosticSource>(diagnosticSource);
        services.TryAddTransient<ITempDataProvider, SessionStateTempDataProvider>();
        services.TryAddTransient<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
        services.TryAddTransient<IGeneratePdf, GeneratePdf>();
        services.TryAddSingleton(updateableFileProvider);

        services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
        {
            options.FileProviders.Add(updateableFileProvider);
        });

        return services;
    }
}