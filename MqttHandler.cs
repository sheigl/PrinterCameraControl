using System.Diagnostics;
using System.Text;
using MQTTnet;
using MQTTnet.Client.Receiving;
using Newtonsoft.Json;

namespace PrinterCameraControl;
class MqttHandler : IMqttApplicationMessageReceivedHandler
{
    private readonly ILogger<MqttHandler> _logger;

    public MqttHandler(ILogger<MqttHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
    {
        var payload = Encoding.UTF8.GetString(eventArgs.ApplicationMessage?.Payload ?? new byte[] { });

        if (!string.IsNullOrEmpty(payload))
        {
            var moonrakerEvent = JsonConvert.DeserializeObject<MoonrakerEvent>(payload)!;

            if (moonrakerEvent.Status?.PrintStats?.State != null)
            {
                switch (moonrakerEvent.Status?.PrintStats?.State)
                {
                    case PrintState.Printing:
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