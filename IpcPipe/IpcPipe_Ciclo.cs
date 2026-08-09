

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IpcPipes
{
	public partial class IpcPipe : ErrorMessages.ErrorMessages
	{

		/********************************************/
		/* Formato dati su stream:
				START_PK			inizio pacchetto					
				dati...				pacchetto in formato stringa. Il tipo di dato o pacchetto è all'interno
				END_PK				fine pacchetto

				START_PK
				...
				END_PK

				END_TR				fine trasmissione
		*/
		/********************************************/


		/// <summary>
		/// Funzione con ciclo di lettura eseguita dal thread secondario
		/// </summary>
		public static void ReadStream()
		{


			List<string> lBuff = new List<string>();					// Buffer di lettura
			bool inPk = false;											// In lettura pacchetto (dopo START_PK)

			do
			{
				string linea, header;
				int llenght;

				foreach(PipeConnection ppCon in Pipes())				// Ripete per tutte le PipeConnection
				{
					if((ppCon.IsSync) && (ppCon.Sr != null))			// Se la PipeConnection è sincronizzata (con StreamReader non nullo)...
					{													// ...legge una linea e la 'ripulisce'
						try
						{
							#pragma warning disable CS8600
							linea = ppCon.Sr.ReadLine();				
							#pragma warning restore CS8600

							if(linea != null)
							{
								
								linea = linea.Trim();
								llenght = linea.Length;
								header = (llenght < HEADER_LENGHT) ? "" : linea.Substring(0, HEADER_LENGHT);

								if(llenght > 1)
								{

								#warning AGGIUNGERE PING E PONG (con un carattere in piu' con id connessione... superfluo, usa ID e ID_other)
									switch(header)
									{
										case START_PK:					// Intestazione (non aggiunta al buffer)
										{
											lBuff.Clear();				
											inPk = true;
										}
										break;
										case END_PK:					// Fine pacchetto (non aggiunto al buffer)
										{	
											inPk = false;
											AnalysePacket(Buff2String(lBuff), ppCon);
											lBuff.Clear();
										}
										break;
										case END_TR:
										{
											lBuff.Clear();
											inPk = false;
											CycleEnabled = false;		// Modifica la proprietà (richiama altre funzioni)
										}
										break;
										
										case PING_PK:
										{
											int id_answ = ppCon.ID_other;
											
											ppCon.Ping(1,IpcPipe.PingPong.Ping);
											#warning DA SCRIVERE: LEGGE IL PING E RIPONDE CON UN PONG
										}
										break;

										case PONG_PK:
										{
											#warning DA SCRIVERE: CHIAMA L'HANDLER DEL PONG
										}
										break;

										default:
										{
											if(inPk)
											{
												lBuff.Add(linea);		// Linea con tipo di pacchetto non aggiunto al buffer
											}
										}
										break;
									} // ...switch
								} // ...if linea.Lenght > 1
							} // ...if linea != null
						}
						catch ( Exception ex )
						{
							linea = string.Empty;
							signalTxtMessage("Errore lettura linea da pipe " + ppCon.ToString() + "\n" + ex.Message);
						}
					} // ...if pipe sincronizzata
				} // ...foreach tra le connessioni
			}
			while(_cycleEnabled);

			if(signalEndCycle != null)
			{
				signalEndCycle();
			}
		}
		
		/// <summary>
		/// Trasforma le linee di List<string> in un'unica stringa
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static string Buff2String(List<string> list)
		{
			StringBuilder sb = new StringBuilder();
			foreach(string s in list)
				sb.AppendLine(s);
			return sb.ToString();
		}

		static void AnalysePacket(string str, PipeConnection pcon)
		{
			//signalTxtMessage("Stringa ricevuta\n" + str);

			Pacchetto pk = Pacchetto.Deserialize(str, pcon);

			if(pk != null)
			{
				string s = pk.ToString();
				//signalTxtMessage("Messaggio ricevuto\n" + s);

				int cmd = pk.Command;
				Type tp = pcon.GetDataType(cmd);

				/************************************************/
				// Generato da Copilot automaticamente. Probabilmente ok, ma da verificare

				if(tp != null)
				{
					if(pk.TypeDat == tp)
					{
						pcon.InvokeHandler(cmd, pk.Data);
					}
					else
					{
						signalTxtMessage("TypeDat di dato ricevuto non corrispondente a quello atteso");
					}
				}
				else
				{
					signalTxtMessage("Command ricevuto non registrato");
				}
				/************************************************/

			}
		}

		/// <summary>
		/// Abilita il ciclo di lettura e lo avvia
		/// </summary>
		public void StartCycle()
		{
			if(!CycleEnabled)
			{
				CycleEnabled = true;
				pipeReaderThread.Start();
			}
		}

		#warning Aggiungere StopCycle(bool safe = true) per arrestare il ciclo di lettura (e il thread) in modo sicuro

		/// <summary>
		/// Crea il ciclo di lettura
		/// </summary>
		/// <param name="segnala_ciclo"></param>
		/// <param name="segnala_fine_ciclo"></param>
		/// <returns></returns>
		public bool CreateCycle(CycleDelegates delegs)
		{
			bool ok = true;

			if(delegs.segnala_ciclo != null)
			{
				signalCycleEnabled = delegs.segnala_ciclo;
			}
			else
			{
				AddErrMessage("Il delegate per segnalare avvio e arresto del ciclo non può essere null");
				ok = false;
			}

			if(delegs.segnala_fine_ciclo != null)
			{
				signalEndCycle = delegs.segnala_fine_ciclo;
			}
			else
			{
				AddErrMessage("Il delegate per segnalare la fine del ciclo non può essere null");
				ok = false;
			}

			if(ok)
			{
				pipeReaderThread = new Thread(ReadStream);
			}
			return ok;
		}

		public void RegisterTextMsgHandler(DelegateString handler)
		{
			signalTxtMessage = handler;
		}

		public void RegisterPongHandler(DelegateInt handler)
		{
			signalPong = handler;
		}

	}
}
