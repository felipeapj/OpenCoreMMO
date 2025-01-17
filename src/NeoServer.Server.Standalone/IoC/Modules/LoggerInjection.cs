﻿using Autofac;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace NeoServer.Server.Standalone.IoC.Modules;

public static class LoggerInjection
{
    public static ContainerBuilder AddLogger(this ContainerBuilder builder, IConfigurationRoot configuration)
    {
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration, "Log")
            .WriteTo.Console(theme: AnsiConsoleTheme.Code);

        var logger = loggerConfig.CreateLogger();

        builder.RegisterInstance(logger).As<ILogger>().SingleInstance();
        builder.RegisterInstance(loggerConfig).SingleInstance();
        return builder;
    }
}