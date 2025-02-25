using System.Management;
using System.Runtime.Versioning;

namespace WpfApp.Processors;

[SupportedOSPlatform("windows")]
public static class OsInfo
{
    public static bool IsVirtualMachine { get; }
    public static bool IsDomainJoined { get; }
    public static string Version { get; }
    
    static OsInfo()
    {
        using (var searcher = new ManagementObjectSearcher("Select Manufacturer,Model,PartOfDomain from Win32_ComputerSystem"))
        using (var items = searcher.Get())
        {
            IsVirtualMachine = items.Cast<ManagementBaseObject>()
                .Any(item =>
                {
                    var manufacturer = ((string?)item["Manufacturer"])?.ToLowerInvariant() ?? string.Empty;
                    var model = ((string?)item["Model"])?.ToLowerInvariant() ?? string.Empty;
                    
                    return (manufacturer == "microsoft corporation" && model.Contains("virtual"))
                           || manufacturer.Contains("vmware")
                           || model == "virtualbox";
                });
        
            IsDomainJoined = items.Cast<ManagementBaseObject>()
                .Any(item => item["PartOfDomain"] is true);
        }

        using (var searcher = new ManagementObjectSearcher("select Caption,Version from Win32_OperatingSystem"))
        using (var items = searcher.Get())
        {
            Version = items.Cast<ManagementBaseObject>()
                .Select(obj =>
                {
                    var caption = (string)obj["Caption"];
                    caption = caption.Replace("Microsoft Windows ", string.Empty);
                    var version = (string)obj[nameof(Version)];
                    // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                    var platform = Environment.OSVersion.Platform switch
                    {
                        PlatformID.Win32S => "Win32S",
                        PlatformID.Win32NT => "Windows NT",
                        PlatformID.WinCE => "Windows CE",
                        _ => throw new ArgumentException($"Unknown platform {Environment.OSVersion.Platform}")
                    };
                    return $"Microsoft {platform} {caption} {version}";
                })
                .First();
        }
    }
}