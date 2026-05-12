
using Fred68.CfgReader;
using NcForms;
using IpcPipes;

namespace IpcPipeM
{
    internal static class Program
    {

		public readonly static string _cfgFile = "CFG.cfg";
		static string? _usrCfgFile, _path;
        static CFG? cfg;       
       

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();			// Generato dal compilatore, chiamata necessaria
			
            cfg = new CFG();								// Configurazione
            cfg.CHR_ListSeparator = @";";					// Separatore di liste

			// Legge gli argomenti della chiamata e imposta il file di configurazione
			bool useArg = false;
			if(args.Length > 0)
            {
                if(args.Length == 1)
                {
                    _usrCfgFile = args[0];
                    if(File.Exists(_usrCfgFile))
                    {
                        useArg = true;
                        MessageBox.Show($"Found '{_usrCfgFile}' user configuration file");
                    }
                    else
                    {
                        MessageBox.Show($"User configuration file '{_usrCfgFile}' not found.{System.Environment.NewLine}Using default '{_cfgFile}' file.");
                    }
                }
                else
                {
                    MessageBox.Show("Too many arguments" + Environment.NewLine + cfg.Message);
                    return;
                }

            }

			// Legge il file di configurazione
			try
            {
			    cfg.ReadConfiguration(useArg ? _usrCfgFile : _cfgFile);    // Legge il file di confogurazione
                cfg.GetNames(true, false);		// Solo le voci del dizionario presenti nella classe derivata
            }
            catch
            {
                MessageBox.Show("Error reading configuration file:" + Environment.NewLine + cfg.Message);
                return;
            }

			// Se c'è un errore nella configurazione, esce
			if(!cfg.IsOk)
            {
                MessageBox.Show(cfg.Message);
                return;
            }

            cfg.Clear();						// Svuota il dizionario

			
            //int fontSz = cfg.FontSize > 0 ? cfg.FontSize : SystemFonts.DefaultFont.Height;

			// Stile della finestra
            NcForms.NcFormStyle ncfs = new NcForms.NcFormStyle(
                NcForms.NcWindowsStyles.TopMost
                | NcForms.NcWindowsStyles.MinMax
                | NcForms.NcWindowsStyles.Help
                | NcForms.NcWindowsStyles.LowerBar
                | NcForms.NcWindowsStyles.Menu
                | NcForms.NcWindowsStyles.Resizable
                ,
                NcForms.NcFormWindowStates.Normal,
                SystemFonts.DefaultFont,
                cfg.FontSize > 0 ? cfg.FontSize : SystemFonts.DefaultFont.Height
                );

			// Colori (usare i nomi di Windows)
            Color[] color = new Color[4];
            color[0] = Color.FromName(cfg.COL_bkgnd);
            color[1] = Color.FromName(cfg.COL_title);
            color[2] = Color.FromName(cfg.COL_status);
            color[3] = Color.FromName(cfg.COL_buttons);
            for(int i=0; i<color.Length; i++)
            {
                if(!color[i].IsKnownColor)
                {
                MessageBox.Show($"{color[i].Name} is not a valid colour");
                color[i] = Color.White;
                }
            }

			NcForms.NcFormColor ncfc = new NcForms.NcFormColor(color[0],color[1],color[2],color[3],1f);
			
			NcMsg ncfm = new NcMsg();       // Usa classe derivata di NcformMsg

			try
            {
                _path = Path.GetDirectoryName(System.Environment.ProcessPath);
            }
            catch
            {
                MessageBox.Show($"Error creating launch path");
                return;  
            }

			// Avvia il task con il MainForm derivato da NcForm (era: Application.Run(new MainForm());
            Application.Run(new MainForm(ncfs, ncfc, ncfm, cfg, _path));

        }
    }

	public class NcMsg : NcForms.NcFormMsg
	{
		public NcMsg() : base()
		{
		
		}
	}
}