using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ErrorMessages
{

	#warning CLASSE ErrorMessages da controllare (probabilmente è ok).
	/// <summary>
	/// Error messages class
	/// </summary>
	public class ErrorMessages
	{
		/// <summary>
		/// Type: message or error
		/// </summary>
		public enum ErrType {Errors = 0,	Messages,	NUM};
		
		public static readonly int N_MESSAGE_TYPES = (int)ErrType.NUM;
		public static readonly string MESSAGE_SEPARATOR = " - ";
		
		protected Stack<ErrorMsg>[] _msg;

		protected class ErrorMsg
		{
			public string Message { get; set; }
			public string Detail { get; set; }
			public ErrorMsg(string msg, string det)
			{
				Message = msg;
				Detail = det;
			}
			public string ToString(bool details = false)
			{
				return Message + ((details && (Detail.Length > 0)) ? ErrorMessages.MESSAGE_SEPARATOR : "") + Detail;
			}
		}

		/// <summary>
		/// CTOR 
		/// </summary>
		public ErrorMessages()
		{
			_msg = new Stack<ErrorMsg>[N_MESSAGE_TYPES];
			for(int i=0; i < N_MESSAGE_TYPES; i++)
			{
				_msg[i] = new Stack<ErrorMsg>();
			}
		}

		/// <summary>
		/// Add a message
		/// </summary>
		/// <param name="msg">string</param>
		/// <param name="dett">string with details</param>
		/// <param name="typ">error or message</param>
		public void AddErrMessage(string msg, string dett = "", ErrType typ = ErrType.Errors)
		{
			int i = (int)typ;
			if( (i>=0) && (i<(int)ErrType.NUM) )
			{
				_msg[i].Push(new ErrorMsg(msg, dett));
			}
		}

		/// <summary>
		/// Clear messages
		/// </summary>
		/// <param name="typ">errors, messages or both</param>
		public void ClearErrMessages(ErrType typ = ErrType.NUM)
		{
			int i = (int)typ;
			if (i == (int)ErrType.NUM)
			{
				foreach (Stack<ErrorMsg> lst in _msg)
					lst.Clear();
			}
			else if ((i >= 0) && (i < (int)ErrType.NUM))
				_msg[i].Clear();
		}

		/// <summary>
		/// Enumerator
		/// </summary>
		/// <param name="typ">errors, messages or both</param>
		/// <returns></returns>
		protected IEnumerable<ErrorMsg> Messages(ErrType typ = ErrType.NUM)
		{
			int i = (int)typ;
			if (i == (int)ErrType.NUM)
			{
				for(int j = 0; j<(int)ErrType.NUM; j++)
				{
					foreach (ErrorMsg msg in _msg[j])
						yield return msg;
				}
			}
			else if((i >= 0) && (i < (int)ErrType.NUM))
			{
				foreach (ErrorMsg msg in _msg[i])
					yield return msg;
			}
			yield break;
		}

		protected List<ErrorMsg> MessageList(ErrType typ)
		{
			List<ErrorMsg> lm = new List<ErrorMsg>();
			int i = (int) typ;
			if((i >= 0) && (i < (int)ErrType.NUM))
			{
				foreach (ErrorMsg msg in Messages(typ))
					lm.Add(msg);
				lm = lm.Distinct().ToList();	
			}
			return lm;
		}

		/// <summary>
		/// Get number of messages
		/// </summary>
		/// <param name="typ">errors, messages or both</param>
		/// <returns></returns>
		public int GetErrMessageNumber(ErrType typ = ErrType.NUM)
		{
			int n = 0;
			int i = (int) typ;
			if (i == (int)ErrType.NUM)
			{
				for(int j = 0; j<(int)ErrType.NUM; j++)
				{
					n += _msg[i].Count;
				}
			}
			else if((i >= 0) && (i < (int)ErrType.NUM))
			{
				n = _msg[i].Count;
			}
			return n;
		}

		/// <summary>
		/// Has messages
		/// </summary>
		/// <param name="typ">errors, messages or both</param>
		/// <returns></returns>
		public bool HasMessages(ErrType typ = ErrType.NUM)
		{
			bool hasMsg = false;
			if (GetErrMessageNumber(typ) > 0)
				hasMsg = true;
			return hasMsg;
		}

		/// <summary>
		/// Has errors
		/// </summary>
		/// <returns></returns>
		public bool HasErrors()
		{
			return HasMessages(ErrType.Errors);
		}

		/// <summary>
		/// Get last message
		/// </summary>
		/// <param name="typ">error or message</param>
		/// <param name="detail">with details ?</param>
		/// <returns></returns>
		public string GetLastErrMessage(ErrType typ = ErrType.Errors, bool detail = false)
		{
			StringBuilder sb = new StringBuilder();
			int i = (int) typ;
			if((i >= 0) && (i < (int)ErrType.NUM))
			{
				if(HasMessages(typ))
					sb.Append(_msg[i].Peek().ToString(detail));
			}
			return sb.ToString();
		}
		
		/// <summary>
		/// Get a string with messages
		/// </summary>
		/// <param name="typ">errors, messages or both</param>
		/// <param name="errTitle">Add line indicating errors or messages</param>
		/// <param name="details">with details ?</param>
		/// <returns></returns>
		public string GetErrMessageString(ErrType typ = ErrType.NUM, bool errTitle = true, bool details = true)
		{
			StringBuilder _msg = new StringBuilder();
			StringBuilder _sb = new StringBuilder();
			
			List<ErrType> err2proc = new List<ErrType>();		// Lista dei tipi di messaggio da considerare
			if(typ == ErrType.NUM)
			{
				err2proc.Add(ErrType.Errors);
				err2proc.Add(ErrType.Messages);
			}
			else if( ((int)typ >= 0) && ((int)typ < (int)ErrType.NUM))
			{
				err2proc.Add(typ);
			}
			
			bool hasMsg = false;
			foreach(ErrType t in err2proc)
			{
				_sb.Clear();
				int count = 0;
				foreach (ErrorMsg msg in Messages(t))
				{
					_sb.AppendLine(msg.ToString(details));
					count++;
				}
				if(errTitle && count > 0)
					_msg.AppendLine(t.ToString());
				if(count > 0)
				{
					_msg.AppendLine(_sb.ToString());
					hasMsg = true;
				}
			}
			if(!hasMsg)	_msg.AppendLine("No error messages");

			return _msg.ToString();
		}

	}
}
