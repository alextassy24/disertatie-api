#nullable disable
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

namespace backend.Hubs
{
    [EnableCors("AllowOrigin")]
    public class GpsHub : Hub
    {
        private readonly Random _random = new Random();
        private Task _gpsDataTask;

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("Client connected!");
            _gpsDataTask = SendGpsData();
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (_gpsDataTask != null && !_gpsDataTask.IsCompleted)
            {
                _gpsDataTask.Dispose();
            }
            Console.WriteLine("Client disconnected!");
            await base.OnDisconnectedAsync(exception);
        }
        public async Task SendGpsData()
        {
            while (true)
            {
                // Generate random GPS data
                double latitude = _random.NextDouble() * 180 - 90;
                double longitude = _random.NextDouble() * 360 - 180;

                // Create the location object
                var location = new
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Time = TimeOnly.FromDateTime(DateTime.Now),
                    Latitude = latitude,
                    Longitude = longitude
                };

                await Clients.Caller.SendAsync("ReceiveLocation", location);
                Console.WriteLine($"Message sent at {DateTime.Now}!");

                await Task.Delay(1000);
            }
        }
    }
}