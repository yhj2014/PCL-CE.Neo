using System;

namespace PCL_CE.Neo.Core.Minecraft.Java;

/// <summary>
/// Java distribution brand types with quality ranking.
/// Higher values indicate better/more reliable distributions.
/// </summary>
public enum JavaBrandType
{
    /// <summary>
    /// Unknown or unrecognized Java distribution.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Generic OpenJDK build.
    /// </summary>
    OpenJDK = 10,

    /// <summary>
    /// Oracle JDK (commercial).
    /// </summary>
    Oracle = 20,

    /// <summary>
    /// AdoptOpenJDK (community).
    /// </summary>
    AdoptOpenJDK = 25,

    /// <summary>
    /// Eclipse Temurin (successor to AdoptOpenJDK).
    /// </summary>
    EclipseTemurin = 30,

    /// <summary>
    /// BellSoft Liberica.
    /// </summary>
    Liberica = 25,

    /// <summary>
    /// Amazon Corretto.
    /// </summary>
    Corretto = 28,

    /// <summary>
    /// Microsoft Build of OpenJDK.
    /// </summary>
    Microsoft = 28,

    /// <summary>
    /// Azul Zulu.
    /// </summary>
    Zulu = 26,

    /// <summary>
    /// IBM Semeru (OpenJ9).
    /// </summary>
    IBMSemeru = 22,

    /// <summary>
    /// Tencent Kona.
    /// </summary>
    TencentKona = 20,

    /// <summary>
    /// Alibaba Dragonwell.
    /// </summary>
    Dragonwell = 20,

    /// <summary>
    /// GraalVM Community Edition.
    /// </summary>
    GraalVmCommunity = 35,

    /// <summary>
    /// JetBrains Runtime.
    /// </summary>
    JetBrains = 24
}

/// <summary>
/// Java source type indicating how the installation was discovered.
/// </summary>
public enum JavaSource
{
    /// <summary>
    /// Auto-scanned from system.
    /// </summary>
    AutoScanned,

    /// <summary>
    /// Manually added by user.
    /// </summary>
    ManualAdded,

    /// <summary>
    /// Downloaded and installed by launcher.
    /// </summary>
    LauncherInstalled,

    /// <summary>
    /// Imported from configuration.
    /// </summary>
    ImportedConfig
}