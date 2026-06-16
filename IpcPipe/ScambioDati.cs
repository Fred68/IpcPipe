
using List_ID;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8618

//namespace ScambioDati
namespace IpcPipes
{
	
	/// <summary>
	/// Classe base Pacchetto
	/// </summary>
	public class Pacchetto
	{
		public enum TPK
		{
			ERROR = -1,
			ERROR_DESERIALIZE = -2,
			UNDEF = 0,
		}

		/// <summary>
		/// Tipo di pacchetto
		/// Se tipo > 0: ok, comando
		/// Se tipo == 0: indefinito
		/// Se tipo < 0: errore
		/// </summary>
		protected int tipo;

		/// <summary>
		/// Proprietà: Tipo
		/// </summary>
		public int Tipo { get { return tipo; } }

		public bool isOk { get { return true; } }


		/// <summary>
		/// CTOR protetto
		/// </summary>
		/// <param name="tipo"></param>
		protected Pacchetto(int tipo = (int)Pacchetto.TPK.UNDEF)
		{
			this.tipo = tipo;
		}

		public static string Serialize(Pacchetto pk)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(pk.tipo.ToString());					// Scrive il tipo di pacchetto così com'é (anche se errore o indefinito)
			return sb.ToString();
		}

		public static Pacchetto Deserialize(string str, PipeConnection pcon)
		{
			Pacchetto pk = new Pacchetto();
			int tipopk = (int)Pacchetto.TPK.UNDEF;
			int pos = str.IndexOf(Environment.NewLine);			// Cerca il primo fine linea
			if(pos != -1)										
			{
				string prima_linea = str.Substring(0, pos + Environment.NewLine.Length).TrimEnd();		// Estrae linea (se trovato fine linea)
				if(int.TryParse(prima_linea, out tipopk))		// Legge il tipo di pacchetto (int), se possibile
				{
					str = str.Substring(pos + Environment.NewLine.Length).TrimStart();

					Type type = pcon.GetDataType(1);
					object X = JsonConvert.DeserializeObject(str,type);

					#warning PROVARE COSI' !!!!!!!!!!! Subito.... !!!!!!

					try
					{
						pk = new Pacchetto(tipopk);
					}
					catch (Exception ex)
					{
						pk.tipo = (int)Pacchetto.TPK.ERROR;
						Debug.WriteLine($"Errore nella deserializzazione: {ex.Message}");
					}
				}
				else
				{
					pk.tipo = (int)Pacchetto.TPK.ERROR;
				}
			}
			return pk;
		}
	}



	/// <summary>
	/// Classe derivata generica Pacchetto<T>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Pacchetto<T> : Pacchetto where T : class
	{		
		T	dati;

		/// <summary>
		/// Proprietà: Dati
		/// </summary>
		public T Dati
		{
			get { return dati; }
			set { dati = value; }
		}
		
		/// <summary>
		/// CTOR con argomenti
		/// </summary>
		/// <param name="tipo"></param>
		/// <param name="dati"></param>
		public Pacchetto(int tipo, T dati) : base(tipo)
		{
			this.dati = dati;
		}

		/// <summary>
		/// CTOR vuoto
		/// </summary>
		/// <param name="tipo"></param>
		public Pacchetto(int tipo = (int)Pacchetto.TPK.UNDEF) : base(tipo)
		{
			#pragma warning disable CS8625
			this.dati = null;
			#pragma warning restore CS8625
		}

		#warning Controllare
		public static string Serialize(Pacchetto<T> pk)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(pk.tipo.ToString());					// Scrive il tipo di pacchetto così com'é (anche se errore o indefinito)
			if(pk.dati != null)
			{
				sb.AppendLine(JsonConvert.SerializeObject(pk.dati));
			}
			return sb.ToString();
		}

		#warning SEPARARE, USARE LA FUNZIONE Extract base ?
		
		
		#warning DEVE RESTITUIRE UN OBJECT + INT TIPO DI OGGETTO, PER IL CAST... MA IL TIPO T NON E' NOTO
		#warning 
		public static Pacchetto<T> Deserialize(string str, PipeConnection pcon)
		{
			#pragma warning disable CS8600
			#pragma warning disable CS8625
			#pragma warning disable CS8601
			
			T obj = null;
			
			Pacchetto<T> pk = new Pacchetto<T>();
			
			int tipopk = (int)Pacchetto.TPK.UNDEF;
			int pos = str.IndexOf(Environment.NewLine);			// Cerca il primo fine linea
			if(pos != -1)										
			{
				string prima_linea = str.Substring(0, pos + Environment.NewLine.Length).TrimEnd();		// Estrae linea (se trovato fine linea)

				if(int.TryParse(prima_linea, out tipopk))		// Legge il tipo di pacchetto (int), se possibile
				{
					str = str.Substring(pos + Environment.NewLine.Length).TrimStart();		// Resto del pacchetto
					try
					{
						pk = new Pacchetto<T>(tipopk,null);
						obj = JsonConvert.DeserializeObject<T>(str);
						pk.Dati = (T) obj;
					}
					catch (Exception ex)
					{
						pk.tipo = (int)Pacchetto.TPK.ERROR;
						Debug.WriteLine($"Errore nella deserializzazione: {ex.Message}");
					}
				}
				else
				{
					pk.tipo = (int)Pacchetto.TPK.ERROR;
				}
			}

			return pk;
			#pragma warning restore CS8601
			#pragma warning restore CS8625
			#pragma warning restore CS8600
		}

	}

	

	public class Cmd : I_ID
	{
		/// <summary>
		/// ID errore
		/// </summary>
		public const int ID_ERROR = -1;
		/// <summary>
		/// ID indefinito
		/// </summary>
		public const int ID_UNDEF = 0;

		int _id;

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
	}

	public delegate bool Handler<TH>(TH arg);

	public class Cmd<T> : Cmd
	{
		Handler<T> _handler;
		string _name;

		#region PROPRIETA'
		public string Name
		{
			
			get {return _name;}
			set {_name = value;}
		}
		#endregion
		
		/// <summary>
		/// CTOR
		/// </summary>
		/// <param name="nCommand"></param>
		/// <param name="handler"></param>
		/// <param name="name"></param>
		public Cmd(Handler<T> handler, int nCommand = Cmd.ID_UNDEF, string name = "")
		{
			_handler = new Handler<T>(handler);
			ID = nCommand;
			_name = name;
		}
		
		/// <summary>
		/// ToString()
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append($"{ID.ToString()}:{_name},{typeof(T)}");
			return sb.ToString() ;
		}
	}

}

#pragma warning restore CS8618
