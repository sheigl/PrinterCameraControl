using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client.Receiving;
using Newtonsoft.Json;

namespace PrinterCameraControl;
class MqttHandler : IMqttApplicationMessageReceivedHandler
{
    private readonly ILogger<MqttHandler> _logger;
    private readonly IConfiguration _configuration;

    public MqttHandler(ILogger<MqttHandler> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
    {
        var payload = Encoding.UTF8.GetString(eventArgs.ApplicationMessage?.Payload ?? new byte[] { });

        if (!string.IsNullOrEmpty(payload))
        {
            if (!string.IsNullOrEmpty(_configuration["verbose"]))
                _logger.LogInformation(payload);

            var moonrakerEvent = JsonConvert.DeserializeObject<MoonrakerEvent>(payload)!;

            if (moonrakerEvent.Status?.PrintStats?.State != null)
            {
                switch (moonrakerEvent.Status?.PrintStats?.State)
                {
                    case "printing":
                        _logger.LogInformation("Starting crowsnest");
                        ControlLinuxService("crowsnest", true);
                        break;
                    default:
                        _logger.LogInformation("Stopping crowsnest");
                        ControlLinuxService("crowsnest", false);
                        break;
                }
            }            
        }

        return Task.CompletedTask;
    }

    public void ControlLinuxService(string serviceName, bool start)
    {
        Task.Run(() => 
        {
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "/bin/bash", Arguments = $"-c systemctl {(start ? "start" : "stop")} {serviceName}", };
            Process proc = new Process() { StartInfo = startInfo, };
            proc.Start();
            proc.WaitForExit();
        });
    }
}