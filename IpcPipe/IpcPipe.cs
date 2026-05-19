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
	public class IpcPipe
	{

		public struct Info
		{
			public string writePipe;
			public string readPipe;
			public int delay;
			public bool killInstances;

			public Info(string write_pipe, string read_pipe, int delay_ms, bool kill_instances)
			{
				writePipe = write_pipe;
				readPipe = read_pipe;
				delay = delay_ms;
				killInstances = kill_instances;
			}
		}

		private static int _istanze = 0;							// Numero di istanze
		private static readonly object _lockObj = new object();		// Oggetto per lock

		Stack<string> _msg = new Stack<string>();					// stack dei messaggi
		#warning STACK DEI MESSAGGI: AGGIUNGERE CLASSE PER GLI ERRORI ED EVENTUALE INTERFACCIA

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
			if(!CheckIstanzaUnica())
				throw new Exception(_msg.Pop());
			if(!CreaPipe(nfo))
				throw new Exception(_msg.Pop());
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
		private bool CheckIstanzaUnica()
		{
			bool ok = true;
			lock(_lockObj)
			{
				_istanze++;
			}
			if(_istanze > 1)
			{
				_msg.Push("Ammessa una sola istanza di connessione");
				ok = false;
			}
			return ok;
		}

		/// <summary>
		/// Crea le pipe
		/// </summary>
		/// <param name="nfo"></param>
		/// <returns></returns>
		private bool CreaPipe(Info nfo)
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
				_msg.Push(ex.Message);
			}
			return ok;
		}

		

	}
}
