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
				iPk					tipo di pacchetto (intero)
				dati...				in formato json
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
		const string START_PK =	"\x0006***S***";						// Inizio pacchetto
		const string END_PK =	"\u0006***E***";						// Fine pachetto
		const string END_TR =	"\u0006***X***";						// Fine trasmissione
		const int TPK_NULL =	-1;										// Tipo pacchetto indefinito



		/// <summary>
		/// Funzione con ciclo di lettura eseguita dal thread secondario
		/// </summary>
		public static void LeggiStream()
		{
			#warning Il Thread pipeReaderThread va arrestato, alla fine (Se5 non lo fa).

			List<string> lBuff = new List<string>();					// Buffer di lettura
			bool inPk = false;											// In lettura pacchetto (dopo START_PK)
			int nlPk = 0;												// Numero di linee dopo START_PK
			int tpPk = TPK_NULL;
			string pacchetto;											// Pacchetto da elaborare

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
										case START_PK:					// Intestazione non aggiunta al buffer
										{
											lBuff.Clear();				
											inPk = true;
											nlPk = 0;
										}
										break;
										case END_PK:					// Fine pacchetto non aggiunto al buffer
										{	
											inPk = false;
											ElaboraPacchetto(Buff2String(lBuff), tpPk, ppCon);
											lBuff.Clear();
											nlPk = 0;
										}
										break;
										case END_TR:
										{
											lBuff.Clear();
											inPk = false;
											nlPk = 0;
											CicloAbilitato = false;		// Modifica la proprietà (richiama altre funzioni)
										}
										break;
										
										default:
										{
											if(inPk)
											{
												if(nlPk == 1)
												{
													tpPk = RiconosciTipoPacchetto(linea,ppCon);
												}
												else
												{
													lBuff.Add(linea);	// Linea con tipo di pacchetto non aggiunto al buffer
												}
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
			foreach(string s in list)	sb.AppendLine(s);
			return sb.ToString();
		}

		static int RiconosciTipoPacchetto(string linea, PipeConnection pcon)
		{
			int tp = TPK_NULL;

			#warning AGGIUNGERE RICONOSCIMENTO DA DIZIONARIO della PipeConnection DELLE FUNZIONI SOTTOSCRITTE

			tp = 100;		// Per test

			return tp;
		}

		static void ElaboraPacchetto(string str, int tpk, PipeConnection pcon)
		{
			#warning COMPLETARE CON DELEGATE DA DIZIONARIO della PipeConnection
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
