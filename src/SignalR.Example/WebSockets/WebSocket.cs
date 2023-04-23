using Microsoft.AspNetCore.SignalR;
using SignalR.Example.Model;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;

namespace SignalR.Example.WebSockets
{
    public class WebSocket : Hub
    {
        private readonly ConnectionMultiplexer _redisConnection;
        private readonly ISubscriber _redisSubscriber;
        private readonly IServiceProvider _serviceProvider;
        private readonly  string _redisChannelName = "channel";
        private ILogger? _logger;

        public WebSocket(IServiceProvider serviceProvider, ConnectionMultiplexer redisConnection)
        {
            _serviceProvider = serviceProvider;
            _redisConnection = redisConnection;
            _redisSubscriber = _redisConnection.GetSubscriber();

            _redisSubscriber.Subscribe(_redisChannelName, async (channel, message) =>
            {
                await RedisReceivedMessage(message);
            });
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var id = Context.ConnectionId;

                await Clients.Client(Context.ConnectionId).SendAsync("Connected", id);

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                using var scope = _serviceProvider.CreateScope();
                _logger= scope.ServiceProvider.GetRequiredService<ILogger<WebSocket>>();
                _logger.LogError(ex, ex.Message);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception == null)
            {
                await base.OnDisconnectedAsync(exception);
            }
            else
            {
                Console.WriteLine($"{exception}, {exception.Message}");
                Debug.WriteLine($"{exception}, {exception.Message}");

                await base.OnDisconnectedAsync(exception);
            }
        }

        public async Task PublishMessageRedis(Person person)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            await _redisConnection.GetDatabase().PublishAsync(_redisChannelName, JsonSerializer.Serialize(person, options));
        }

        public async Task SendMessageToClient(string userId, string uniqueMessage)
        {
            try
            {
                await Clients.Client(userId).SendAsync("ReceiveMessage", Context.ConnectionId, uniqueMessage);
            }
            catch (Exception ex)
            {
                using var scope = _serviceProvider.CreateScope();
                _logger= scope.ServiceProvider.GetRequiredService<ILogger<WebSocket>>();
                _logger.LogError(ex, ex.Message);
            }
        }

        public async Task SendMessage(string username, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", username, message);
        }

        public async Task RedisReceivedMessage(RedisValue message)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                if (!message.IsNullOrEmpty)
                {
                    var content = JsonSerializer.Deserialize<Person>(message, options);
                    await Clients.All.SendAsync("ReceiveObject", content).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                using var scope = _serviceProvider.CreateScope();
                _logger= scope.ServiceProvider.GetRequiredService<ILogger<WebSocket>>();
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}