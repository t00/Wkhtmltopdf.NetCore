﻿using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
    public static string RotativaPath { get; set; }

    /// <summary>
    /// Setup Rotativa library
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="wkhtmltopdfRelativePath">Optional. Relative path to the directory containing wkhtmltopdf. Default is "Rotativa". Download at https://wkhtmltopdf.org/downloads.html</param>
    public static IServiceCollection AddWkhtmltopdf(this IServiceCollection services, string wkhtmltopdfRelativePath = "Rotativa")
    {
        RotativaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, wkhtmltopdfRelativePath);

        if (!Directory.Exists(RotativaPath))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new Exception("Folder containing wkhtmltopdf.exe not found, searched for " + RotativaPath);
            }

            throw new Exception("Folder containing wkhtmltopdf not found, searched for " + RotativaPath);
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