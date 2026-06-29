//using ScambioDati;
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


		/********************************************/
		// Costanti (con carattere ASCII ACK = 006)
		/********************************************/
		const string START_PK =	"\x0006*S*";						// Inizio pacchetto
		const string END_PK =	"\u0006*E*";						// Fine pachetto
		const string END_TR =	"\u0006*X*";						// Fine trasmissione



		/// <summary>
		/// Funzione con ciclo di lettura eseguita dal thread secondario
		/// </summary>
		public static void LeggiStream()
		{
			#warning Il Thread pipeReaderThread va arrestato, alla fine (Se5 non lo fa).

			List<string> lBuff = new List<string>();					// Buffer di lettura
			bool inPk = false;											// In lettura pacchetto (dopo START_PK)

			#warning AGGIUNGERE LE CLASSI Pacchetto (base) e la generica Pacchetto<T> : Pacchetto there T : class, per incapsulare i dati
			#warning NELLA TRASMISSIONE LE LINEE START_PK E END_PK NON FANNO PARTE DEL PACCHETTO. Il tipo di pacchetto è al suo interno.
			#warning NELLA CLASSE Pacchetto<T> creare le funzioni Serialize e Deserialize (usare Newtonsoft Json)
			#warning Nella PipeConnection, valutare lista con handler base (per Pacchetto), ma che contengono handler di Pacchetto<T>: PROVARE !!!

			do
			{
				string linea;
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
								if(linea.Length > 1)
								{
									switch(linea)
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
											AnalizzaPacchetto(Buff2String(lBuff), ppCon);
											lBuff.Clear();
										}
										break;
										case END_TR:
										{
											lBuff.Clear();
											inPk = false;
											CicloAbilitato = false;		// Modifica la proprietà (richiama altre funzioni)
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

							#warning Segnalare l'errore, ma non arrestare il ciclo...
						}
					} // ...if pipe sincronizzata
				} // ...foreach tra le connessioni
			}
			while(_cicloAbilitato);

			if(segnalaFineCiclo != null)
			{
				segnalaFineCiclo();
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
			foreach(string s in list)		sb.AppendLine(s);
			return sb.ToString();
		}

		static void AnalizzaPacchetto(string str, PipeConnection pcon)
		{
			Pacchetto pk = Pacchetto.Deserialize(str, pcon);
			
			
		}

		/// <summary>
		/// Abilita il ciclo di lettura e lo avvia
		/// </summary>
		public void AvviaCiclo()
		{
			CicloAbilitato = true;
			pipeReaderThread.Start();
		}

		/// <summary>
		/// Crea il ciclo di lettura
		/// </summary>
		/// <param name="segnala_ciclo"></param>
		/// <param name="segnala_fine_ciclo"></param>
		/// <returns></returns>
		public bool CreaCiclo(DelegateBool segnala_ciclo, DelegateNull segnala_fine_ciclo)
		{
			bool ok = true;

			if(segnala_ciclo != null)
			{
				segnalaCiclo = segnala_ciclo;
			}
			else
			{
				AddErrMessage("Il delegate per segnalare avvio e arresto del ciclo non può essere null");
				ok = false;
			}

			if(segnala_fine_ciclo != null)
			{
				segnalaFineCiclo = segnala_fine_ciclo;
			}
			else
			{
				AddErrMessage("Il delegate per segnalare la fine del ciclo non può essere null");
				ok = false;
			}

			if(ok)
			{
				pipeReaderThread = new Thread(LeggiStream);
			}
			return ok;
		}

		
	}
}
