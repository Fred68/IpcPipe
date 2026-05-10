namespace NcForms
{

    /// <summary>
    /// NcForm colors class
    /// </summary>
    public class NcFormColor
    {
        public Color backColor;
        public Color titleBarColor;
        public Color statusBarColor;
        public Color buttonsColor;
        public float opacity;

        public NcFormColor(Color bkgnd, Color title, Color status, Color buttons, float opac)
        {
            backColor = bkgnd;
            titleBarColor = title;
            statusBarColor = status;
            buttonsColor = buttons;
            opacity = opac;
        }

        
        public static NcFormColor Normal = new NcFormColor(Color.White, Color.White, Color.White, Color.Gray, 0.9f);
        public static NcFormColor Simple = new NcFormColor(Color.White, Color.White, Color.White, Color.Gray, 0.9f);
        public static NcFormColor Fixed = new NcFormColor(Color.White, Color.White, Color.White, Color.Gray, 0.5f);

    }


}
