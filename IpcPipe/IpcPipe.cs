using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;				// Pipe		
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using List_ID;

// Versione di linguaggio C# compatibile con .NET 9.0 e .Net Framework 4.8.1 (non ha tipi nullable)

#pragma warning disable CS8618     // Disabilita warning per campi non inizializzati (non nullable)                                                                      

namespace IpcPipes
{
	/// <summary>
	/// Delegate con argomento bool
	/// </summary>
	/// <param name="stat"></param>
	public delegate void DelegateBool(bool stat);
	/// <summary>
	/// Delegate senza argomenti
	/// </summary>
	public delegate void DelegateNull();
	/// <summary>
	/// Delegate con argomento stringa
	/// </summary>
	/// <param name="str"></param>
	public delegate void DelegateStrInt(string str, int num);

	public partial class IpcPipe : ErrorMessages.ErrorMessages
	{

		private static int _istanze = 0;									// Numero di istanze
		private static readonly object _lockObj = new object();				// Oggetto per lock: controllo istanze
		private static List_ID<PipeConnection> _pipes;						// Lista delle connessioni (thread safe)

		static DelegateBool segnalaCiclo;									// Chiamata per segnalare esternamente la (dis)abilitazione del ciclo di lettura
		static DelegateNull segnalaFineCiclo;								// Chiamata dopo l'arresto del ciclo di lettura

		Thread pipeReaderThread;											// Thread di lettura
		static bool _cicloAbilitato;                                        // Attivato thread di lettura delle pipe

		/********************************************/
		// Messaggi di sincronizzazione
		/********************************************/
		#region MESSAGGI DI SINCRONIZZAZIONE
		public static int ID_ERROR = List_ID<PipeConnection>.ID_ERROR;		// ID di errore per la creazione della connessione		
		public static string STR_SYNC = "Sync";								// Stringa di prova per la sincronizzazione
		public static string STR_SYNC_ERR = "Error";						// Stringa di errore
		public static char CHR_SEP = '|';                                   // Carattere separatore per i messaggi
		#endregion


		/********************************************/
		// Proprietà
		/********************************************/
		#region PROPRIETA'
		/// <summary>
		/// Ciclo (thread di lettura) abilitato
		/// </summary>
		public static bool CicloAbilitato
		{
			get
			{
				return _cicloAbilitato;
			}
			set
			{
				_cicloAbilitato = value;
				if (segnalaCiclo != null)
				{
					segnalaCiclo(_cicloAbilitato);
				}
			}
		}
		#endregion

		/********************************************/
		// CTOR
		/********************************************/
		/// <summary>
		/// CTOR
		/// </summary>
		/// <exception cref="Exception"></exception>
		public IpcPipe(DelegateBool segnala_ciclo, DelegateNull segnala_fine_ciclo)
		{
			ClearErrMessages();
			if(!CheckUniquenClassIstance())									// Ammessa una sola istanza della classe IpcPipe
				throw new Exception(GetLastErrMessage());
			_pipes = new List_ID<PipeConnection>();
			if(!CreaCiclo(segnala_ciclo, segnala_fine_ciclo))				// I delegate non possono essere nulli
				throw new Exception(GetLastErrMessage());
		}


		/********************************************/
		// Funzion membro private o protette
		/********************************************/
		#region PRIVATE

		/// <summary>
		/// Iteratore per le connessioni pipe (thread safe)
		/// Statico e privato (usato dal thread di lettura)
		/// </summary>
		/// <returns></returns>
		static IEnumerable<PipeConnection> Pipes()
		{
			lock(_lockObj)
			{
				foreach(PipeConnection pp in _pipes)
				{
					yield return pp;
				}
			}
		}

		/// <summary>
		/// Controlla che ci sia un'istanza unica della classe
		/// </summary>
		/// <returns></returns>
		bool CheckUniquenClassIstance()
		{
			bool ok = true;
			lock(_lockObj)
			{
				_istanze++;
			}
			if(_istanze > 1)
			{
				AddErrMessage($"Ammessa soltanto un'istanza della classe");
				ok = false;
			}
			return ok;
		}

