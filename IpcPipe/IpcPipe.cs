using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;				// Pipe		
using System.Threading;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

// Versione di linguaggio C# compatibile con .NET 9.0 e .Net Framework 4.8.1 (non ha tipi nullable)

#pragma warning disable CS8618     // Disabilita warning per campi non inizializzati (non nullable)                                                                      

namespace IpcPipes
{
	public class IpcPipe : ErrorMessages.ErrorMessages
	{
		/// <summary>
		/// Info per il CTOR di IpcPipe
		/// </summary>
		public struct Info
		{
			public string writePipe;		// Pipe di scrittura
			public string readPipe;			// Pipe di lettura
			public bool isMaster;			// Indica se è il master
			public int delay;				// Pausa per polling pipe di lettura
			public bool killInstances;		// ...

			public Info(string write_pipe, string read_pipe, bool is_master, int delay_ms, bool kill_instances)
			{
				writePipe = write_pipe;
				readPipe = read_pipe;
				isMaster = is_master;
				delay = delay_ms;
				killInstances = kill_instances;
			}
		}

		protected class ProcPipes
		{
			bool isMaster;
			string writePipeName,readPipeName;
			NamedPipeServerStream psW;
			NamedPipeClientStream psR;
			StreamReader sr;
			StreamWriter sw;
			
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

			public ProcPipes(string write_pipe_name, string read_pipe_name, bool is_master)
			{
				writePipeName = write_pipe_name;
				readPipeName = read_pipe_name;
				isMaster = is_master;
			}

		}

		private static int _istanze = 0;							// Numero di istanze
		private static readonly object _lockObj = new object();     // Oggetto per lock: controllo istanze, accesso alla lista delle pipe

		static List<ProcPipes> _pipes;

#warning Aggiungere un timeout per identificare le pipe, creare quelle di scrittura e vedere se esistono quelle di lettura


		/// <summary>
		/// CTOR
		/// </summary>
		/// <exception cref="Exception"></exception>
		public IpcPipe()
		{
			ClearErrMessages();
			if(!CheckNuovaIstanza())
				throw new Exception(GetLastErrMessage());
			_pipes = new List<ProcPipes>();
		}


		/// <summary>
		/// Conta le istanze del processo
		/// Elimina le altre, se richiesto
		/// </summary>
		/// <param name="kill_other"></param>
		/// <returns></returns>
		public int CountKillInstances(bool kill_other = false)
		{
			int count = 0;
			Process current = Process.GetCurrentProcess();
			Process[] processes = Process.GetProcessesByName(current.ProcessName);
			count = processes.Length;
			if(kill_other)
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
			return count;
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
		/// Crea le pipe
		/// </summary>
		/// <param name="nfo"></param>
		/// <returns></returns>
		public bool CreaPipe(Info nfo)
		{
			bool ok = true;
			ProcPipes pp;
			try
			{
				pp = new ProcPipes(nfo.writePipe,nfo.readPipe,nfo.killInstances);
				pp.PsW = new NamedPipeServerStream(pp.WritePipeName,PipeDirection.Out);
				pp.PsR = new NamedPipeClientStream(".",pp.ReadPipeName,PipeDirection.In);
				lock(_lockObj)
				{
					_pipes.Add(pp);
				}
			}
			catch (Exception ex)
			{
				ok = false;
				AddErrMessage(ex.Message);
			}
			return ok;
		}

		

	}
}

#pragma warning restore CS8618 
