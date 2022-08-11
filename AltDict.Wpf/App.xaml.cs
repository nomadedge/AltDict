using AltDict.Data.AutoMapper;
using AltDict.Data.DbContexts;
using AltDict.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace AltDict.Wpf
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddDbContext<AltDictDbContext>(options =>
            {
                options.UseSqlite("Data Source = AltDict.db");
            });

            services.AddScoped<IAltDictRepository, SqlAltDictRepository>();

            services.AddAutoMapper(typeof(EntityDtoProfile), typeof(DtoModelProfile));

            services.AddSingleton<MainWindow>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }
    }
}
