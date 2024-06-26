using PrinterCameraControl;
using Microsoft.Extensions.Hosting.Systemd;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Client;
using MQTTnet.Client.Receiving;

var builder = Host.CreateApplicationBuilder(args);
builder
    .Services
    .AddSystemd()
    .AddHostedService<Worker>()
    .AddSingleton(_ =>
    {
        // Create TCP based options using the builder.
        var options = new MqttClientOptionsBuilder()
            .WithClientId($"PrinterCameraConrol-{Guid.NewGuid()}")
            .WithTcpServer("192.168.4.202")
            .WithCredentials("mosquitto", "")
            .WithCleanSession()
            .Build();

        return options;
    })
    .AddSingleton<IMqttApplicationMessageReceivedHandler, MqttHandler>()
    .AddSingleton(provider =>
    {
        var factory = new MqttFactory();
        var mqttClient = factory.CreateMqttClient();
        var options = provider.GetRequiredService<MqttClientOptions>();
        var handler = provider.GetRequiredService<IMqttApplicationMessageReceivedHandler>();
        mqttClient.UseDisconnectedHandler(async e =>
        {
            Console.WriteLine("### DISCONNECTED FROM SERVER ###");
            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await mqttClient.ConnectAsync(options, CancellationToken.None); // Since 3.0.5 with CancellationToken
            }
            catch
            {
                Console.WriteLine("### RECONNECTING FAILED ###");
            }
        });

        mqttClient.UseConnectedHandler(async e =>
        {
            Console.WriteLine("### CONNECTED WITH SERVER ###");

            // Subscribe to a topic
            await mqttClient.SubscribeAsync("");

            Console.WriteLine("### SUBSCRIBED ###");
        });

        mqttClient.UseApplicationMessageReceivedHandler(handler);

        return mqttClient;
    });

var host = builder.Build();
await host.RunAsync();
