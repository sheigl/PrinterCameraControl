using PrinterCameraControl;
using Microsoft.Extensions.Hosting.Systemd;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Client;
using MQTTnet.Client.Receiving;
using Microsoft.Extensions.Options;

var builder = Host.CreateDefaultBuilder(args);
builder
    .ConfigureHostConfiguration(builder => builder
        .AddCommandLine(args)
        .AddEnvironmentVariables()
        .AddJsonFile("appsettings.json", false)
        .AddJsonFile("appsettings.secrets.json", false))
    .ConfigureServices(services =>
    {
        services.AddSystemd()
            .AddHostedService<Worker>()
            .AddOptions<MqttSettings>()
            .Configure<IConfiguration>((settings, config) => config.GetSection("MqttSettings").Bind(settings));

        services.AddSingleton(provider =>
            {
                var settings = provider.GetRequiredService<IOptionsMonitor<MqttSettings>>();
                // Create TCP based options using the builder.
                var options = new MqttClientOptionsBuilder()
                    .WithClientId($"PrinterCameraConrol-{Guid.NewGuid()}")
                    .WithTcpServer(settings.CurrentValue.Host)
                    .WithCredentials("mosquitto", "86LSwHF0JSAf")
                    .WithCleanSession()
                    .Build();

                return options;
            })
            .AddSingleton<IMqttApplicationMessageReceivedHandler, MqttHandler>()
            .AddSingleton(provider =>
            {
                var factory = new MqttFactory();
                var mqttClient = factory.CreateMqttClient();
                var settings = provider.GetRequiredService<IOptionsMonitor<MqttSettings>>();
                var options = provider.GetRequiredService<IMqttClientOptions>();
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
                    await mqttClient.SubscribeAsync(settings.CurrentValue.Topic);

                    Console.WriteLine($"### SUBSCRIBED TO {settings.CurrentValue.Topic} ###");
                });

                mqttClient.UseApplicationMessageReceivedHandler(handler);

                return mqttClient;
            });

    });


var host = builder.Build();

var config = host.Services.GetRequiredService<IConfiguration>();

await host.RunAsync();
