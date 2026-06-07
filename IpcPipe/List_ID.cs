using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace List_ID
{

	public interface I_ID
	{
		int ID
		{
			get; set;
		}
	}

	/// <summary>
	/// CTOR: List_ID<T> where T : class, I_ID, new()
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class List_ID<T> where T : class, I_ID, new()
	{
		/// <summary>
		/// ID_ERROR: valore di ID che indica errore
		/// </summary>
		public static int ID_ERROR = -1;

		readonly object _lockLst = new object();     // Oggetto per lock: controllo accesso alla lista
		List<T> _list;

		/// <summary>
		/// Count: numero di elementi nella lista (thread safe)
		/// </summary>
		public int Count
		{
			get
			{
				lock (_lockLst)
				{
					return _list.Count;
				}
			}
		}

		/// <summary>
		/// CTOR
		/// </summary>
		public List_ID()
		{
			_list = new List<T>();
		}

		/// <summary>
		/// Add: aggiunge un elemento alla lista, assegnandogli un ID univoco (thread safe)
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int Add(T item)
		{
			int id;
			lock(_lockLst)
			{
				id = GetFirstFreeID();
				item.ID = id;
				_list.Add(item);
			}
			return id;
		}

		/// <summary>
		/// GetEnumerator: restituisce un enumeratore per iterare sulla lista (non thread safe)
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		/// <summary>
		/// GetFirstFreeID: restituisce il primo ID libero (non thread safe)
		/// </summary>
		/// <returns></returns>
		protected int GetFirstFreeID()
		{
			int id = 1;
			lock(_lockLst)
			{
				while(_list.Any(x => (x.ID == id) ))
				{
					id++;
				}
			}
			return id;
		}

		/// <summary>
		/// GetbyID: restituisce l'elemento con l'ID specificato, o un elemento con ID_ERROR se non trovato (thread safe)
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public T GetByID(int id)
		{
			T item = new T();
			item.ID = ID_ERROR;

			if(_list != null)
			{
				lock(_lockLst)
				{
										
						
					#pragma warning disable CS8600
					T found = _list.Find(x => x.ID == id);
					#pragma warning restore CS8600 
					if(found != null)
					{
						item = found;
					}
				}
			}
			return item;
		}

	}
}
