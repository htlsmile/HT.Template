using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace HT.Template.BackEnd
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Console.Title = Config.AssemblyName;
                var host = CreateHostBuilder(args).Build();
                Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(host.Services.GetRequiredService<IConfiguration>()).CreateLogger();
                Log.Debug("���ݿ�����...");
                AppDbInitializer.Initialize(host.Services);
                Log.Debug("���ݿ������.");
                Log.Information($"{Config.AssemblyName} ����");
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"{Config.AssemblyName} ��ֹ");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseSerilog();
                webBuilder.UseUrls();
            })
            .ConfigureServices(services => services.AddHostedService<TaskManager>());
    }
}
