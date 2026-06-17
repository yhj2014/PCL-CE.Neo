using System;

namespace PCL_CE.Neo.Core.Utils;

public class VersionRange(Version? minVersion, Version? maxVersion)
{
    public Version? MinVersion { get; set; } = minVersion;
    
    public Version? MaxVersion { get; set; } = maxVersion;

    public bool IsInRange(Version target) => (MinVersion != null || MaxVersion != null)
                                             && (MinVersion ?? target) <= target
                                             && (MaxVersion ?? target) >= target;
    
    public bool SetMin(Version target) => (MinVersion = (MinVersion == null || target > MinVersion) ? target : MinVersion) == target;
    
    public bool SetMax(Version target) => (MaxVersion = (MaxVersion == null || target < MaxVersion) ? target : MaxVersion) == target;
}