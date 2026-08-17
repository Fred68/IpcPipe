using List_ID;
//using ScambioDati;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IpcPipes.IpcPipe;


/// <summary>
/// Per PROVE
/// </summary>


namespace IpcPipes
{
	// Versione di linguaggio C# compatibile con .NET 9.0 e .Net Framework 4.8.1 (non ha tipi nullable)
	#pragma warning disable CS8618     // Disabilita warning per campi non inizializzati (non nullable)                                                                      
	

	public class PipeConnection : I_ID
	{
		public static int ID_ERROR;				// ID di errore per la creazione della connessione

		int _id;                                // ID della connessione (per questo processo)
		int _id_other;                          // ID dell'altra connessione (per l'altro processo)
		bool _isMaster;							// True se la connessione è master
		string _writePipeName, _readPipeName;	// Nomi delle pipe di scrittura e lettura
		NamedPipeServerStream _psW;				// Pipe di scrittura
		NamedPipeClientStream _psR;				// Pipe di lettura
		StreamReader _sr;                       // StreamReader			
		StreamWriter _sw;						// StreamReader
		bool _isSync;                           // La connessione è sincronizzata con la controparte
		IpcPipe _ipcOwner;						// Oggetto IpcPipe di appartenenza
		bool _isConnected;                      // La connessione è stata stabilita (pipe aperte)
		bool _active;                           // La connessione (già sincronizzata) è attiva						

		private List_ID<Cmd> _commands;			// Lista dei comandi (thread safe)


		#region PROPRIETA'
		/// <summary>
		/// ID della connessione (per questo processo)
		/// </summary>
		public int ID
		{
			get {return _id;}
			set {_id = value;}
		}

		/// <summary>
		/// Questo lato della connessione è master ?
		/// </summary>
		public bool IsMaster
		{
			get{return _isMaster;}
		}

		/// <summary>
		/// La connessione (già sincronizzata con la controparte) è attiva ?
		/// </summary>
		public bool IsActive
		{
			get
			{
				return _active;
			}
			set
			{
				_active = value;
			}
		}

		bool isConnected
		{
			get {return _isConnected;}
		}

		/// <summary>
		/// Nome della pipe di scrittura
		/// </summary>
		public string WritePipeName
		{
			get{return _writePipeName;}
		}

		/// <summary>
		///  Nome della pipe di lettura
		/// </summary>
		public string ReadPipeName
		{
			get{return _readPipeName;}
		}

		/// <summary>
		/// La connessione è sincronizzata ?
		/// </summary>
		public bool IsSync
		{
			get{return _isSync;}
			set{_isSync = value;}
		}

		/// <summary>
		/// ID della connessione (per l'altro processo)
		/// </summary>
		public int ID_other
		{
			get{return _id_other;}
			set{_id_other = value;}
		}

		/// <summary>
		/// NamedPipeServerStream
		/// </summary>
		public NamedPipeServerStream PsW
		{
			get {return _psW;}
			protected set {_psW = value;}
		}

		/// <summary>
		/// NamedPipeClientStream
		/// </summary>
		public NamedPipeClientStream PsR
		{
			get {return _psR;}
			protected set {_psR = value;}
		}

		/// <summary>
		/// StreamReader
		/// </summary>
		public StreamReader Sr
		{
			get{return _sr;}
			set{_sr = value;}
		}

		/// <summary>
		/// StreamWriter
		/// </summary>
		public StreamWriter Sw
		{
			get{return _sw;}
			set{_sw = value;}
		}
		#endregion

		/// <summary>
		/// CTOR static
		/// </summary>
		static PipeConnection()
		{
			ID_ERROR = List_ID<PipeConnection>.ID_ERROR;
		}

		
		public PipeConnection(string write_pipe_name, string read_pipe_name, bool is_master, IpcPipe owner)
		{
			_writePipeName = write_pipe_name;
			_readPipeName = read_pipe_name;
			_isMaster = is_master;
			_isSync = false;
			_ipcOwner = owner;
			_active = false;

			try
			{
				_commands = new List_ID<Cmd>();
				_psW = new NamedPipeServerStream(_writePipeName,PipeDirection.Out);
				_psR = new NamedPipeClientStream(".",_readPipeName,PipeDirection.In);
			}
			catch(Exception ex)
			{
				_ipcOwner.AddErrMessage(ex.Message);
				throw new Exception($"Errore nella creazione della connessione {_writePipeName} - {_readPipeName}: {ex.Message}");
			}
		}
			
		/// <summary>
		/// CTOR (empty)
		/// </summary>
		public PipeConnection()
		{
			ID = ID_ERROR;
			_isSync = false;
			_active = false;
			_commands = new List_ID<Cmd>();
		}
		
