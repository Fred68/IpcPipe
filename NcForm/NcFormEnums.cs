namespace NcForms
{

    /// <summary>
    /// NcWindowsStyles flags
    /// </summary>
    [Flags]
    public enum NcWindowsStyles
    {
        None = 0,
        Menu = 1 << 0,
        MinMax = 1 << 1,
        Help = 1 << 2,
        Resizable = 1 << 3,
        TopMost = 1 << 4,
        LowerBar = 1 << 5,
        All = -1
    }

    /// <summary>
    /// NcWindowsState enumeration
    /// </summary>
    public enum NcFormWindowStates
    {
        /// <summary>
        ///  A default sized window.
        /// </summary>
        Normal = FormWindowState.Normal,
        /// <summary>
        ///  A minimized window.
        /// </summary>
        Minimized = FormWindowState.Minimized,
        /// <summary>
        ///  A maximized window.
        /// </summary>
        Maximized = FormWindowState.Maximized,
        /// <summary>
        /// Title bar only window.
        /// </summary>
        BarOnly = 3
    }

    /// <summary>
    /// NcForm bar identifyers: upper and lower
    /// </summary>
    [Flags]
    public enum NcBars
    {
        None = 0,
        Upper = 1 << 0,
        Lower = 1 << 1,
        All = -1
    }

}