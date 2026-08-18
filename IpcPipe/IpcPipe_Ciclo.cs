

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

#warning GESTIRE L'ARRESTO DEL CICLO DI LETTURA IN MODO DIVERSO (Messaggio END_TR).
		/*
			.	Modificare il ciclo per analizzare una PipeConnection solo se è active.
			.	Funzione per attivate/disattivare una PipeConnection (solo per il processo corrente).
				

			.	Funzione con messaggio (solo lato master) per disattivare una PipeConnection.
			.	Handler del messaggio di disattivazione della PipeConnection (solo lato slave).


			.	Funzione in IpcPipe per rimozione della pipe connection:
					. Disattiva la pipe connection (_active = false) dai lati master 
					. Desincronizza e disconnette la pipe connection (chiude le pipe)
					. chiude gli stream e le pipe
					. rimuove la PipeConnection dalla lista







		*/

		/// <summary>
		/// Funzione con ciclo di lettura eseguita dal thread secondario
		/// </summary>
		static void ReadStream()
		{

			List<string> lBuff = new List<string>();					// Buffer di lettura
			bool inPk = false;											// In lettura pacchetto (dopo START_PK)

			do
			{
				string linea, header;
				int llenght;

				foreach(PipeConnection ppCon in Pipes())				// Ripete per tutte le PipeConnection
				{
					if(	(ppCon.IsSync) &&								// Se la connessione è sincronizzata...
						(ppCon.Sr != null) &&                           // ...con StreamReader non nullo...
						(ppCon.IsActive))								// ... e attiva:
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
											#warning GESTIRE L'ARRESTO IN MODO DIVERSO. VD. INIZIO FILE.
											lBuff.Clear();
											inPk = false;
											CycleEnabled = false;		// proprietà: richiama altre funzioni
										}
										break;
										
										case PING_PK:
										{
											ppCon.Sw.WriteLine(PONG_PK);
										}
										break;

										case PONG_PK:
										{
											if(_signalPong != null)
											{
												_signalPong(ppCon.ID);
											}
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
							_signalTxtMessage("Errore lettura linea da pipe " + ppCon.ToString() + "\n" + ex.Message);
						}
					} // ...if pipe sincronizzata
				} // ...foreach tra le connessioni
			}
			while(_cycleEnabled);

			if(_signalEndCycle != null)
			{
				_signalEndCycle();
			}
		}
		
		/// <summary>
		/// Trasforma le linee di List<string> in un'unica stringa
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		static string Buff2String(List<string> list)
		{
			StringBuilder sb = new StringBuilder();
			foreach(string s in list)
				sb.AppendLine(s);
			return sb.ToString();
		}

		static void AnalysePacket(string str, PipeConnection pcon)
		{
			Pacchetto pk = Pacchetto.Deserialize(str, pcon);	// _signalTxtMessage("Stringa ricevuta\n" + str);

			if(pk != null)
			{
				
				int cmd = pk.Command;
				Type tp = pcon.GetDataType(cmd);				// string s = pk.ToString(); _signalTxtMessage("Ricevuto\n" + s);

				if(tp != null)
				{
					if(pk.TypeDat == tp)
					{
						pcon.InvokeHandler(cmd, pk.Data);
					}
					else
					{
						_signalTxtMessage("TypeDat di dato ricevuto non corrispondente a quello atteso");
					}
				}
				else
				{
					_signalTxtMessage("Command ricevuto non registrato");
				}

			}
		}

	}		
}
