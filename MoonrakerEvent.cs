namespace PrinterCameraControl;

using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

public partial class MoonrakerEvent
{
    public double EventTime { get; set; }

    public Status? Status { get; set; }    
}

public partial class Status
{
    public TemperatureReading? HeaterBed { get; set; }
    public TemperatureReading? Extruder { get; set; }    
    public PrintStats? PrintStats { get; set; }
    public VirtualSdCard? VirtualSdCard { get; set; }
    public ToolHead? ToolHead { get; set; }
}

public partial class TemperatureReading
{    
    public double? Temperature { get; set; }
    public double? Target { get; set; }
    public double? Power { get; set; }
}

public class ToolHead 
{
    public double[]? Position { get; set; }
}

public class VirtualSdCard 
{
    public string? FilePath { get; set; }
    public double? Progress { get; set; }
    public bool? IsActive { get; set; }
    public long? FilePosition { get; set; }    
}

public enum PrintState
{
    Cancelled,
    Printing,
    Standby,
    Paused,
    Complete,
    Error
}

public class PrintStats
{
    public string? FileName { get; set; }
    public double? TotalDuration { get; set; }
    [JsonConverter(typeof(StringEnumConverter))]
    public PrintState? State { get; set; }
}

public partial class MoonrakerEvent
{
    public static MoonrakerEvent FromJson(string json) => JsonConvert.DeserializeObject<MoonrakerEvent>(json, Converter.Settings);
}

public static class Serialize
{
    public static string ToJson(this MoonrakerEvent self) => JsonConvert.SerializeObject(self, Converter.Settings);
}

internal static class Converter
{
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters =
        {
            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
        },
        Formatting = Formatting.Indented,
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        }
    };
}
