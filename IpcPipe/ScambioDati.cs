
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
#pragma warning disable CS8625
#pragma warning disable CS8600

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
		protected int tp;

		/// <summary>
		/// Tipo di dato
		/// </summary>
		protected Type tpDat;

		/// <summary>
		/// Data
		/// </summary>
		protected object _data;

		/// <summary>
		/// Proprietà: Tipo
		/// > 0: ok, comando
		/// == 0: indefinito
		/// < 0: errore
		/// </summary>
		public int Tipo
		{
			get { return tp; }
			protected set { tp = value; }	
		}

		/// <summary>
		/// Tipo di dato
		/// </summary>
		public Type TipoDato
		{
			get { return tpDat; }
			protected set { tpDat = value; }
		}


		/// <summary>
		/// isOk true se Tipo > 1
		/// </summary>
		public bool isOk
		{
			get { return (tp > 0); }
		}


		/// <summary>
		/// CTOR
		/// </summary>
		/// <param name="tipo"></param>
		public Pacchetto(int tipo)
		{
			this.tp = tipo;
			this.tpDat = null;
			this._data = null;
		}

		/// <summary>
		/// CTOR
		/// </summary>
		/// <param name="type"></param>
		/// <param name="tipo"></param>
		public Pacchetto(int tipo, Type type)
		{
			this.tp = tipo;
			this.tpDat = type;
			this._data = null;
		}

		/// <summary>
		/// CTOR
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="type"></param>
		/// <param name="tipo"></param>
		public Pacchetto(int tipo, Type type, object obj)
		{
			this.tp = tipo;
			this.tpDat = type;
			this._data = obj;
		}

		/// <summary>
		/// Serialize data to string
		/// </summary>
		/// <param name="pk"></param>
		/// <returns></returns>
		public static string Serialize(Pacchetto pk)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(pk.tp.ToString());
			sb.AppendLine(pk.tpDat.ToString());
			if(pk._data != null)
			{
				sb.AppendLine(JsonConvert.SerializeObject(pk._data,pk.tpDat,null));
			}
			return sb.ToString();
		}

		public static Pacchetto Deserialize(string str, PipeConnection pcon)
		{
			Pacchetto pk;											// Pacchetto (non allocato)
			int tipopk = (int)Pacchetto.TPK.UNDEF;					// Tipo di pacchetto (comando)
			Type type = null;										// Tipo di dato
			object x = null;										// Oggetto (base)

			int pos = str.IndexOf(Environment.NewLine);											// Cerca il primo fine linea
			if(pos != -1)										
			{
				string prima_linea; 
				prima_linea= str.Substring(0, pos + Environment.NewLine.Length).TrimEnd();		// Estrae linea (se trovato fine linea)
				if(int.TryParse(prima_linea, out tipopk))										// Legge il tipo di pacchetto
				{
					str = str.Substring(pos + Environment.NewLine.Length).TrimStart();			// Elimina la prima linea			
					type = pcon.GetDataType(tipopk);											// Ottiene il tipo di dato in base al tipo di pacchetto
					
					#warning CONTROLLARE type... possibile errore se comando non presente
					try
					{
						x = JsonConvert.DeserializeObject(str,type);							// Deserializza su un object semplice
					}
					catch(Exception ex)
					{
						tipopk = (int)Pacchetto.TPK.ERROR;
						Debug.WriteLine($"Errore nella deserializzazione: {ex.Message}");	
					}
				}
				else
				{
					tipopk = (int)Pacchetto.TPK.ERROR;											// Fallita conversione del numero sulla prima linea 
				}
			}
			if((x != null) && (type != null))						// Alloca il pacchetto					
			{
				pk = new Pacchetto(tipopk, type, x);						
			}
			else
			{
				pk = new Pacchetto(tipopk);
			}
			return pk;
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
			get {return _id;}
			set {_id = value;}
		}
		
	}

	public delegate bool Handler<TH>(TH arg);

	public class Cmd<T> : Cmd
	{
		Handler<T> _handler;
		string _name;
		Type _tp;

		#region PROPRIETA'
		public string Name
		{		
			get {return _name;}
			set {_name = value;}
		}

		public Type Type
		{
			get { return _tp; }
			private set { _tp = value; }
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
			_tp = typeof(T);
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
#pragma warning restore CS8600
#pragma warning restore CS8625
#pragma warning restore CS8618
