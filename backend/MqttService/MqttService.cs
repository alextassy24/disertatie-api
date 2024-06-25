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
                    .WithTopicFilter("alextassy24/feeds/battery")
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
            else if (topic == "alextassy24/feeds/battery")
            {
                await HandleBatteryMessage(message);
            }
        };

        await _client.ConnectAsync(options, cancellationToken);
    }

    private async Task HandleGpsMessage(string message)
    {
        // Console.WriteLine("GPS Message = ");
        // Console.WriteLine(message);
        try
        {
            var data = JsonSerializer.Deserialize<LocationData>(message);

            // Console.WriteLine("GPS Data = ");
            // Console.WriteLine(data);
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

                var product = context.Products.FirstOrDefault(p =>
                    p.DeviceID == new Guid(data.guid)
                );

                var location = new Location
                {
                    Latitude = data.lat.ToString(),
                    Longitude = data.lon.ToString(),
                    Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    Time = DateTime.UtcNow.ToString("HH:mm:ss"),
                    ProductID = new Guid(data.guid),
                    Product = product,
                };

                context.Locations.Add(location);
                await context.SaveChangesAsync();

                var locationData = new
                {
                    Latitude = data.lat.ToString(),
                    Longitude = data.lon.ToString(),
                    Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    Time = DateTime.UtcNow.ToString("HH:mm:ss"),
                    ProductID = new Guid(data.guid),
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
        // Console.WriteLine("Status Message = ");
        // Console.WriteLine(message);
        try
        {
            // Console.WriteLine("Received status message: " + message);
            var data = JsonSerializer.Deserialize<StatusMessage>(message);

            var statusMessage = new
            {
                guid = new Guid(data.guid),
                status_message = data.status_message
            };
            // Console.WriteLine(statusMessage);
            await _hubContext.Clients.All.SendAsync("ReceiveStatusUpdate", statusMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error handling status message: " + ex.Message);
        }
    }

    private async Task HandleBatteryMessage(string message)
    {
        // Console.WriteLine("Battery Message = ");
        // Console.WriteLine(message);
        try
        {
            var data = JsonSerializer.Deserialize<BatteryStatus>(message);

            var statusMessage = new
            {
                guid = new Guid(data.guid),
                battery_percentage = data.battery_percentage
            };
            // Console.WriteLine(statusMessage);
            await _hubContext.Clients.All.SendAsync("ReceiveBatteryUpdate", statusMessage);
            // Console.WriteLine("Battery status is sent");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error handling battery message: " + ex.Message);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.DisconnectAsync(new MqttClientDisconnectOptions(), cancellationToken);
    }
}
