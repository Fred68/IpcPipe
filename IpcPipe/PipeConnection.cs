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

namespace IpcPipes
{
	// Versione di linguaggio C# compatibile con .NET 9.0 e .Net Framework 4.8.1 (non ha tipi nullable)
	#pragma warning disable CS8618     // Disabilita warning per campi non inizializzati (non nullable)                                                                      
	

	public class MyClass
	{
		double _x;
		string _str;

		public double X
		{
			get { return _x;}
			set { _x = value; }
		}	

		public string Str
		{
			get{ return _str;}
			set { _str = value; }
		}
		public MyClass()
		{}

		public MyClass(double x, string str)
		{
			_x = x;
			_str = str;
		}
	}


	public class PipeConnection : I_ID
	{
		public static int ID_ERROR = List_ID<PipeConnection>.ID_ERROR;		// ID di errore per la creazione della connessione

		int _id;
		bool isMaster;
		string writePipeName,readPipeName;
		NamedPipeServerStream psW;
		NamedPipeClientStream psR;
		StreamReader sr;
		StreamWriter sw;
		bool isSync;
		int _id_other;

		private static List_ID<Cmd> _commands;								// Lista dei comandi (thread safe)


		#region PROPRIETA'
		/// <summary>
		/// ID della connessione (per questo processo)
		/// </summary>
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

		/// <summary>
		/// Questo lato della connessione è master ?
		/// </summary>
		public bool IsMaster
		{
			get
			{
				return isMaster;
			}
		}

		/// <summary>
		/// Nome della pipe di scrittura
		/// </summary>
		public string WritePipeName
		{
			get
			{
				return writePipeName;
			}
		}

		/// <summary>
		///  Nome della pipe di lettura
		/// </summary>
		public string ReadPipeName
		{
			get
			{
				return readPipeName;
			}
		}

		/// <summary>
		/// La connessione è sincronizzata ?
		/// </summary>
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

		/// <summary>
		/// ID della connessione (per l'altro processo)
		/// </summary>
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

		/// <summary>
		/// NamedPipeServerStream
		/// </summary>
		public NamedPipeServerStream PsW
		{
			get {return psW;}
			set {psW = value;}
		}

		/// <summary>
		/// NamedPipeClientStream
		/// </summary>
		public NamedPipeClientStream PsR
		{
			get {return psR;}
			set {psR = value;}
		}

		/// <summary>
		/// StreamReader
		/// </summary>
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

		/// <summary>
		/// StreamWriter
		/// </summary>
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
		#endregion


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
			
		/// <summary>
		/// CTOR (empty)
		/// </summary>
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

		#warning Controllare
		public int CreateCommand<T>(int nCommand, Handler<T> hnd, string name)
		{
			int idCmd = Cmd.ID_ERROR;
			Cmd<T> _cmd = new Cmd<T>(hnd, nCommand, name);
			idCmd = _commands.Add(nCommand, _cmd);
			return idCmd;
		}

		#warning Controllare
		public int CreateCommand<T>(Handler<T> hnd, string name)
		{
			int idCmd = Cmd.ID_UNDEF;
			Cmd<T> _cmd = new Cmd<T>(hnd, idCmd, name);
			idCmd = _commands.Add(_cmd);
			return idCmd;
		}

		/// <summary>
		/// Restituisce il tipo di dato associato al tipo di pacchetto
		/// Restiruisce null se il tipo di pacchetto non è registrato
		/// </summary>
		/// <param name="nType"></param>
		/// <returns></returns>
		public Type GetDataType(int nType)
		{
			#pragma warning disable CS8600
			Type tp = null;

			#warning DA SCRIVERE
			tp = typeof(MyClass);

			return tp;
			#pragma warning restore CS8600
		}

		public string TestSer(object x, Type tp, int com)
		{
			string s = string.Empty;
			if(x != null)
			{
				Pacchetto p = new Pacchetto(com, tp, x);
				s = Pacchetto.Serialize(p);
			}
			return s;
		}

		Pacchetto TestDeser(string s)
		{
			Pacchetto p = Pacchetto.Deserialize(s,this);
			return p;
		}

	}
	#pragma warning restore CS8618 
}
}
