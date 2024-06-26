using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace PrinterCameraControl;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMqttClientOptions _options;
    private readonly IMqttClient _mqttClient;

    public Worker(
        ILogger<Worker> logger,
        IMqttClientOptions options,
        IMqttClient mqttClient)
    {
        _logger = logger;
        this._options = options;
        _mqttClient = mqttClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connectionResult = await _mqttClient.ConnectAsync(_options, stoppingToken);
        stoppingToken.Register(async () => await _mqttClient.DisconnectAsync());
    }
}
