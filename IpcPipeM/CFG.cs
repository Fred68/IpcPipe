using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fred68.CfgReader;



namespace IpcPipeM
{
	#pragma warning disable CS8618	// Non nullable...
	public class CFG : CfgReader
	{

		// Impostazioni generali
		public bool FastQuit;
		public bool Verbose;
		public string Titolo;
		public float Opacity;
		public int MinWidth;
		public bool ShowInTaskbar;
		public int FontSize;

		// Sezione: Colori
		public string COL_bkgnd;
		public string COL_title;
		public string COL_status;
		public string COL_buttons;

		// Sezione: Scambio dati


	}
	#pragma warning restore CS8618
}