		/// <summary>
		/// ToString() override
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder strb = new StringBuilder();
			strb.AppendLine($"Pipe ID: {_id}");
			string mst = _isMaster ? "Master" : "Slave";
			strb.AppendLine($"IsMaster: {mst}");
			strb.AppendLine($"WritePipe: {WritePipeName}");
			strb.AppendLine($"ReadPipe: {ReadPipeName}");	
			strb.AppendLine($"IsSync: {_isSync}");
			strb.AppendLine($"ID_other: {_id_other}");
			strb.AppendLine("_commands:");
			foreach(Cmd cmd in _commands)
			{
				strb.AppendLine(cmd.ToString());
			}
			return strb.ToString();
		}

		/// <summary>
		/// CreateCommand<T>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="nCommand">int nCommand id</param>
		/// <param name="hnd">Handler<T></param>
		/// <param name="name">Command name</param>
		/// <returns>command id o ID_ERROR se fallito (id già usato)</returns>
		public int CreateCommand<T>(int nCommand, Handler<T> hnd, string name)
		{
			int idCmd = Cmd.ID_ERROR;
			Cmd<T> _cmd = new Cmd<T>(hnd, nCommand, name);
			idCmd = _commands.Add(nCommand, _cmd);
			return idCmd;
		}

		/// <summary>
		/// CreateCommand<T>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="hnd">Handler<T></param>
		/// <param name="name">Command name</param>
		/// <returns>command id o ID_UNDEF/ID_ERROR</returns>
		public int CreateCommand<T>(Handler<T> hnd, string name)
		{
			int idCmd = Cmd.ID_UNDEF;
			Cmd<T> _cmd = new Cmd<T>(hnd, idCmd, name);
			idCmd = _commands.Add(_cmd);
			return idCmd;
		}

		/// <summary>
		/// Restituisce il tipo di dato associato al comando del pacchetto
		/// Restiruisce null se il tipo di pacchetto non è registrato
		/// </summary>
		/// <param name="idCmd"></param>
		/// <returns></returns>
		public Type GetDataType(int idCmd)
		{
			#pragma warning disable CS8600
			#pragma warning disable CS8603
			Type tp = null;

			foreach(Cmd cmd in _commands)
			{
				if(cmd.ID == idCmd)
				{
					tp = cmd.TypeDat;
				}
			}
			return tp;
			#pragma warning restore CS8603
			#pragma warning restore CS8600
			
		}

		/// <summary>
		/// Chiama l'handler associato al comando nCmd con i dati
		/// </summary>
		/// <param name="nCmd"></param>
		/// <param name="data"></param>
		/// <exception cref="Exception"></exception>
		public void InvokeHandler(int nCmd, object data)
		{
			Cmd cmd = _commands.GetByID(nCmd);
			if(cmd != null)
			{
				int id = cmd.ID;
				Type tp = cmd.TypeDat;
				
				if(!cmd.Invoke(data))    // Chiama funzione (virtuale) sovrascritta nelle classi derivate generiche Cmd<T>
				{
					throw new Exception("Errore nell'invocazione del comando");
				}
			}
		}

		/// <summary>
		/// Scrive una riga nella pipe di scrittura.
		/// Gestisce gli errori e li aggiunge alla lista nella classe ipcPipe di appartenenza
		/// </summary>
		/// <param name="line"></param>
		/// <returns>true se l'operazione è riuscita</returns>
		public bool WriteLine(string line)
		{
			bool ret = false;
			if(_sw != null)
			{
				try
				{
					_sw.WriteLine(line);		//_sw.Flush() superfluo: _sw.AutoFlush = true in Connect()
					ret = true;
				}
				catch(Exception ex)
				{
					_ipcOwner.AddErrMessage(ex.Message);
				}
			}
			return ret;
		}
	
		public bool Connect()
		{
			if(_isConnected)
			{
				_ipcOwner.AddErrMessage($"Connessione {_writePipeName} - {_readPipeName} già stabilita","",ErrorMessages.ErrorMessages.ErrType.Messages);
			}
			else
			{
				try
				{
					if(_isMaster)
					{
						_psW.WaitForConnection();
						_psR.Connect();
					}
					else
					{
						_psR.Connect();
						_psW.WaitForConnection();
					}
					_sr = new StreamReader(_psR);
					_sw = new StreamWriter(_psW);
					_sw.AutoFlush = true;

					if(_sr == null)
					{
						_ipcOwner.AddErrMessage($"Errore nella creazione dello StreamReader per la pipe {_readPipeName}");
					}
					if(_sw == null)
					{
						_ipcOwner.AddErrMessage($"Errore nella creazione dello StreamWriter per la pipe {_writePipeName}");
					}
					_isConnected = true;
				}
				catch(Exception ex)
				{
					_ipcOwner.AddErrMessage(ex.Message);
				}
			}
			return _isConnected;
		}
	}
	#pragma warning restore CS8618 
}

