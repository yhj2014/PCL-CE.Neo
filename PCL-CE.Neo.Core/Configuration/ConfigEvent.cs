using System;

namespace PCL_CE.Neo.Core.Configuration;

[Flags]
public enum ConfigEvent
{
    Init = 0b00001,
    Get = 0b00010,
    Set = 0b00100,
    Reset = 0b01000,
    CheckDefault = 0b10000,
    None = 0,
    Read = Get | CheckDefault,
    Update = Set | Reset,
    Changed = Init | Update,
    All = Read | Changed
}