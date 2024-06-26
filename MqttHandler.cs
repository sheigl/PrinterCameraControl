using MQTTnet;
using MQTTnet.Client.Receiving;

namespace PrinterCameraControl;
class MqttHandler : IMqttApplicationMessageReceivedHandler
{
    public async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
    {
        
    }
}