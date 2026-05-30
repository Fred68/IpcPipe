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

namespace IpcPipes
{
	public class IpcPipe : ErrorMessages.ErrorMessages
	{

		 
		/// <summary>
		/// Info per il CTOR di IpcPipe
		/// </summary>
		public struct Info
		{
			public bool isMaster;			// Master o slave ?
			public string writePipe;		// Pipe di scrittura
			public string readPipe;			// Pipe di lettura
			public int delay;				// Pausa per polling pipe di lettura
			public bool killInstances;		// ...

			public Info(bool master, string write_pipe, string read_pipe, int delay_ms, bool kill_instances)
			{
				this.isMaster = master;
				this.writePipe = write_pipe;
				this.readPipe = read_pipe;
				this.delay = delay_ms;
				this.killInstances = kill_instances;
			}
		}

		private static int _istanze = 0;							// Numero di istanze
		private static readonly object _lockObj = new object();		// Oggetto per lock

		#warning USARE UNA LISTA CON: pipe server, pipe client, stream reader, stream writer... usare struct
		#warning Aggiungere un timeout per identificare le pipe, creare quelle di scrittura e vedere se esistono quelle di lettura

		NamedPipeServerStream psW;
		NamedPipeClientStream psR;
		static StreamReader sr;
		static StreamWriter sw;

		/// <summary>
		/// CTOR
		/// </summary>
		/// <param name="nfo">Info nfo con i dati</param>
		/// <exception cref="Exception"></exception>
		public IpcPipe(Info nfo)
		{
			ClearErrMessages();
			if(!CheckNewInstance())
				throw new Exception(GetLastErrMessage());
			if(!CreatePipe(nfo))
				throw new Exception(GetLastErrMessage());
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
		private bool CheckNewInstance(int nmax = 1)
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
		private bool CreatePipe(Info nfo)
		{
			bool ok = true;
			try
			{
				psW = new NamedPipeServerStream(nfo.writePipe,PipeDirection.Out);
				psR = new NamedPipeClientStream(".",nfo.readPipe,PipeDirection.In);
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
