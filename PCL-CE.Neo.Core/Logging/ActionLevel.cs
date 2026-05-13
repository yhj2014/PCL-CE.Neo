namespace PCL_CE.Neo.Core.Logging;

/// <summary>
/// Action level that decides what to do when a log is received.
/// </summary>
public enum ActionLevel
{
    /// <summary>
    /// Only trace log, do nothing else.
    /// </summary>
    TraceLog = 0,
    
    /// <summary>
    /// Normal log, no UI interaction.
    /// </summary>
    NormalLog = 1,
    
    /// <summary>
    /// Show hint without error theme.
    /// </summary>
    Hint = 2,
    
    /// <summary>
    /// Show hint with error theme.
    /// </summary>
    HintErr = 3,
    
    /// <summary>
    /// Show message box without error theme.
    /// </summary>
    MsgBox = 4,
    
    /// <summary>
    /// Show message box with error theme.
    /// </summary>
    MsgBoxErr = 5,
    
    /// <summary>
    /// Show fatal message box.
    /// </summary>
    MsgBoxFatal = 6
}
