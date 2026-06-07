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
		
			#warning AGGIUNGERE DIZIONARIO CON: ID, Parola chiave, delegate
			

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

		}

		
	}
}

#pragma warning restore CS8618 
