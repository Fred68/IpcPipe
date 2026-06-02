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
	public class IpcPipe : ErrorMessages.ErrorMessages
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
		protected class PipeConnection : I_ID
		{
			int _id;
			bool isMaster;
			string writePipeName,readPipeName;
			NamedPipeServerStream psW;
			NamedPipeClientStream psR;
			StreamReader sr;
			StreamWriter sw;
			bool isSync;

#warning Aggiungere timeout alla connessione alle pipe
#warning Aggiungere handshaking (scambio degli id...), da inserire come other_id.
#warning Aggiungere processo per lettura continuativa (thread dedicato o async) con evento per i messaggi ricevuti


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
				//strb.AppendLine($"Delay: {delay}");
				//strb.AppendLine($"InstanceCheck: {instanceCheck.ToString()}");
				return strb.ToString();
			}

		}

		private static int _istanze = 0;							// Numero di istanze
		private static readonly object _lockObj = new object();		// Oggetto per lock: controllo istanze
		private static List_ID<PipeConnection> _pipes;				// Lista delle connessioni (thread safe)

		public static int ID_ERROR = List_ID<PipeConnection>.ID_ERROR;		// ID di errore per la creazione della connessione
		public static string STR_SYNC1 = "Sync_1";					// Primo e...
		public static string STR_SYNC2 = "Null_2";					// ...secondo messaggio di prova per la sincronizzazione


		/// <summary>
		/// CTOR
		/// </summary>
		/// <exception cref="Exception"></exception>
		public IpcPipe()
		{
			ClearErrMessages();
			if(!CheckNuovaIstanza())
				throw new Exception(GetLastErrMessage());
			_pipes = new List_ID<PipeConnection>();
		}


		/// <summary>
		/// Controlla le istanze del processo
		/// </summary>
		/// <param name="instance_check">InstanceCheck. Multiple / Unique / KillOther</param>
		/// <returns></returns>
		public bool CheckInstances(InstanceCheck instance_check = IpcPipe.InstanceCheck.Multiple)
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
		private bool CheckNuovaIstanza(int nmax = 1)
		{
			bool ok = true;
			lock(_lockObj)
			{
				_istanze++;
			}
			if(_istanze > nmax)
			{
				AddErrMessage($"Ammesse soltanto N°{nmax} istanze");
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
		/// Connette le pipe con numero ID alla controparte (master o slave) e crea gli stream reader/writer /// </summary>
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
					if(pp.Sr == null) {}
					if(pp.Sw == null) {}
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
		/// ToString() override
		/// </summary>
		/// <returns></returns>
		
		public bool Sync(int id)
		{
			bool ok = false;
			PipeConnection pp = _pipes.GetByID(id);
			if(pp.ID != ID_ERROR)
			{
				string msg1, msg2;
				msg1 = msg2 = string.Empty;
				if(pp.IsMaster)                     // Se è master, invia un messaggio di prova e attende la risposta dallo slave, che deve essere identica
				{
					msg1 = STR_SYNC1;
					try
					{
						pp.Sw.WriteLine(msg1);
						msg2 = pp.Sr.ReadLine();
						if(msg2 != null)
						{
							if(msg1 == msg2)			// Se la connessione è sincronizzata (lato master)...
							{
								pp.Sw.WriteLine(STR_SYNC1);		// ...riconferma allo slave.
								ok = true;
							}
							else
							{
								AddErrMessage($"Messaggio di risposta non corrispondente: '{msg1}!={msg2}'");
							}
						}
						else
						{
							AddErrMessage("Messaggio di risposta nullo");
						}

					}
					catch(Exception ex)
					{
						AddErrMessage(ex.Message);
					}
				}
				else
				{										// Se è slave, attende un messaggio di prova dallo master.
					string msg;
					msg = pp.Sr.ReadLine();
					if(msg == null)
					{
						pp.Sw.WriteLine(STR_SYNC2);     // Se il messaggio è nullo, invia un messaggio diverso per far fallire la sincronizzazione
					}
					else
					{
						pp.Sw.WriteLine(msg);			// Se il messaggio è valido, lo rimanda al master per la verifica.
						
						msg = string.Empty;
						msg = pp.Sr.ReadLine();			// Poi attenda la conferma dal master.

						if(msg != null)
						{
							if(msg == STR_SYNC1)        // Se la conferma è valida, la sincronizzazione è avvenuta con successo.
							{
								ok = true;
							}
							else
							{
								AddErrMessage($"Messaggio di conferma non corrispondente: '{STR_SYNC2}!={msg}'");
							}
						}
						else
						{
							AddErrMessage("Messaggio di conferma nullo");
						}
					}
					
				}
				pp.IsSync = ok;                 // Imposta lo stato di sincronizzazione della connessione
			}
			else
			{
				AddErrMessage($"ID {id} non trovato");
			}

			return ok;
		}
		
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
