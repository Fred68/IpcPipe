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
	/// Delegate per segnalare esternamente l'abilitazione/disabilitazione del ciclo di lettura
	/// </summary>
	/// <param name="stat"></param>
	public delegate void DelegateBool(bool stat);


	public partial class IpcPipe : ErrorMessages.ErrorMessages
	{
		public enum InstanceCheck
		{
			Unique,							// Ammessa una sola istanza
			Multiple,						// Ammesse più istanze
			KillOther,						// Ammessa sola l'ultima istanza, le altre vengono eliminate
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
				return strb.ToString();
			}
		}

		/// <summary>
		/// Connessione bidirezionale con pipe
		/// </summary>
		public class PipeConnection : I_ID
		{
			int _id;
			bool isMaster;
			string writePipeName,readPipeName;
			NamedPipeServerStream psW;
			NamedPipeClientStream psR;
			StreamReader sr;
			StreamWriter sw;
			bool isSync;
			int _id_other;


#warning Aggiungere processo per lettura continuativa (thread dedicato) con evento per i messaggi ricevuti


			public int ID
			{
				get
				{
					return _id;
				}
				set
				{
					_id = value;
				}
			}
			public bool IsMaster
			{
				get
				{
					return isMaster;
				}
			}
			public string WritePipeName
			{
				get
				{
					return writePipeName;
				}
			}
			public string ReadPipeName
			{
				get
				{
					return readPipeName;
				}
			}
			public bool IsSync
			{
				get
				{
					return isSync;
				}
				set
				{
					isSync = value;
				}
			}
			public int ID_other
			{
				get
				{
					return _id_other;
				}
				set
				{
					_id_other = value;
				}
			}

			public NamedPipeServerStream PsW
			{
				get {return psW;}
				set {psW = value;}
			}
			public NamedPipeClientStream PsR
			{
				get {return psR;}
				set {psR = value;}
			}
			public StreamReader Sr
			{
				get
				{
					return sr;
				}
				set
				{
					sr = value;
				}
			}
			public StreamWriter Sw
			{
				get
				{
					return sw;
				}
				set
				{
					sw = value;
				}
			}
			
			/// <summary>
			/// CTOR
			/// </summary>
			/// <param name="write_pipe_name"></param>
			/// <param name="read_pipe_name"></param>
			/// <param name="is_master"></param>
			public PipeConnection(string write_pipe_name, string read_pipe_name, bool is_master)
			{
				writePipeName = write_pipe_name;
				readPipeName = read_pipe_name;
				isMaster = is_master;
				isSync = false;
			}

			public PipeConnection()
			{
				ID = ID_ERROR;
				isSync = false;
			}
			public override string ToString()
			{
				StringBuilder strb = new StringBuilder();
				strb.AppendLine($"ID: {_id}");
				string mst = isMaster ? "Master" : "Slave";
				strb.AppendLine($"IsMaster: {mst}");
				strb.AppendLine($"WritePipe: {WritePipeName}");
				strb.AppendLine($"ReadPipe: {ReadPipeName}");	
				strb.AppendLine($"IsSync: {isSync}");
				strb.AppendLine($"ID_other: {_id_other}");
				return strb.ToString();
			}

		}

		private static int _istanze = 0;									// Numero di istanze
		private static readonly object _lockObj = new object();				// Oggetto per lock: controllo istanze
		private static List_ID<PipeConnection> _pipes;						// Lista delle connessioni (thread safe)
		static DelegateBool segnalaCiclo;                                   // Delegate per segnalare esternamente la (dis)abilitazione del ciclo di lettura


		public static int ID_ERROR = List_ID<PipeConnection>.ID_ERROR;		// ID di errore per la creazione della connessione
		public static string STR_SYNC = "Sync";								// Stringa di prova per la sincronizzazione
		public static string STR_SYNC_ERR = "Error";						// Stringa di errore
		public static char CHR_SEP = '|';									// Carattere separatore per i messaggi

		/// <summary>
		/// CTOR
		/// </summary>
		/// <exception cref="Exception"></exception>
		public IpcPipe(DelegateBool segnala_ciclo)
		{
			ClearErrMessages();
			if(!CheckUniquenClassIstance())									// Ammessa una sola istanza della classe IpcPipe
				throw new Exception(GetLastErrMessage());
			_pipes = new List_ID<PipeConnection>();
			segnalaCiclo = segnala_ciclo;
		}

		/// <summary>
		/// Iteratore per le connessioni pipe (thread safe)
		/// </summary>
		/// <returns></returns>
		public IEnumerable<PipeConnection> Pipes()
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
		/// Controlla che ci sia un'istanza unica della classe
		/// </summary>
		/// <returns></returns>
		private bool CheckUniquenClassIstance()
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
		/// Connette le pipe con numero ID alla controparte (master o slave) e crea gli stream reader/writer
		/// Le funzioni non prevedono un timeout (a meno di usar la versione asincrona)
		/// /// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
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
		/// Sincronizza e scambia gli ID delle connessioni 
		/// </summary>
		/// <param name="id"></param>
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
						msg = pp.Sr.ReadLine();                         // Legge la risposta dallo slave.
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
						msg = pp.Sr.ReadLine();							// Legge il messaggio dal master
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

		/// <summary>
		/// Ottiene l'ID (int) da un messaggio di sincronizzazione, se il messaggio è valido. Altrimenti restituisce ID_ERROR
		/// Il messaggio deve essere costotuito da STR_SYNC + CHR_SEPID
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		public static int IdFromSyncMsg(string msg)
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
		public static string SyncMsgFromId(int id)
		{
			return STR_SYNC + CHR_SEP + id.ToString();
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
	}
}

#pragma warning restore CS8618 