		/// <summary>
		/// Ottiene l'ID (int) da un messaggio di sincronizzazione, se il messaggio è valido. Altrimenti restituisce ID_ERROR
		/// Il messaggio deve essere costituito da STR_SYNC + CHR_SEPID
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		static int IdFromSyncMsg(string msg)
		{
			int id = ID_ERROR;
			if(msg.StartsWith(STR_SYNC))
			{
				int sep_pos = msg.IndexOf(CHR_SEP);
				if(sep_pos > 0 && sep_pos < msg.Length - 1)
				{
					string id_str = msg.Substring(sep_pos + 1);
					if(!int.TryParse(id_str,out id))
					{
						id = ID_ERROR;
					}
				}
			}
			return id;
		}

		/// <summary>
		/// Crea un messaggio di sincronizzazione con l'ID specificato, nel formato STR_SYNC + CHR_SEP + ID
		/// </summary>
		/// <param name="id"></param>
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		static string SyncMsgFromId(int id)
		{
			return STR_SYNC + CHR_SEP + id.ToString();
		}


		#endregion


		/********************************************/
		// Funzion membro pubbliche
		/********************************************/

		/// <summary>
		/// Controlla le istanze del processo
		/// </summary>
		/// <param name="instance_check">InstanceCheck. Multiple / Unique / KillOther</param>
		/// <returns></returns>
		public bool CheckProcInstances(InstanceCheck instance_check = IpcPipe.InstanceCheck.Multiple)
		{
			bool ok = false;
			int count = 0;
			Process current = Process.GetCurrentProcess();
			Process[] processes = Process.GetProcessesByName(current.ProcessName);
			count = processes.Length;

			switch(instance_check)
			{
				case InstanceCheck.Multiple:
					ok = true;
					break;
				case InstanceCheck.Unique:
					if(count > 1)
					{
						AddErrMessage("Ammessa una sola istanza");
					}
					else
					{
						ok = true;
					}
					break;
				case InstanceCheck.KillOther:
					if(count > 1)
					{
						foreach(Process process in processes)
						{
							if(process.Id != current.Id)
							{
								process.Kill();
								count--;
							}
						}
					}
					if(count > 1)
						{
						AddErrMessage("Non è stato possibile eliminare tutte le altre istanze");
						}
					else
					{
						ok = true;
					}
					break;
			}
			return ok;
		}


		/// <summary>
		/// Crea una connessione pipe e la aggiunge alla lista delle connessioni
		/// </summary>
		/// <param name="nfo"></param>
		/// <returns>L'ID (int) della connessione oppure ID_ERROR in caso di errore</returns>
		public int CreatePipeConnection(Info nfo)
		{
			int id = ID_ERROR;
			PipeConnection pp;
			try
			{
				pp = new PipeConnection(nfo.writePipe,nfo.readPipe,nfo.isMaster);
				pp.PsW = new NamedPipeServerStream(pp.WritePipeName,PipeDirection.Out);
				pp.PsR = new NamedPipeClientStream(".",pp.ReadPipeName,PipeDirection.In);
				id = _pipes.Add(pp);
			}
			catch (Exception ex)
			{
				id = ID_ERROR;
				AddErrMessage(ex.Message);
			}
			return id;
		}

		/// <summary>
		/// Connette le pipe con numero 'id' alla controparte (master o slave) e crea gli stream reader/writer
		/// Le funzioni non prevedono un timeout (a meno di usar la versione asincrona)
		/// /// </summary>
		/// <param name="id"></param>
		/// <returns>true se connessione riuscita, false se fallita</returns>
		public bool ConnectPipe(int id)
		{
			bool ok = false;
			PipeConnection pp = _pipes.GetByID(id);
			if(pp.ID != ID_ERROR)
			{
				try
				{
					if(pp.IsMaster)
					{
						pp.PsW.WaitForConnection();
						pp.PsR.Connect();
					}
					else
					{
						pp.PsR.Connect();
						pp.PsW.WaitForConnection();
					}
					pp.Sr = new StreamReader(pp.PsR);
					pp.Sw = new StreamWriter(pp.PsW);
					pp.Sw.AutoFlush = true;

					if(pp.Sr == null)
					{
						AddErrMessage($"Errore nella creazione dello StreamReader per la pipe {pp.ReadPipeName}");
					}
					if(pp.Sw == null)
					{
						AddErrMessage($"Errore nella creazione dello StreamWriter per la pipe {pp.WritePipeName}");
					}
					ok = true;
				}
				catch(Exception ex)
				{
					AddErrMessage(ex.Message);
				}
			}
			else
			{
				AddErrMessage($"ID {id} non trovato");
			}
			return ok;
		}
		
