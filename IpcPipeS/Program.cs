using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IpcPipeS
{

	#warning RENDERE IL SECONDO PROGRAMMA STANDARD (1 SOLO ARGOMENTO, FIEL DI CONFIGURAZIONE)
	#warning COMMENTARE LE FUNZIONI ATTUALI
	#warning METTERE pipeMaster o pipeSlave nella configurazione
	#warning Gestire la creazione e la connessione alle pipe con dei cicli

	internal static class Program
	{
		/// <summary>
		/// Punto di ingresso del programma
		/// Deve avere N°4 argomenti:
		/// string pipeIn			Nomi delle pipe in ingresso (lettura)...
		/// string pipeOut			...e in uscita (scrittura)
		/// string strDelay			Pausa [ms] tra i polling delle code
		/// string strDelayClose	Pausa [ms] prima della chiusura
		/// 
		/// </summary>
		/// <param name="args"></param>
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);


			if (args.Length == 4 )
			{
				Application.Run(new SecondForm(args[0],args[1],args[2],args[3]));
			}
			else
			{
				MessageBox.Show("Argomenti errati");
			}
		}
	}
}
