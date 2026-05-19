

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using Fred68.CfgReader;

namespace IpcPipeS
{

	#warning GESTIRE LA CREAZIONE E LA CONNESSIONE ALLE PIPE CON DEI CICLI

	internal static class Program
	{

		public readonly static string _cfgFile = "CFGfw.cfg";
		static string _usrCfgFile, _path;
        static CFGfw cfg;

		/// <summary>
		/// The main entry point for the application. 
		/// </summary>
		/// <param name="args"></param>
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			cfg = new CFGfw();								// Configurazione
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
			    cfg.ReadConfiguration(useArg ? _usrCfgFile : _cfgFile);    // Legge il file di configurazione
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

			//MessageBox.Show("Messages:" + Environment.NewLine + cfg.Message);

            cfg.Clear();						// Svuota il dizionario

			//if (args.Length == 4 )
			//{
			//	Application.Run(new SecondForm(args[0],args[1],args[2],args[3]));
			//}
			//else
			//{
			//	MessageBox.Show("Argomenti errati");
			//}

			Application.Run(new SecondForm(cfg));
		}
	}
}