		/// <summary>
		/// Sincronizza le connessioni master e slave, scambiando i rispettivi 'id'
		/// Le connessioni vengono riconosciute dal nome delle pipe
		/// </summary>
		/// <param name="id">id della connessione</param>
		/// <returns></returns>
		public bool Sync(int id)
		{
			bool ok = false;
			PipeConnection pp = _pipes.GetByID(id);
			if(pp.ID != ID_ERROR)
			{
				string msg = string.Empty;
				int id_other = ID_ERROR;
				if(pp.IsMaster)
				{														// Se è master:
					try
					{
						pp.Sw.WriteLine(IpcPipe.SyncMsgFromId(pp.ID));	// Manda allo slave il messaggio di sincronizzazione con il proprio ID.
						#pragma warning disable CS8600
						msg = pp.Sr.ReadLine();                         // Legge la risposta dallo slave.
						#pragma warning restore CS8600 
						if(msg != null)									// Se non è nullo...
						{
							id_other = IpcPipe.IdFromSyncMsg(msg);		// ...estrae l'id (dello slave) dal messaggio
							if(id_other != ID_ERROR)                    // Se l'id è valido, lo memorizza
							{
								pp.ID_other = id_other;
								ok = true;
							}
							else
							{
								pp.Sw.WriteLine(STR_SYNC_ERR);			// Se no, invia messaggio di errore
								AddErrMessage($"Messaggio di risposta non valido: '{msg}'");
							}
						}
						else
						{
							pp.Sw.WriteLine(STR_SYNC_ERR);				// Se è nullo, invia messaggio di errore
							AddErrMessage("Messaggio di risposta nullo");
						}
					}
					catch(Exception ex)
					{
						AddErrMessage(ex.Message);
					}
				}
				else
				{                                                       // Se è slave:
					try
					{
						#pragma warning disable CS8600
						msg = pp.Sr.ReadLine();							// Legge il messaggio dal master
						#pragma warning restore CS8600 
						if(msg != null)									// Se non è nullo...
						{
							id_other = IpcPipe.IdFromSyncMsg(msg);		// ...estrae l'id (del master) dal messaggio
							if(id_other != ID_ERROR)                    // Se l'id è valido, lo memorizza
							{
								pp.ID_other = id_other;
								pp.Sw.WriteLine(IpcPipe.SyncMsgFromId(pp.ID));	// Manda al masteril messaggio di sincronizzazione con il proprio ID.
								ok = true;
							}
							else
							{
								pp.Sw.WriteLine(STR_SYNC_ERR);			// Se no, invia messaggio di errore
								AddErrMessage($"Messaggio di risposta non valido: '{msg}'");
							}

						}
						else
						{
							pp.Sw.WriteLine(STR_SYNC_ERR);				// Se è nullo, invia messaggio di errore
							AddErrMessage("Messaggio di risposta nullo");
						}
					}
					catch(Exception ex)
					{
						AddErrMessage(ex.Message);
					}
				}
				
			}
			else
			{
				AddErrMessage($"ID {id} non trovato");
			}

			pp.IsSync = ok;                 // Imposta lo stato di sincronizzazione della connessione
				
			return ok;
		}

		public bool SendPacket
		/// <summary>
		/// ToString() override
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder strb = new StringBuilder();
			strb.AppendLine($"Numero istanze: {_istanze}");
			strb.AppendLine($"Numero pipe: {_pipes.Count}");
			foreach(PipeConnection pp in _pipes)
			{
				strb.AppendLine(pp.ToString());
			}
			return strb.ToString();
		}


		#warning AGGIUNGERE FUNZIONE PER INVIO DI PACCHETTI DI DATI !!!

		#warning AGGIUNGERE GESTIONE DEI DIZIONARI (id, nome comando, delegate...)
		#warning VALUTARE COME GESTIRE I DATI... PROBABILMENTE ListaProprietà è abbastanza generico
		#warning VALUTARE SE E COME GESTIRE GLI STATI (COMANDI MULTIPLI, PING/PONG), MEGLIO SE INCLUSI NELLA ListaProprietà
	}
}

#pragma warning restore CS8618 
