namespace NcForms
{

    /// <summary>
    /// NcForm style class
    /// </summary>
    public class NcFormStyle
    {
        /// <summary>
        /// Form style
        /// </summary>
        public NcWindowsStyles ncWindowsStyle;
        /// <summary>
        /// Form window state
        /// </summary>
        public NcFormWindowStates ncFormWindowState;
        /// <summary>
        /// Bars font
        /// </summary>
        public Font barsFont, barsFontBold;


        public NcFormStyle(NcWindowsStyles ncwStyle, NcFormWindowStates ncfwState, Font barFont, int barFontSize)
        {
            ncWindowsStyle = ncwStyle;
            ncFormWindowState = ncfwState;
            barsFont = new Font(barFont.FontFamily, barFontSize, FontStyle.Regular);    // Crea due font (normale e grassetto)...
            barsFontBold = new Font(barFont.FontFamily, barFontSize, FontStyle.Bold);   // ...della dimensione richiesta

            //barsFontBold = new Font(barFont, 

        }
        public NcFormStyle(NcWindowsStyles ncwStyle, NcFormWindowStates ncfwState) : this(ncwStyle, ncfwState, SystemFonts.DefaultFont, SystemFonts.DefaultFont.Height) { }


        /// <summary>
        /// Default double clict time on toolbars
        /// </summary>
        public static double dblclkOnBarSecondsDefault = 0.3;
        /// <summary>
        /// Normal with everything
        /// </summary>
        public static NcFormStyle Normal = new NcFormStyle(NcWindowsStyles.All, NcFormWindowStates.Normal);
        /// <summary>
        /// Simple with menu and min/max buttons
        /// </summary>
        public static NcFormStyle Simple = new NcFormStyle(NcWindowsStyles.Menu | NcWindowsStyles.MinMax | NcWindowsStyles.Resizable, NcFormWindowStates.Normal);
        /// <summary>
        /// Fixed with only help, not resizable
        /// </summary>
        public static NcFormStyle Fixed = new NcFormStyle(NcWindowsStyles.Help | NcWindowsStyles.TopMost | NcWindowsStyles.LowerBar, NcFormWindowStates.Normal);

    }


}