namespace PCL_CE.Neo.Core.Minecraft.Java;

public static class JavaConsts
{
    public const string JavaHomeEnvironmentVariable = "JAVA_HOME";
    public const string JavaPathEnvironmentVariable = "PATH";

    public const string MacOSJavaHomeBase = "/Library/Java/JavaVirtualMachines";
    public const string MacOSLibraryJava = "/Library/Internet Plug-Ins/JavaAppletPlugin.plugin/Contents/Home";

    public const string LinuxJavaHomeBase = "/usr/lib/jvm";
    public const string LinuxAlternatives = "/etc/alternatives/java";

    public const string WindowsProgramFiles = "ProgramFiles";
    public const string WindowsProgramFilesX86 = "ProgramFiles(x86)";
    public const string WindowsJavaHomeBase = "Java/jdk";
    public const string WindowsOracleJavaBase = "Oracle/Java";
    public const string WindowsOpenJdkBase = "Microsoft/openjdk";

    public static readonly string[] JavaExecutableNames = { "java", "java.exe", "javaw", "javaw.exe" };

    public static readonly string[] JavaVersionPatterns =
    {
        "version \"([^\"]+)\"",
        "([\\d.]+)",
        "openjdk version \"([^\"]+)\"",
        "java version \"([^\"]+)\""
    };

    public static readonly Dictionary<JavaBrandType, string[]> BrandKeywords = new Dictionary<JavaBrandType, string[]>
    {
        { JavaBrandType.Oracle, new[] { "Oracle", "HotSpot" } },
        { JavaBrandType.OpenJDK, new[] { "OpenJDK", "openjdk" } },
        { JavaBrandType.AdoptOpenJDK, new[] { "AdoptOpenJDK", "adoptopenjdk" } },
        { JavaBrandType.AmazonCorretto, new[] { "Corretto", "Amazon" } },
        { JavaBrandType.AzulZulu, new[] { "Zulu", "Azul" } },
        { JavaBrandType.BellSoftLiberica, new[] { "Liberica", "BellSoft" } },
        { JavaBrandType.GraalVM, new[] { "GraalVM", "graalvm" } },
        { JavaBrandType.MicrosoftOpenJDK, new[] { "Microsoft", "microsoft" } },
        { JavaBrandType.SAPMachine, new[] { "SAP", "sapmachine" } }
    };
}