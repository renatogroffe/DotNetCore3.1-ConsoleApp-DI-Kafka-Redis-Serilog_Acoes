using Microsoft.Extensions.DependencyInjection;

namespace ProcessadorAcoes
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            Startup.ConfigureServices(services);
            services.BuildServiceProvider()
                .GetService<ConsoleApp>().Run();
        }
    }
}