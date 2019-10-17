﻿using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mystique.Core.BusinessLogics;
using Mystique.Core.Contracts;
using Mystique.Core.DomainModel;
using Mystique.Core.Helpers;
using Mystique.Core.Repositories;
using Mystique.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Mystique.Core.Mvc.Infrastructure
{
    public static class MystiqueStartup
    {
        private static readonly IList<string> presets = new List<string>();

        public static async Task MystiqueSetupAsync(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.AddDbContext<PluginDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("PluginsConnectionString");
                // options.UseSqlServer(connectionString);
                options.UseSqlite(connectionString);
                options.EnableSensitiveDataLogging(true);
                options.EnableDetailedErrors(true);
            });

            services.AddSingleton<IMvcModuleSetup, MvcModuleSetup>();
            services.AddScoped<IPluginManager, PluginManager>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPluginRepository, PluginRepository>();
            services.AddScoped<PluginPackage>();
            services.AddSingleton<IActionDescriptorChangeProvider>(MystiqueActionDescriptorChangeProvider.Instance);
            services.AddSingleton(MystiqueActionDescriptorChangeProvider.Instance);

            var mvcBuilder = services.AddMvc().AddRazorRuntimeCompilation(o =>
            {
                foreach (var item in presets)
                {
                    o.AdditionalReferencePaths.Add(item);
                }
                AdditionalReferencePathHolder.AdditionalReferencePaths = o.AdditionalReferencePaths;
            });

            var provider = services.BuildServiceProvider();
            using (var scope = provider.CreateScope())
            {
                var option = scope.ServiceProvider.GetService<MvcRazorRuntimeCompilationOptions>();

                var unitOfWork = scope.ServiceProvider.GetService<IUnitOfWork>();
                var pluginRepository = scope.ServiceProvider.GetService<IPluginRepository>();
                var allEnabledPlugins = await pluginRepository.GetAllEnabledPluginsAsync();

                foreach (var plugin in allEnabledPlugins)
                {
                    var context = new CollectibleAssemblyLoadContext();
                    var moduleName = plugin.Name;
                    var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules", moduleName, $"{moduleName}.dll");
                    var referenceFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules", moduleName);

                    using var fs = new FileStream(filePath, FileMode.Open);
                    var assembly = context.LoadFromStream(fs);
                    presets.Add(filePath);

                    DefaultReferenceLoader loader = new DefaultReferenceLoader(referenceFolderPath, $"{moduleName}.dll");
                    loader.LoadStreamsIntoContext(context);

                    var controllerAssemblyPart = new MystiqueAssemblyPart(assembly);
                    mvcBuilder.PartManager.ApplicationParts.Add(controllerAssemblyPart);
                    PluginsLoadContexts.AddPluginContext(plugin.Name, context);
                }
            }

            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.AreaViewLocationFormats.Add("/Modules/{2}/Views/{1}/{0}" + RazorViewEngine.ViewExtension);
                o.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
            });
        }
    }
}
