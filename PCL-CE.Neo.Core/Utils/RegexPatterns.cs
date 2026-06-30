using System;
using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils;

public static class RegexPatterns
{
    public static readonly Regex Email = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    public static readonly Regex Url = new(@"^https?:\/\/[^\s$.?#].[^\s]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    public static readonly Regex IpAddress = new(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", RegexOptions.Compiled);
    
    public static readonly Regex IpAddressV6 = new(@"^([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$|^::$|^([0-9a-fA-F]{1,4}:){1,7}:$|^([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    public static readonly Regex Domain = new(@"^([a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$", RegexOptions.Compiled);
    
    public static readonly Regex FileName = new(@"^[^\\/:*?""<>|\r\n]+$", RegexOptions.Compiled);
    
    public static readonly Regex FolderName = new(@"^[^\\/:*?""<>|\r\n]+$", RegexOptions.Compiled);
    
    public static readonly Regex WindowsPath = new(@"^(?:[a-zA-Z]:)?(?:\\[^\\/:*?""<>|\r\n]+)*\\?$", RegexOptions.Compiled);
    
    public static readonly Regex UnixPath = new(@"^\/(?:[^\\/:*?""<>|\r\n]+\/)*[^\\/:*?""<>|\r\n]*$", RegexOptions.Compiled);
    
    public static readonly Regex SemVer = new(@"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$", RegexOptions.Compiled);
    
    public static readonly Regex MinecraftVersion = new(@"^(?:(?:[1-9]\d*)\.(?:0|[1-9]\d*)(?:\.(?:0|[1-9]\d*))?(?:-(?:alpha|beta|pre|rc)(?:\.[0-9]+)?)?)|(?:[0-9a-f]{7})$", RegexOptions.Compiled);
    
    public static readonly Regex JavaVersion = new(@"^(?:(?:1\.)?(?:[0-9]+))(?:\.[0-9]+)*(_[0-9]+)?$", RegexOptions.Compiled);
    
    public static readonly Regex BroadcastMotd = new(@"\[MOTD\](.*?)\[/MOTD\]", RegexOptions.Compiled);
    
    public static readonly Regex BroadcastAd = new(@"\[AD\](.*?)\[/AD\]", RegexOptions.Compiled);
    
    public static readonly Regex HexColor = new(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", RegexOptions.Compiled);
    
    public static readonly Regex Guid = new(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    public static readonly Regex PhoneNumber = new(@"^\+?[1-9]\d{1,14}$", RegexOptions.Compiled);
    
    public static readonly Regex DateIso8601 = new(@"^\d{4}-(?:0[1-9]|1[0-2])-(?:0[1-9]|[12]\d|3[01])$", RegexOptions.Compiled);
    
    public static readonly Regex DateTimeIso8601 = new(@"^\d{4}-(?:0[1-9]|1[0-2])-(?:0[1-9]|[12]\d|3[01])T(?:[01]\d|2[0-3]):[0-5]\d(?::[0-5]\d(?:\.\d+)?)?(?:Z|[+-](?:0[1-9]|1[0-2]):[0-5]\d)?$", RegexOptions.Compiled);
    
    public static readonly Regex IPv4WithPort = new(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?):(?:[1-9]\d{0,4}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5])$", RegexOptions.Compiled);
    
    public static readonly Regex DomainWithPort = new(@"^([a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}:(?:[1-9]\d{0,4}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5])$", RegexOptions.Compiled);
    
    public static bool IsEmail(string input) => !string.IsNullOrWhiteSpace(input) && Email.IsMatch(input);
    
    public static bool IsUrl(string input) => !string.IsNullOrWhiteSpace(input) && Url.IsMatch(input);
    
    public static bool IsIpAddress(string input) => !string.IsNullOrWhiteSpace(input) && IpAddress.IsMatch(input);
    
    public static bool IsIpAddressV6(string input) => !string.IsNullOrWhiteSpace(input) && IpAddressV6.IsMatch(input);
    
    public static bool IsDomain(string input) => !string.IsNullOrWhiteSpace(input) && Domain.IsMatch(input);
    
    public static bool IsFileName(string input) => !string.IsNullOrWhiteSpace(input) && FileName.IsMatch(input);
    
    public static bool IsFolderName(string input) => !string.IsNullOrWhiteSpace(input) && FolderName.IsMatch(input);
    
    public static bool IsWindowsPath(string input) => !string.IsNullOrWhiteSpace(input) && WindowsPath.IsMatch(input);
    
    public static bool IsUnixPath(string input) => !string.IsNullOrWhiteSpace(input) && UnixPath.IsMatch(input);
    
    public static bool IsSemVer(string input) => !string.IsNullOrWhiteSpace(input) && SemVer.IsMatch(input);
    
    public static bool IsMinecraftVersion(string input) => !string.IsNullOrWhiteSpace(input) && MinecraftVersion.IsMatch(input);
    
    public static bool IsJavaVersion(string input) => !string.IsNullOrWhiteSpace(input) && JavaVersion.IsMatch(input);
    
    public static bool IsHexColor(string input) => !string.IsNullOrWhiteSpace(input) && HexColor.IsMatch(input);
    
    public static bool IsGuid(string input) => !string.IsNullOrWhiteSpace(input) && Guid.IsMatch(input);
    
    public static bool IsPhoneNumber(string input) => !string.IsNullOrWhiteSpace(input) && PhoneNumber.IsMatch(input);
    
    public static bool IsDateIso8601(string input) => !string.IsNullOrWhiteSpace(input) && DateIso8601.IsMatch(input);
    
    public static bool IsDateTimeIso8601(string input) => !string.IsNullOrWhiteSpace(input) && DateTimeIso8601.IsMatch(input);
    
    public static bool IsIPv4WithPort(string input) => !string.IsNullOrWhiteSpace(input) && IPv4WithPort.IsMatch(input);
    
    public static bool IsDomainWithPort(string input) => !string.IsNullOrWhiteSpace(input) && DomainWithPort.IsMatch(input);
}