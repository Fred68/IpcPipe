using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Se3
	{
	public class MessaggioErrore
		{
		public string Messaggio { get; set; }
		public string Dettaglio { get; set; }
		public MessaggioErrore(string msg, string det)
			{
			Messaggio = msg;
			Dettaglio = det;
			}
		public string ToLine()
			{
			return Messaggio + ((Dettaglio.Length > 0) ? Messaggi.SeparatoreMsg : "") + Dettaglio + System.Environment.NewLine;
			}
		}

	static public class Messaggi
		{
		static int LISTE = 2;
		static List<MessaggioErrore>[] _msg = new List<MessaggioErrore>[LISTE];

		public static string SeparatoreMsg = " - ";
        /// <summary>
        /// Messaggi di errore
        /// </summary>
		public struct ERR
			{
			public static string NOME_FILE = "Errore nel nome file";
			public static string NOME_FILE_INCOMPLETO = "Nome file incompleto";
			public static string NOME_FILE_NULLO = "Nome file nullo";
			public static string CODICE = "Errore nel codice";
			public static string PARAMETRO = "Parametro funzione errato";
			public static string IO = "Errore durante operazioni su disco";
			public static string FOLDER = "Cartella esistente o errata";
			public static string FILE = "File esistente o errato";
			public static string NO_FILE = "File inesistente o errato";
            public static string NO_PROPSEED = "File propseed.txt non trovato, illeggibile o vuoto";
			public static string CLIPBOARD = "Errore nella lettura del Clipboard";
			public static string TITOLI = "Lista dei titoli errata";
			public static string NO_TITOLI = "Tabella con titoli incompleti";
			public static string TAB_VUOTA = "Tabella errata o vuota";
            public static string SOLIDEDGE = "Errore Solid Edge";
            public static string ISTANZA_SE_ATTIVA = "Istanza di Solid Edge già attiva";
			public static string ISTANZA_SE_NON_ATTIVA = "Impossibile connettersi ad un'istanza di Solid Edge";
			public static string NESSUN_DOCUMENTO_SE = "Nessun documento di Solid Edge attivo";
			public static string NO_DRAFT = "Il documento attivo non è un draft";
			public static string NO_DOC_DRAFT = "Nessun documento draft attivo";
			public static string NO_MODEL = "Il documento attivo non è file 3D";
			public static string OPEN_FILE = "Errore nell'apertura del file";
			public static string FILE_ALREADY_OPEN = "File già aperto";
			public static string LETTURA_PROP_FILE = "Errore nella lettura delle proprietà del file";
			public static string SCRITTURA_PROP_DOC = "Errore nella scrittura delle proprietà sul documento attivo";
			public static string NO_PART_LIST = "Nessuna tabella di distinta nel disegno attivo";
			public static string MULTI_PART_LIST = "Presenti più di una tabella di distinta  nel disegno attivo";
			public static string UNHANDLED_FILE = "Operazione non gestita sul tipo file attivo";
			public static string NO_PROP_TIPO = "Manca la proprietà con il tipo di disegno";
			public static string NO_PROP_COD = "Manca la proprietà codice della parte";
			public static string COD_TOO_LONG = "Codice della parte troppo lungo";
			public static string CODB_TOO_LONG = "Codice troppo lungo in distinta";
			public static string ITEMDB_TOO_LONG = "Numero di caratteri dell'item troppo lungo in distinta";
			public static string NO_PROP_EXPORT = "Mancano alcune proprietà necessarie all'esportazione in DB";
			public static string NONE_PROP_EXPORT = "Nessuna proprietà per l'esportazione in DB";
			public static string NO_EXPORT = "Esportazione dati fallita";
			public static string DB_VUOTA = "Distinta base vuota";
			public static string PESO = "Peso errato";
			public static string DB_ERRATA = "Linee di distinta non corrette";
			public static string NO_NOME_PR_VISTA_PR = "Nome non valido della proprietà con la vista principale. Controllare il file di configurazione";
			public static string NOME_VISTA_NO_MODEL = "Nessun modello corrisponde alla vista principale richiesta";
			}
        /// <summary>
        /// Messaggi informativi
        /// </summary>
		public struct MSG
			{
			public static string TITOLI3 = "La lista dei titoli deve contenere quelli ralativi a: posizione, codice, quantità";
			public static string PALLINATURA = "Attenzione: pallinatura incompleta";
			public static string DISTINTAVUOTA = "Attenzione: distinta nulla";
            public static string SEAVVIATO = "Solid Edge avviato";
			public static string FILEOPEN = "File già aperto";
			public static string FILE = "File già esistente";
			public static string PROPREAD = "Lettura proprietà completata";
			public static string SCALEREAD = "Lettura scale completata";
			public static string NOME_PR_VISTA_PR = "Ottenuto nome della proprietà con la vista principale";
			public static string NO_PR_VISTA_PR = "Manca la proprietà con la vista principale, nel draft attivo";
			}
		/// <summary>
		/// Messaggi dellínterfaccia grafica (dialog box ecc...)
		/// </summary>
		public struct GUI
			{
			public struct MSG
				{
				public static string USCIRE = @"Uscire dal programma ?";
				}
			public struct TIT
				{
				public static string USCIRE = "Chiusura programma";
				}
			}
		public enum Tipo {Messaggi=0, Errori, NUM};
		/// <summary>
		/// Costruttore statico
		/// </summary>
		static Messaggi()
			{
			for(int i=0; i < (int)Tipo.NUM; i++)
				{
				_msg[i] = new List<MessaggioErrore>();
				}
			}
		/// <summary>
		/// Aggiunge un messaggio
		/// </summary>
		/// <param name="msg">Messaggio, string</param>
		/// <param name="dett">Dettagli, string</param>
		/// <param name="typ">Tipo: errore o messaggio</param>
		public static void AddMessage(string msg, string dett = "", Tipo typ = Tipo.Messaggi)
			{
			int i = (int)typ;
			if( (i>=0) && (i<(int)Tipo.NUM) )
				{
				_msg[i].Add(new MessaggioErrore(msg, dett));
				}
			}
		/// <summary>
		/// Cancella i messaggi del tipo indicato (oppure tutti)
		/// </summary>
		/// <param name="typ"></param>
		public static void Clear(Tipo typ = Tipo.NUM)
			{
			int i = (int)typ;
			if (i == (int)Tipo.NUM)
				{
				foreach (List<MessaggioErrore> lst in _msg)
					lst.Clear();
				}
			else if ((i >= 0) && (i < (int)Tipo.NUM))
				_msg[i].Clear();
			}
		/// <summary>
		/// Enumeratore per i messaggi
		/// </summary>
		/// <param name="typ">Tipo: errore o messaggio</param>
		/// <returns>IEnumerable<MessaggioErrore></returns>
		public static IEnumerable<MessaggioErrore> Messages(Tipo typ)
			{
			int i = (int)typ;
			if((i >= 0) && (i < (int)Tipo.NUM))
				{
				foreach (MessaggioErrore str in _msg[i])
					yield return str;
				}
			yield break;
			}
		/// <summary>
		/// Numero di messaggi
		/// </summary>
		/// <param name="typ">Tipo: errori o messaggi</param>
		/// <returns>int</returns>
		public static int Nmessages(Tipo typ)
			{
			int n = 0;
			int i = (int) typ;
			if ((i >= 0) && (i < (int)Tipo.NUM))
				{
				n = _msg[i].Count;
				}
			return n;
			}
		/// <summary>
		/// Restiruisce true se ci sono messaggi o errori
		/// </summary>
		/// <param name="typ"></param>
		/// <returns>bool</returns>
		public static bool HasMessages(Tipo typ)
			{
			bool hasMsg = false;
			if (Nmessages(typ) > 0)
				hasMsg = true;
			return hasMsg;
			}
		/// <summary>
		/// Restituisce un'unica stringa con i messaggi
		/// </summary>
		/// <param name="typ"></param>
		/// <returns>string</returns>
		public static string ToString(Messaggi.Tipo typ)
			{
			StringBuilder strb = new StringBuilder();
			List<string> lm = new List<string>();

			foreach (MessaggioErrore msg in Messages(typ))
				lm.Add(msg.ToLine());
			lm = lm.Distinct().ToList();

			foreach (string str in lm)
				strb.Append(str /*+ Environment.NewLine*/);
			return strb.ToString();
			}
		/// <summary>
		/// Estrae i messaggi completi
		/// </summary>
		/// <returns>string</returns>
        public static string MessaggiCompleti()
            {
            StringBuilder strb = new StringBuilder();
			string s1, s2;
			s1 = Messaggi.ToString(Messaggi.Tipo.Errori);
			s2 = Messaggi.ToString(Messaggi.Tipo.Messaggi);
			if(s1.Length > 0)
				strb.Append("Errori"+ System.Environment.NewLine + s1+ Environment.NewLine);
			if(s2.Length > 0)
				strb.Append("Avvisi"+ System.Environment.NewLine + s2);
            return strb.ToString();
            }
        }
	}
