using System;
using Microsoft.Extensions.DependencyInjection;
using TechNova.RMS.ConsoleApp;

var services = new ServiceCollection();
CompositionRoot.ConfigureServices(services);

using var provider = services.BuildServiceProvider();
var app = provider.GetRequiredService<ConsoleMenu>();
await app.RunAsync();
