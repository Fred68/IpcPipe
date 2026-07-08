
using List_ID;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;


#pragma warning disable CS8618
#pragma warning disable CS8625
#pragma warning disable CS8600
#pragma warning disable CS8603

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
			ERROR_TYPE_MISMATCH = -3,
			ERROR_WRONG_HEADER = -4,
			UNDEF = 0,
		}

		/// <summary>
		/// Tipo di comando del pacchetto
		/// Se tipo > 0: ok, comando
		/// Se tipo == 0: indefinito
		/// Se tipo < 0: errore
		/// </summary>
		protected int cmd;

		/// <summary>
		/// Tipo di dato
		/// </summary>
		protected Type tpDat;

		/// <summary>
		/// Data
		/// </summary>
		protected object _data;

		#region PROPRIETA'
		/// <summary>
		/// Proprietà: Comando
		/// > 0: ok
		/// == 0: indefinito
		/// < 0: errore
		/// </summary>
		public int Comando
		{
			get { return cmd; }
			protected set { cmd = value; }	
		}
	
		/// <summary>
		/// Tipo di dato
		/// </summary>
		public Type Tipo
		{
			get { return tpDat; }
			protected set { tpDat = value; }
		
		}
		/// <summary>
		/// Dato (object)
		/// </summary>
		public object Data
		{
			get { return _data; }
		}

		/// <summary>
		/// isOk true se Tipo > 1
		/// </summary>
		public bool isOk
		{
			get { return (cmd > 0); }
		}

		
		#endregion

		#region CTORs
		/// <summary>
		/// CTOR
		/// </summary>
		/// <param name="cmd"></param>
		public Pacchetto(int cmd)
		{
			this.cmd = cmd;
			this.tpDat = null;
			this._data = null;
		}

		/// <summary>
		/// CTOR
		/// </summary>
		/// <param name="type"></param>
		/// <param name="cmd"></param>
		public Pacchetto(int cmd, Type type)
		{
			this.cmd = cmd;
			this.tpDat = type;
			this._data = null;
		}

		/// <summary>
		/// CTOR
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="type"></param>
		/// <param name="cmd"></param>
		public Pacchetto(int cmd, Type type, object obj)
		{
			this.cmd = cmd;
			this.tpDat = type;
			this._data = obj;
		}
		
		#endregion

		/// <summary>
		/// Serialize data to string
		/// </summary>
		/// <param name="pk"></param>
		/// <returns></returns>
		public static string Serialize(Pacchetto pk)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(pk.cmd.ToString());				// 1° linea: int con id del pacchetto/comando
			sb.AppendLine(pk.tpDat.ToString());				// 2° linea: Type to string
			if(pk._data != null)							// 3° linea (in poi): dato serializzato
			{
				sb.AppendLine(JsonConvert.SerializeObject(pk._data,pk.tpDat,null));
			}
			return sb.ToString();
		}

		/// <summary>
		/// Deserialize
		/// </summary>
		/// <param name="str"></param>
		/// <param name="pcon"></param>
		/// <returns></returns>
		public static Pacchetto Deserialize(string str, PipeConnection pcon)
		{
			Pacchetto pk;									// Pacchetto (non allocato)
			int tipopk = (int)Pacchetto.TPK.UNDEF;			// Tipo di pacchetto (comando)
			Type type = null;								// Tipo di dato
			object x = null;								// Oggetto (base)
			int pos1, pos2;									// Posizioni prima e seconda linea
			
			try
			{
				pos1 = str.IndexOf(Environment.NewLine);
				pos2 = str.IndexOf(Environment.NewLine,(pos1!=-1) ? pos1+Environment.NewLine.Length : 0);

				if( (pos1 != -1) && (pos2 != -1))
				{
					string prima_linea, seconda_linea;
					prima_linea		= str.Substring(0, pos1 + Environment.NewLine.Length).TrimEnd();
					seconda_linea	= str.Substring(pos1, pos2-pos1 + Environment.NewLine.Length).Trim();

					str = str.Substring(pos2 + Environment.NewLine.Length).TrimStart();		// Elimina intestazione

					if(int.TryParse(prima_linea, out tipopk))					// Legge il tipo di pacchetto
					{
						type = pcon.GetDataType(tipopk);						// Ottiene il tipo di dato in base al tipo di pacchetto
						if(type.FullName == seconda_linea)						// Controlla che i tipi di dato corrispondano
						{
							x = JsonConvert.DeserializeObject(str,type);		// Deserializza su un object semplice
						}
						else
						{
							tipopk = (int)Pacchetto.TPK.ERROR_TYPE_MISMATCH;	// Tipi di dato (pacchetto/comando) diversi
						}
					}
					else
					{
						tipopk = (int)Pacchetto.TPK.ERROR_WRONG_HEADER;
					}
				}
				else
				{
					tipopk = (int)Pacchetto.TPK.ERROR_WRONG_HEADER;
				}
			}
			catch (Exception ex)
			{
				tipopk = (int)Pacchetto.TPK.ERROR;
				Debug.WriteLine($"Errore nella deserializzazione: {ex.Message}");	
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


	/// <summary>
	/// Classe Pacchetto, generica
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Pacchetto<T>:Pacchetto where T : class
	{

		#warning Classe Pacchetto<T> forse superflua.

		#region PROPRIETA
		/// <summary>
		/// Dato (T), readonly
		/// </summary>
		public new T Data
		{
			get
			{
				return (T)_data;
			}
		}
		#endregion

		/// <summary>
		/// CTOR
		/// </summary>
		/// <param name="cmd">int comando</param>
		/// <param name="data">T dato</param>
		#region CTOR
		public Pacchetto(int cmd, T data) : base(cmd, typeof(T), data)
		{
			this._data = data;
		}
		#endregion


		#warning (superfluo?) Creare le funzioni Pacchetto<T>::Serialize()/Deserialize() (con Newtonsoft Json)


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

		protected int _id;
		

		public int ID
		{
			get {return _id;}
			set {_id = value;}
		}

		public virtual Type Tipo
		{
			#warning Questa funzione non dovrebbe mai essere chiamata
			get { return (Type)null; }
			set {}
		}
		
		public Cmd()
		{
			_id = ID_UNDEF;
		}
		public Cmd(int id)
		{
			_id = id;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append($"ID command: {_id.ToString()}");
			return sb.ToString() ;
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

		public override Type Tipo
		{
			#warning usare public e protected nella classe base e new al posto di override
			get { return _tp; }
			set { _tp = value; }
		}
		#endregion
		
		/// <summary>
		/// CTOR
		/// </summary>
		/// <param name="idCmd"></param>
		/// <param name="handler"></param>
		/// <param name="name"></param>
		public Cmd(Handler<T> handler, int idCmd = Cmd.ID_UNDEF, string name = "") : base(idCmd)
		{
			_handler = new Handler<T>(handler);
			_name = name;
			_tp = typeof(T);
			int xxx = this._id;
			return;
		}

		/// <summary>
		/// ToString()
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append($"{base.ToString()}:{_name},{_tp}");
			return sb.ToString() ;
		}
	}

}
#pragma warning restore CS8603
#pragma warning restore CS8600
#pragma warning restore CS8625
#pragma warning restore CS8618
