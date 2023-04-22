using SignalR.Example.WebSockets;
using StackExchange.Redis;

namespace SignalR.Example
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) =>
            Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            var redis = ConnectionMultiplexer.Connect("localhost");
            services.AddScoped(s => redis.GetDatabase());

            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddSignalR();
            services.AddSingleton<WebSocket>();
        }

        public static void Configure(WebApplication app, IWebHostEnvironment env)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.MapHub<WebSocket>("/connection");

            app.Run();
        }
    }
}
