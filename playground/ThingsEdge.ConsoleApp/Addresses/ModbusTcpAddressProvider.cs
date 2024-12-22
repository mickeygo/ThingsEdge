using ThingsEdge.Exchange.Addresses;
using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Utils;

namespace ThingsEdge.ConsoleApp.Addresses;

internal sealed class ModbusTcpAddressProvider : IAddressProvider
{
    public List<Channel> GetChannels()
    {
        var json = """
[
    {
        "Name": "TestChannel01",
        "Devices": [
            {
                "Name": "Device01",
                "Model": "ModbusTcp",
                "Host": "127.0.0.1",
                "Tags": [
                    { "Name": "Heartbeat", "Address": "s=1;x=3;1", "DataType": "Int", "ScanRate": 500, "Flag": "Heartbeat" },
                    { "Name": "Notice01", "Address": "s=1;x=3;3", "DataType": "Int", "ScanRate": 1000, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice02", "Address": "s=1;x=3;5", "DataType": "Int", "ScanRate": 10000, "Flag": "Notice", "PublishMode": "EveryScan" },
                    { "Name": "PLC_Archive_Sign", "Address": "s=1;x=3;10", "DataType": "Int", "ScanRate": 500, "Flag": "Trigger",
                        "NormalTags": [
                            { "Name": "Trigger01_1", "Address": "s=1;x=3;11", "DataType": "Int" },
                            { "Name": "Trigger01_2", "Address": "s=1;x=3;12", "DataType": "Int" },
                            { "Name": "Trigger01_3", "Address": "s=1;x=3;13", "DataType": "Int" },
                        ],
                    },
                    { "Name": "Notice03", "Address": "s=1;x=3;3", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice04", "Address": "s=1;x=3;4", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice05", "Address": "s=1;x=3;5", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice06", "Address": "s=1;x=3;6", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice07", "Address": "s=1;x=3;7", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice08", "Address": "s=1;x=3;8", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice09", "Address": "s=1;x=3;9", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice10", "Address": "s=1;x=3;10", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice11", "Address": "s=1;x=3;11", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice12", "Address": "s=1;x=3;12", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice13", "Address": "s=1;x=3;13", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice14", "Address": "s=1;x=3;14", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice15", "Address": "s=1;x=3;15", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice16", "Address": "s=1;x=3;16", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice17", "Address": "s=1;x=3;17", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice18", "Address": "s=1;x=3;18", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice19", "Address": "s=1;x=3;19", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice20", "Address": "s=1;x=3;20", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice21", "Address": "s=1;x=3;21", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice22", "Address": "s=1;x=3;22", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice23", "Address": "s=1;x=3;23", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice24", "Address": "s=1;x=3;24", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice25", "Address": "s=1;x=3;25", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice26", "Address": "s=1;x=3;26", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice27", "Address": "s=1;x=3;27", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice28", "Address": "s=1;x=3;28", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice29", "Address": "s=1;x=3;29", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice30", "Address": "s=1;x=3;30", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice31", "Address": "s=1;x=3;31", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice32", "Address": "s=1;x=3;32", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice33", "Address": "s=1;x=3;33", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice34", "Address": "s=1;x=3;34", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice35", "Address": "s=1;x=3;35", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice36", "Address": "s=1;x=3;36", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice37", "Address": "s=1;x=3;37", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice38", "Address": "s=1;x=3;38", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice39", "Address": "s=1;x=3;39", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice40", "Address": "s=1;x=3;40", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice41", "Address": "s=1;x=3;41", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice42", "Address": "s=1;x=3;42", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice43", "Address": "s=1;x=3;43", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice44", "Address": "s=1;x=3;44", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice45", "Address": "s=1;x=3;45", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice46", "Address": "s=1;x=3;46", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice47", "Address": "s=1;x=3;47", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice48", "Address": "s=1;x=3;48", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice49", "Address": "s=1;x=3;49", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice50", "Address": "s=1;x=3;50", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice51", "Address": "s=1;x=3;51", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice52", "Address": "s=1;x=3;52", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice53", "Address": "s=1;x=3;53", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice54", "Address": "s=1;x=3;54", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice55", "Address": "s=1;x=3;55", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice56", "Address": "s=1;x=3;56", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice57", "Address": "s=1;x=3;57", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice58", "Address": "s=1;x=3;58", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice59", "Address": "s=1;x=3;59", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice60", "Address": "s=1;x=3;60", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice61", "Address": "s=1;x=3;61", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice62", "Address": "s=1;x=3;62", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice63", "Address": "s=1;x=3;63", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice64", "Address": "s=1;x=3;64", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice65", "Address": "s=1;x=3;65", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice66", "Address": "s=1;x=3;66", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice67", "Address": "s=1;x=3;67", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice68", "Address": "s=1;x=3;68", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice69", "Address": "s=1;x=3;69", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice70", "Address": "s=1;x=3;70", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice71", "Address": "s=1;x=3;71", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice72", "Address": "s=1;x=3;72", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice73", "Address": "s=1;x=3;73", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice74", "Address": "s=1;x=3;74", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice75", "Address": "s=1;x=3;75", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice76", "Address": "s=1;x=3;76", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice77", "Address": "s=1;x=3;77", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice78", "Address": "s=1;x=3;78", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice79", "Address": "s=1;x=3;79", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice80", "Address": "s=1;x=3;80", "DataType": "Int", "ScanRate": 100, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                ],
            },
        ],
    },
]
""";

        return JsonUtils.Deserialize<List<Channel>>(json) ?? [];
    }
}
