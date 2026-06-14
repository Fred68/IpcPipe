using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
		/// Aggiunge un elemento, assegna automaticamente un ID univoco (thread safe)
		/// </summary>
		/// <param name="item"></param>
		/// <returns>item ID or ID_ERROR</returns>
		public int Add(T item)
		{
			int id;
			Debug.Assert(_list != null);
			lock(_lockLst)
			{
				id = GetFirstFreeID(false);
				item.ID = id;
				_list.Add(item);
			}
			return id;
		}

		/// <summary>
		/// Aggiunge un elemento con l'ID specificato
		/// </summary>
		/// <param name="id">new id</param>
		/// <param name="item"></param>
		/// <returns>item ID or ID_ERROR</returns>
		public int Add(int id, T item)
		{
			int idTmp = ID_ERROR;
			Debug.Assert(_list != null);
			lock (_lockLst)
			{
				if(IsIDfree(id,false))
				{
					idTmp = id;
					item.ID = idTmp;
					_list.Add(item);
				}
			}
			return idTmp;
		}

		/// <summary>
		/// GetEnumerator (non thread safe)
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		/// <summary>
		/// Restituisce il primo ID libero
		/// </summary>
		/// <param name="thread_safe">se true, esegue il lock</param>
		/// <returns></returns>
		protected int GetFirstFreeID(bool thread_safe)
		{
			int id = 1;
			Debug.Assert(_list != null);
			if(thread_safe)
			{
				lock(_lockLst)
				{
					while(_list.Any(x => (x.ID == id) ))	id++;
				}
			}
			else
			{
				while(_list.Any(x => (x.ID == id) ))	id++;
			}
			return id;
		}

		/// <summary>
		/// Verifica se l'ID specificato è libero
		/// </summary>
		/// <param name="id"></param>
		/// <param name="thread_safe">se true, esegue il lock</param>
		/// <returns></returns>
		protected bool IsIDfree(int id, bool thread_safe)
		{
			bool exist = true;
			Debug.Assert(_list != null);
			if(thread_safe)
			{
				lock(_lockLst)
				{
					exist = _list.Exists(x => x.ID == id);
				}
			}
			else
			{
				exist = _list.Exists(x => x.ID == id);
			}
			return !exist;
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
			
			return item;
		}

	}
}
