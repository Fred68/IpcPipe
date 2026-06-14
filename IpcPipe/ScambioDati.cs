
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using List_ID;
using Newtonsoft.Json;

#pragma warning disable CS8618

namespace ScambioDati
{
	
	/// <summary>
	/// Classe base Pacchetto
	/// </summary>
	public class Pacchetto
	{
		/// <summary>
		/// Tipo pacchetto: errore
		/// </summary>
		public const int TPK_ERROR = -1;
		/// <summary>
		/// Tipo pacchetto: indefinito
		/// </summary>
		public const int TPK_UNDEF = 0;

		protected int tipo;

		/// <summary>
		/// Proprietà: Tipo
		/// </summary>
		public int Tipo { get { return tipo; } }

		/// <summary>
		/// CTOR protetto
		/// </summary>
		/// <param name="tipo"></param>
		protected Pacchetto(int tipo = Pacchetto.TPK_UNDEF) { this.tipo = tipo; }

	}

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
		public Pacchetto(int tipo = ScambioDati.Pacchetto.TPK_UNDEF) : base(tipo)
		{
			#pragma warning disable CS8625
			this.dati = null;
			#pragma warning restore CS8625
		}


		public static string Serialize(Pacchetto<T> pk)
		{
			StringBuilder sb = new StringBuilder();



			return sb.ToString();
		}



	}

	

	public class Cmd : I_ID
	{
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
		public Cmd(int nCommand, Handler<T> handler)
		{
			_handler = new Handler<T>(handler);
			ID = nCommand;
		}
	}

}

#pragma warning restore CS8618
