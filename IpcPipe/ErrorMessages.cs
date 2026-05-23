using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorMessages
{
	public class ErrorMessages
	{
		
		public enum Type {Messages=0, Errors, NUM};
		
		public static readonly int N_TYPES = (int)Type.NUM-1;
		public static readonly string Separator = " - ";
		
		protected Stack<ErrorMsg>[] _msg;
		protected class ErrorMsg
		{
			public string Messaggio { get; set; }
			public string Dettaglio { get; set; }
			public ErrorMsg(string msg, string det)
			{
				Messaggio = msg;
				Dettaglio = det;
			}
			public string ToLine()
			{
				return Messaggio + ((Dettaglio.Length > 0) ? ErrorMessages.Separator : "") + Dettaglio + System.Environment.NewLine;
			}
		}

		/// <summary>
		/// CTOR
		/// </summary>
		public ErrorMessages()
		{
			_msg = new Stack<ErrorMsg>[N_TYPES];
			for(int i=0; i < (int)Type.NUM; i++)
			{
				_msg[i] = new Stack<ErrorMsg>();
			}
		}

		/// <summary>
		/// Add a message
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="dett"></param>
		/// <param name="typ"></param>
		public void AddMessage(string msg, string dett = "", Type typ = Type.Messages)
		{
			int i = (int)typ;
			if( (i>=0) && (i<(int)Type.NUM) )
			{
				_msg[i].Push(new ErrorMsg(msg, dett));
			}
		}

		/// <summary>
		/// Clear messages
		/// </summary>
		/// <param name="typ"></param>
		public void Clear(Type typ = Type.NUM)
		{
			int i = (int)typ;
			if (i == (int)Type.NUM)
			{
				foreach (Stack<ErrorMsg> lst in _msg)
					lst.Clear();
			}
			else if ((i >= 0) && (i < (int)Type.NUM))
				_msg[i].Clear();
		}

		/// <summary>
		/// Enumerator
		/// </summary>
		/// <param name="typ"></param>
		/// <returns></returns>
		protected IEnumerable<ErrorMsg> Messages(Type typ)
		{
			#warning AGGIUNGERE IL NUMERO TOTALE e ARG DI DEFAULT, vd. Clear()
			int i = (int)typ;
			if((i >= 0) && (i < (int)Type.NUM))
			{
				foreach (ErrorMsg str in _msg[i])
					yield return str;
			}
			yield break;
		}

		/// <summary>
		/// Get number of messages
		/// </summary>
		/// <param name="typ"></param>
		/// <returns></returns>
		public int Nmessages(Type typ)
		{
			#warning AGGIUNGERE IL NUMERO TOTALE e ARG DI DEFAULT, vd. Clear()
			int n = 0;
			int i = (int) typ;
			if ((i >= 0) && (i < (int)Type.NUM))
			{
				n = _msg[i].Count;
			}
			
			return n;
		}

		/// <summary>
		/// Has messages
		/// </summary>
		/// <param name="typ"></param>
		/// <returns></returns>
		public bool HasMessages(Type typ)
		{
			bool hasMsg = false;
			if (Nmessages(typ) > 0)
				hasMsg = true;
			return hasMsg;
		}

		public string ToString(ErrorMessages.Type typ)
		{
			#warning AGGIUNGERE IL NUMERO TOTALE e ARG DI DEFAULT, vd. Clear()
			StringBuilder strb = new StringBuilder();
			List<string> lm = new List<string>();

			foreach (ErrorMsg msg in Messages(typ))
				lm.Add(msg.ToLine());
			lm = lm.Distinct().ToList();

			foreach (string str in lm)
				strb.Append(str /*+ Environment.NewLine*/);
			return strb.ToString();
		}

		public string MessaggiCompleti()
		{
            StringBuilder strb = new StringBuilder();
			string s1, s2;
			s1 = ToString(ErrorMessages.Type.Errors);
			s2 = ToString(ErrorMessages.Type.Messages);
			if(s1.Length > 0)
				strb.Append("Errori"+ System.Environment.NewLine + s1+ Environment.NewLine);
			if(s2.Length > 0)
				strb.Append("Avvisi"+ System.Environment.NewLine + s2);
            return strb.ToString();
        }
	}
}
