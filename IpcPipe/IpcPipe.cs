
#define _IPC_SIGLETON
#undef _IPC_SIGLETON

using List_ID;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;				// Pipe		
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
	/// Delegate con argomento stringa e intero
	/// </summary>
	/// <param name="str"></param>
	public delegate void DelegateStrInt(string str, int num);
	/// <summary>
	/// Delegate con argomento stringa
	/// </summary>
	/// <param name="str"></param>
	public delegate void DelegateString(string str);
	/// <summary>
	/// Delegate con argomento intero
	/// </summary>
	/// <param name="i"></param>
	public delegate void DelegateInt(int i);

	public partial class IpcPipe : ErrorMessages.ErrorMessages
	{
		/********************************************/
		// Costanti (con carattere ASCII ACK = 006)
		/********************************************/
		public const string PKH = "\x6";
		public const string START_PK =	PKH + "*STR*";							// Inizio pacchetto
		public const string END_PK =	PKH + "*END*";							// Fine pachetto
		public const string END_TR =	PKH + "*XTR*";							// Fine trasmissione
		public const string PING_PK =	PKH + "*PIN*";							// Ping
		public const string PONG_PK =	PKH + "*PON*";							// Pong (risposta al ping)

		public static readonly int HEADER_LENGHT;

		
		static int _GetHeaderLenght()
		{
			int hl = -1;
			bool hl_set = false;
			string[] tmp = {START_PK, END_PK, END_TR, PING_PK, PONG_PK };		// SCRIVERE TUTTE LE COSTANTI !!!
			foreach(string s in tmp)
			{
				int l = s.Length;
				if( (!hl_set) && (l > 1) )
				{
					hl = l;
					hl_set = true;
				}
				else if(hl != l)
				{
					hl = -1;
					break;
				}
				
			}
			return hl;
		}

		/********************************************/
		// Variabili statiche
		/********************************************/
		private static int _istanze = 0;									// Numero di istanze
		private static readonly object _lockObj = new object();				// Oggetto per lock: controllo istanze

		private static List_ID<PipeConnection> _pipes;						// Lista delle connessioni (thread safe)

		// Handler obblicatori
		static DelegateBool signalCycleEnabled;								// Chiamata per segnalare esternamente la (dis)abilitazione del ciclo di lettura
		static DelegateNull signalEndCycle;									// Chiamata dopo hl'arresto del ciclo di lettura
		// Handler opzionali
		static DelegateString signalTxtMessage;                             // Chiamata per segnalare un messaggio di testo
		static DelegateInt signalPong;										// Chiamata per segnalare la risposta ad un ping

		/********************************************/
		// Variabili
		/********************************************/
		Thread pipeReaderThread;											// Thread di lettura
		static bool _cycleEnabled;                                          // Attivato thread di lettura delle pipe

		// Non serve rendere la classe singleton, c'é già un controllo di istanze nel CTOR
		#if _IPC_SIGLETON
		#pragma warning disable CS8625
				static IpcPipe _instance = null;									// Istanza della classe (singleton)
		#pragma warning restore CS8625
		#endif

		/********************************************/
		// Messaggi di sincronizzazione
		/********************************************/
		#region MESSAGGI DI SINCRONIZZAZIONE
		public static int ID_ERROR = List_ID<PipeConnection>.ID_ERROR;		// ID di errore per la creazione della connessione		
		public static string STR_SYNC = "Sync";								// Stringa di prova per la sincronizzazione
		public static string STR_SYNC_ERR = "Error";						// Stringa di errore
		public static char CHR_SEP = '|';                                   // Carattere separatore per i messaggi
		#endregion

		public enum InstanceCheck
		{
			Unique,							// Ammessa una sola istanza
			Multiple,						// Ammesse più istanze
			KillOther,						// Ammessa sola hl'ultima istanza, le altre vengono eliminate
		}

		/// <summary>
		/// Info per il CTOR di IpcPipe
		/// </summary>
		public struct Info
		{
			public string writePipe;				// Pipe di scrittura
			public string readPipe;					// Pipe di lettura
			public bool isMaster;					// Indica se è il master
			public int delay;						// Pausa per polling pipe di lettura
			public InstanceCheck instanceCheck;		// Controllo istanze

			public Info(string write_pipe, string read_pipe, bool is_master, int delay_ms, InstanceCheck instance_check)
			{
				writePipe = write_pipe;
				readPipe = read_pipe;
				isMaster = is_master;
				delay = delay_ms;
				instanceCheck = instance_check;
			}

			public override string ToString()
			{
				StringBuilder strb = new StringBuilder();
				string mst = isMaster ? "Master" : "Slave";
				strb.AppendLine($"IsMaster: {mst}");
				strb.AppendLine($"WritePipe: {writePipe}");
				strb.AppendLine($"ReadPipe: {readPipe}");	
				strb.AppendLine($"Delay: {delay}");
				strb.AppendLine($"InstanceCheck: {instanceCheck.ToString()}");
				strb.AppendLine("_pipes:");
				foreach(PipeConnection pc in _pipes)
				{
					strb.AppendLine(pc.ToString());
				}

				return strb.ToString();
			}
		}

		/// <summary>
		/// Delegates per costruttore e ciclo
		/// </summary>
		public struct CycleDelegates
		{
			public DelegateBool segnala_ciclo;
			public DelegateNull segnala_fine_ciclo;
			
			public CycleDelegates(DelegateBool segnala_ciclo, DelegateNull segnala_fine_ciclo)
			{
				this.segnala_ciclo = segnala_ciclo;
				this.segnala_fine_ciclo = segnala_fine_ciclo;
			}
		}

		public enum PingPong {Ping, Pong}

		/********************************************/
		// Proprietà
		/********************************************/
		#region PROPRIETA'
		/// <summary>
		/// Ciclo (thread di lettura) abilitato
		/// </summary>
		public static bool CycleEnabled
		{
			get
			{
				return _cycleEnabled;
			}
			set
			{
				_cycleEnabled = value;
				if (signalCycleEnabled != null)
				{
					signalCycleEnabled(_cycleEnabled);
				}
			}
		}
		
		
		
		#endregion

		/********************************************/
		// CTOR
		/********************************************/

		/// <summary>
		/// Static CTOR (verifica le costanti)
		/// </summary>
		/// <exception cref="Exception"></exception>
		static IpcPipe()
		{
			HEADER_LENGHT = _GetHeaderLenght();
			if(HEADER_LENGHT == -1)
				throw new Exception("COSTANTI DI LUNGHEZZA DIFFERENTE");
			return;
		}


		/// <summary>
		/// CTOR
		/// </summary>
		/// <exception cref="Exception"></exception>		
		#if _IPC_SIGLETON
		protected IpcPipe(CycleDelegates delegs)
		#else
		public IpcPipe(CycleDelegates delegs)
		#endif
		{
			ClearErrMessages();
			if(!CheckUniquenClassIstance())								// Ammessa una sola istanza della classe IpcPipe
				throw new Exception(GetLastErrMessage());
			_pipes = new List_ID<PipeConnection>();
			
			signalTxtMessage = EmptyDelegateString;						// Inizializza i delegate opzionali con una funzione vuota
			signalPong = EmptyDelegateInt;

			if(!CreateCycle(delegs))									// I delegate non possono essere nulli
				throw new Exception(GetLastErrMessage());
		}


		#if _IPC_SIGLETON
		public IpcPipe GetInstance(CycleDelegates delegs)
		{
			if(_instance == null)
			{
				_instance = new IpcPipe(delegs);
			}
			return _instance;
		}
		#endif

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
		/// Ottiene hl'ID (int) da un messaggio di sincronizzazione, se il messaggio è valido. Altrimenti restituisce ID_ERROR
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
		/// Crea un messaggio di sincronizzazione con hl'ID specificato, nel formato STR_SYNC + CHR_SEP + ID
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
		/// <param name="id">id della connessione</param>
		/// <returns>true se connessione riuscita, false se fallita</returns>
		public bool ConnectPipe(int id)
		{
			bool ok = false;
			PipeConnection pc = _pipes.GetByID(id);
			if(pc.ID != ID_ERROR)
			{
				try
				{
					if(pc.IsMaster)
					{
						pc.PsW.WaitForConnection();
						pc.PsR.Connect();
					}
					else
					{
						pc.PsR.Connect();
						pc.PsW.WaitForConnection();
					}
					pc.Sr = new StreamReader(pc.PsR);
					pc.Sw = new StreamWriter(pc.PsW);
					pc.Sw.AutoFlush = true;

					if(pc.Sr == null)
					{
						AddErrMessage($"Errore nella creazione dello StreamReader per la pipe {pc.ReadPipeName}");
					}
					if(pc.Sw == null)
					{
						AddErrMessage($"Errore nella creazione dello StreamWriter per la pipe {pc.WritePipeName}");
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
			PipeConnection pc = _pipes.GetByID(id);
			if(pc.ID != ID_ERROR)
			{
				string msg = string.Empty;
				int id_other = ID_ERROR;
				if(pc.IsMaster)
				{														// Se è master:
					try
					{
						pc.Sw.WriteLine(IpcPipe.SyncMsgFromId(pc.ID));	// Manda allo slave il messaggio di sincronizzazione con il proprio ID.
						#pragma warning disable CS8600
						msg = pc.Sr.ReadLine();                         // Legge la risposta dallo slave.
						#pragma warning restore CS8600 
						if(msg != null)									// Se non è nullo...
						{
							id_other = IpcPipe.IdFromSyncMsg(msg);		// ...estrae hl'id (dello slave) dal messaggio
							if(id_other != ID_ERROR)                    // Se hl'id è valido, lo memorizza
							{
								pc.ID_other = id_other;
								ok = true;
							}
							else
							{
								pc.Sw.WriteLine(STR_SYNC_ERR);			// Se no, invia messaggio di errore
								AddErrMessage($"Messaggio di risposta non valido: '{msg}'");
							}
						}
						else
						{
							pc.Sw.WriteLine(STR_SYNC_ERR);				// Se è nullo, invia messaggio di errore
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
						msg = pc.Sr.ReadLine();							// Legge il messaggio dal master
						#pragma warning restore CS8600 
						if(msg != null)									// Se non è nullo...
						{
							id_other = IpcPipe.IdFromSyncMsg(msg);		// ...estrae hl'id (del master) dal messaggio
							if(id_other != ID_ERROR)                    // Se hl'id è valido, lo memorizza
							{
								pc.ID_other = id_other;
								pc.Sw.WriteLine(IpcPipe.SyncMsgFromId(pc.ID));	// Manda al masteril messaggio di sincronizzazione con il proprio ID.
								ok = true;
							}
							else
							{
								pc.Sw.WriteLine(STR_SYNC_ERR);			// Se no, invia messaggio di errore
								AddErrMessage($"Messaggio di risposta non valido: '{msg}'");
							}

						}
						else
						{
							pc.Sw.WriteLine(STR_SYNC_ERR);				// Se è nullo, invia messaggio di errore
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
				AddErrMessage($"Connessione {id} non trovata");
			}

			pc.IsSync = ok;                 // Imposta lo stato di sincronizzazione della connessione
				
			return ok;
		}
		
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

		/// <summary>
		/// Crea il comando nCommand per la connessione nConn, con nome name e gestito dall'handler hnd
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="nCommand">int comando</param>
		/// <param name="nConn">int connessione</param>
		/// <param name="hnd">handlet</param>
		/// <param name="name">nome del comando</param>
		/// <returns></returns>
		public int CreateCommand<T>(int nCommand, int nConn, Handler<T> hnd, string name)
		{
			int idCmd = Cmd.ID_ERROR;
			PipeConnection pc = _pipes.GetByID(nConn);
			if(pc.ID != ID_ERROR)
			{
				idCmd = pc.CreateCommand<T>(nCommand, hnd, name);
			}
			return idCmd;
		}

		/// <summary>
		///  Serialize hl'oggetto T come comando idCommand appartenente alla connessione iDConnection 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="idCommand"></param>
		/// <param name="idConnection"></param>
		/// <param name="str">oggetto serializzato in stringa</param>
		/// <returns></returns>
		protected bool Serialize<T>(T obj, int idCommand, int idConnection, out string str) where T : class, new()
		{
			bool ok = false;
			str = string.Empty;
			
			PipeConnection pc = _pipes.GetByID(idConnection);		// Cerca la connessione id

			if(pc.ID != ID_ERROR)									// Se hl'ha trovata...
			{
				Type tp = pc.GetDataType(idCommand);				// ...cerca il comando idCommnand ed il tipo di dato

				if(tp != null)
				{
					Pacchetto p = new Pacchetto(idCommand, typeof(T), obj);
					string ss = Pacchetto.Serialize(p);
					if(ss.Length > 1)
					{
						str = ss;
						ok = true;
					}
				}
			}
			return ok;
		}

		/// <summary>
		/// Deserialize la stringa str come oggetto T, se il comando idCommand appartiene alla connessione id
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="str"></param>
		/// <param name="idConnection"></param>
		/// <param name="dato"></param>
		/// <returns></returns>
		protected bool Deserialize<T>(string str, int idConnection, out T dato) where T : class, new()
		{
			bool ok = false;
			dato = new T();

			PipeConnection pc = _pipes.GetByID(idConnection);		// Cerca la connessione id

			if(pc.ID != ID_ERROR)									// Se trovata
			{
				Pacchetto p = Pacchetto.Deserialize(str,pc);
				if(p.isOk)
				{
					if(p.TypeDat == typeof(T) && (p.Data != null))
					{
						dato = (T) p.Data;
						ok = true;
					}
				}
			}
			
			return ok;
		}

		/// <summary>
		/// Invia alla connessione id il comando idCommand con hl'oggetto T obj
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj">Oggetto di tipo T</param>
		/// <param name="idCommand">int if del comando</param>
		/// <param name="idConnection">int if della connessione</param>
		/// <returns></returns>
		public bool SendCommand<T>(T obj, int idCommand, int idConnection) where T : class, new()
		{
			bool ok = false;
			string str = string.Empty;
			if(Serialize<T>(obj, idCommand, idConnection, out str))
			{
				PipeConnection pc = _pipes.GetByID(idConnection);
				if(pc.ID != ID_ERROR)
				{
					if(pc.IsSync)
					{
						StringBuilder sb = new StringBuilder();
						sb.AppendLine(START_PK);
						sb.Append(str);
						sb.AppendLine(END_PK);
						try
						{
							pc.Sw.AutoFlush = true;
							pc.Sw.WriteLine(sb.ToString());
							ok = true;
						}
						catch(Exception ex)
						{
							AddErrMessage(ex.Message);
						}
					}
					else
					{
						AddErrMessage($"Connessione {pc.ID} non sincronizzata");
					}
				}
			}
			return ok;
		}

		public bool Ping(int idConnection)
		{
			bool ok = false;
			PipeConnection pc = _pipes.GetByID(idConnection);
			if(pc.ID != ID_ERROR)
			{
				if(pc.IsSync)
				{
					try
					{
						pc.Sw.AutoFlush = true;
						pc.Sw.WriteLine(PING_PK);
						ok = true;
					}
					catch(Exception ex)
					{
						AddErrMessage(ex.Message);
					}	
				}
			}
			return ok;
		}

#warning Il Thread pipeReaderThread va arrestato, alla fine (Se5 non lo fa).

#warning Aggiungere SendCloseConnection(int idConnection) per chiudere la connessione e rimuoverla dalla lista _pipes

#warning VALUTARE COME GESTIRE I DATI... PROBABILMENTE ListaProprietà è abbastanza generico
#warning VALUTARE SE E COME GESTIRE GLI STATI (COMANDI MULTIPLI), MEGLIO SE INCLUSI NELLA ListaProprietà


		/// <summary>
		/// Handler vuoto per inizializzare static DelegateString signalTxtMessage 
		/// </summary>
		/// <param name="str"></param>
		public static void EmptyDelegateString(string str) {}
		/// <summary>
		/// Handler vuoto per inizializzare static DelegateInt signalPong 
		/// </summary>
		/// <param name="i"></param>
		public static void EmptyDelegateInt(int i) {}
	}
}

#pragma warning restore CS8618 
