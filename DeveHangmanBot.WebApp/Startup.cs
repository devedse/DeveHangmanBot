using DeveCoolLib.Logging;
using DeveCoolLib.Logging.Appenders;
using DeveHangmanBot.Config;
using DeveHangmanBot.TelegramBot;
using DeveHangmanBot.WebApp.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace DeveHangmanBot.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            Task.Run(() =>
            {
                DirtyMemoryLogger.LoggingLines.Add($"{DateTime.Now}: Starting Telegram Bot");

                try
                {
                    StartTelegramBot().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    DirtyMemoryLogger.LoggingLines.Add($"{DateTime.Now}: Exception TelegramBot: {ex}");
                }

                DirtyMemoryLogger.LoggingLines.Add($"{DateTime.Now}: TelegramBot exited at");
            });

            DirtyMemoryLogger.LoggingLines.Add($"{DateTime.Now}: Configure completed!");
        }

        public async Task StartTelegramBot()
        {
            var config = BotConfigLoader.LoadFromEnvironmentVariables(Configuration);

            if (!config.IsValid)
            {
                config = BotConfigLoader.LoadFromStaticFile();
            }

            if (!config.IsValid)
            {
                DirtyMemoryLogger.LoggingLines.Add($"{DateTime.Now}: Bot configuration not valid");
            }

            var extraLogger = new DateTimeLoggerAppender(new DirtyMemoryLogger(LogLevel.Verbose), ": ");
            var telegramBot = new DeveHangmanTelegramBot(config, extraLogger);

            await telegramBot.Start();
        }
    }
}
