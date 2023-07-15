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
            //var configurationOptions = new ConfigurationOptions
            //{
            //    EndPoints = { "localhost:6379" },
            //    Password = "sua_senha_aqui", 
            //    AbortOnConnectFail = false, //Essa configuração é útil quando você deseja que o StackExchange.Redis continue tentando se conectar ao Redis, mesmo que ocorram falhas iniciais. 
            //};

            var redis = ConnectionMultiplexer.Connect("localhost:6379"); //configurationOptions
            services.AddScoped(s => redis.GetDatabase());
            services.AddSingleton(provider =>
            {
                return redis;
            });

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
