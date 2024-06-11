using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using backend.Data;
using backend.Hubs;
using backend.Models;
using backend.Settings;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;

public class MqttService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<GpsHub> _hubContext;
    private readonly MqttConfiguration _mqttConfig;
    private IMqttClient _client;

    public MqttService(
        IServiceScopeFactory scopeFactory,
        IHubContext<GpsHub> hubContext,
        IOptions<MqttConfiguration> mqttConfig
    )
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _mqttConfig = mqttConfig.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var options = new MqttClientOptionsBuilder()
            .WithClientId(_mqttConfig.Client)
            .WithTcpServer(_mqttConfig.Server, _mqttConfig.Port)
            .WithCredentials(_mqttConfig.Username, _mqttConfig.Password)
            .WithCleanSession()
            .Build();

        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();

        _client.ConnectedAsync += async e =>
        {
            Console.WriteLine("Connected to MQTT Broker.");
            await _client.SubscribeAsync(
                new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter("alextassy24/feeds/gps")
                    .WithTopicFilter("alextassy24/feeds/status")
                    .Build()
            );
        };

        _client.ApplicationMessageReceivedAsync += async e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            if (topic == "alextassy24/feeds/gps")
            {
                await HandleGpsMessage(message);
            }
            else if (topic == "alextassy24/feeds/status")
            {
                await HandleStatusMessage(message);
            }
        };

        await _client.ConnectAsync(options, cancellationToken);
    }

    private async Task HandleGpsMessage(string message)
    {
        try
        {
            var data = JsonSerializer.Deserialize<LocationData>(message);

            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

                var product = context.Products.FirstOrDefault(p =>
                    p.DeviceID == new Guid(data.Guid)
                );

                var location = new Location
                {
                    Latitude = data.Lat.ToString(),
                    Longitude = data.Lon.ToString(),
                    Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    Time = DateTime.UtcNow.ToString("HH:mm:ss"),
                    ProductID = new Guid(data.Guid),
                    Product = product,
                };

                context.Locations.Add(location);
                await context.SaveChangesAsync();

                var locationData = new
                {
                    Latitude = data.Lat.ToString(),
                    Longitude = data.Lon.ToString(),
                    Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    Time = DateTime.UtcNow.ToString("HH:mm:ss"),
                    Sattelites = data.Sattelites.ToString(),
                    ProductID = new Guid(data.Guid),
                };
                await _hubContext.Clients.All.SendAsync("ReceiveLocationUpdate", locationData);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error deserializing GPS message: " + ex.Message);
        }
    }

    private async Task HandleStatusMessage(string message)
    {
        try
        {
            Console.WriteLine("Received status message: " + message);
            await _hubContext.Clients.All.SendAsync("ReceiveStatusUpdate", message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error handling status message: " + ex.Message);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.DisconnectAsync(new MqttClientDisconnectOptions(), cancellationToken);
    }
}
