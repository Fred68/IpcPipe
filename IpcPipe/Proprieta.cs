using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ScambioDati
{

	/// <summary>
	/// Classe: proprietà
	/// </summary>
	public class Proprieta
	{
		/// <summary>
		/// TypeDat di variabile
		/// </summary>
		public enum TypeVar
		{
			INT = 1,
			STR,
			BOOL,
			FLOAT,
			DOUBLE,
			DATE,
			None				// Ultimo 
		}

		/// <summary>
		/// Operazioni su proprietà
		/// </summary>
		[Flags]
		public enum Op
		{
			None	=	0,			// Nessuna operazione
			M		=	1<<1,		// Modifica
			A		=	1<<2,		// Aggiungi
			X		=	1<<3		// Cancella
		}

		static Op[] __oplist;		// Lista statica dell'enum Op

		string	_name;
		TypeVar	_t;
		object	_obj;
		Op		_op;	

		/// <summary>
		/// TypeDat di dato di una variabile.
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static Type GetEqType(dynamic x)
		{
			return x.GetType();
		}

		#region PROPS
		/// <summary>
		/// Proprietà: TypeDat di dato
		/// </summary>
		public TypeVar Type
		{
			get { return _t; }
			set { _t = value; }	
		}
		
		/// <summary>
		/// Proprietà: Value (dinamica)
		/// Attenzione: la proprieta' e' pubblica per deserializzazione, ma non viene controllato il tipo di dato prima del cast
		/// </summary>
		public dynamic Valore
		{
			get {return Get();}
			set
			{
				Debug.WriteLine($"PROPRIETA Value set {value} [{_t.ToString()}]");
				switch(_t)
				{
					case TypeVar.INT:
						_obj = (int) value;
						break;
					case TypeVar.STR:
						_obj = (string) value;
						break;
					case TypeVar.BOOL:
						_obj = (bool) value;
						break;
					case TypeVar.FLOAT:
						_obj = (float) value;
						break;
					case TypeVar.DOUBLE:
						_obj = (double) value;
						break;
					case TypeVar.DATE:
						_obj = (DateTime) value;
						break;
					default:
						throw new NotImplementedException("TypeDat dato non definito");
				}
			}
		}

		/// <summary>
		/// Proprietà: Name
		/// </summary>
		public string Nome
		{
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// Proprietà: Operazion
		/// </summary>
		public Op Operazione
		{
			get {return _op;}
			set { _op = value; }
		}
		#endregion

		#region CTORs

		/// <summary>
		/// Static CTOR (non public)
		/// </summary>
		static Proprieta()
		{
			__oplist = (Op[]) Enum.GetValues(typeof(Op));
		}

		/// <summary>
		/// CTOR vuoto
		/// Ammesso costruttore senza parametri.
		/// Se no, aggiungere throw new Exception("Costruttore senza argomenti");
		/// </summary>
		public Proprieta()
		{
			_name = "";
			_t = TypeVar.INT;
			_obj = 1;
			_op = Op.None;
		}
		
		/// <summary>
		/// CTOR con dato dynamic (solo System.String, .Int32, .Double, .DateTime, .Boolean, per ora).
		/// </summary>
		/// <param name="_d"></param>
		/// <param name="name"></param>
		/// <exception cref="Exception">Se tipo di dato non gestito</exception>
		public Proprieta(dynamic _d, string name = "")
		{

			if(_d is System.String)		// Non si può fare switch su Type _d.GetType();
			{
				_t = TypeVar.STR;
			}
			else if(_d is System.Double)
			{
				_t = TypeVar.DOUBLE;	 
			}
			else if(_d is System.Int32)
			{
				_t = TypeVar.INT;
			}
			else if(_d is System.Boolean)
			{
				_t = TypeVar.BOOL;
			}
			else if(_d is System.DateTime)
			{
				_t = TypeVar.DATE;
			}
			else
			{
				throw new Exception($"TypeDat: {_d.GetType().ToString()} non gestito.");
			}

			_obj = _d;
			_name = name;
			_op = Op.None;
		}
		public Proprieta(int _d, string name = "")
		{
			_name = name;
			_t = TypeVar.INT;
			_obj = _d;
			_op = Op.None;
		}
		public Proprieta(string _d, string name = "")
		{
			_name = name;
			_t = TypeVar.STR;
			_obj = _d;
			_op = Op.None;
		}
		public Proprieta(bool _d, string name = "")
		{
			_name = name;
			_t = TypeVar.BOOL;
			_obj = _d;
			_op = Op.None;
		}
		public Proprieta(float _d, string name = "")
		{
			_name = name;
			_t = TypeVar.FLOAT;
			_obj = _d;
			_op = Op.None;
		}
		public Proprieta(double _d, string name = "")
		{
			_name = name;
			_t = TypeVar.DOUBLE;
			_obj = _d;
			_op = Op.None;
		}
		public Proprieta(DateTime _d, string name = "")
		{
			_name = name;
			_t = TypeVar.DATE;
			_obj = _d;
			_op = Op.None;
		}
		
		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="prev"></param>
		public Proprieta(in Proprieta prev)
		{
			_name = prev._name;
			_t = prev._t;
			_obj = prev._obj;
			_op	= prev._op;
			// In alternativa: public Proprieta(in Proprieta prev) : this(...)
		}
		
		#endregion
		
		/// <summary>
		/// Get value (dinamico)
		/// </summary>
		/// <returns></returns>
		dynamic Get()
		{
			switch(_t)
			{
				case TypeVar.INT:
				{
					return (int)_obj;
				}
				//break;
				case TypeVar.STR:
				{
					return (string)_obj;
				}
				//break;
				case TypeVar.BOOL:
				{
					return (bool)_obj;
				}
				//break;
				case TypeVar.FLOAT:
				{
					return (float)_obj;
				}
				//break;
				case TypeVar.DOUBLE:
				{
					return (double)_obj;
				}
				//break;
				case TypeVar.DATE:
				{
					return (DateTime)_obj;
				}
				//break;
				
				default:
					throw new NotImplementedException("TypeDat dato non definito");
			}
		}

		/// <summary>
		/// ToString() [override]
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"{this.Nome}: {this.Get().ToString()} {FlagString()} [{_t.ToString()}]");
			return sb.ToString();
		}

		/// <summary>
		/// Stringa con i flags
		/// </summary>
		/// <returns></returns>
		public string FlagString()
		{
			StringBuilder sb = new StringBuilder();
			
			Op x;
			for(int i=0; i<__oplist.Length; i++)
			{
				x = __oplist[i];
				if((_op & x)!=0)
				{
					sb.Append(x.ToString());
				}
			}
			if(sb.Length > 0)
			{
				sb.Insert(0,'|');
				sb.Append('|');
			}
			return sb.ToString();
		}

		/// <summary>
		/// Ricava la maschera corripondente ai flag della stringa
		/// </summary>
		/// <param name="strFlags"></param>
		/// <returns></returns>
		Op MaskFromString(string strFlags)
		{
			Op op = Op.None;
			strFlags = strFlags.ToUpper().Trim();
			for(int i=0; i<__oplist.Length; i++)
			{
				if(strFlags.Contains(__oplist[i].ToString()))
				{
					op = op | __oplist[i];
				}
			}
			return op;
		}

		/// <summary>
		/// Imposta i flag richiesti
		/// </summary>
		/// <param name="mask">Maschera</param>
		/// <param name="set">set/reset</param>
		public void SetFlags(Op mask, bool set = true)
		{
			if(set)
			{
				_op = _op | mask;
			}
			else
			{
				_op = _op & (~mask);	
			}
		}

		/// <summary>
		/// Imposta i flag richiesti 
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="set"></param>
		/// <returns></returns>
		public void SetFlags(string flags, bool set = true)
		{
			SetFlags(MaskFromString(flags), set);
		}

		/// <summary>
		/// Restituisce true se almeno uno dei flag della maschera è attivo
		/// </summary>
		/// <param name="mask"></param>
		/// <returns></returns>
		public bool GetFlag(Op mask)
		{
			#warning DA CONTROLLARE
			return ((_op & mask) == Op.None);
		}


	}



	/// <summary>
	/// Classe: lista delle proprieta
	/// Eredita tutte le funzoni di List<>
	/// </summary>
	public class ListaProprieta : List<Proprieta>
	{
		
		/// <summary>
		/// CTOR
		/// </summary>
		public ListaProprieta() : base()
		{
			// Aggiungere operazioni	
		}

		/// <summary>
		/// CTOR
		/// </summary>
		/// <param name="pr">Proprità da aggiungere alla lista</param>
		public ListaProprieta(Proprieta pr) : base()
		{
			this.Add(pr);
		}

		/// <summary>
		/// Costruttore di copia
		/// </summary>
		/// <param name="lp"></param>
		public ListaProprieta(in ListaProprieta lp) : base()
		{
		#warning DA PROVARE, DOVREBBE ESSERE OK
			foreach(Proprieta p in lp)
			{
				this.Add(new Proprieta(p));
			}
		}

		/// <summary>
		/// Enumeratore
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Proprieta> Proprieta()
		{
			foreach(Proprieta p in this)
			{
				yield return p;
			}
		}

		/// <summary>
		/// ToString()
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach(Proprieta p in this)
			{
				sb.AppendLine(p.ToString());
			}
			return sb.ToString();
		}
	}
}
