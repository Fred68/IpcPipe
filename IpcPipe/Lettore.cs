using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IpcPipes.IpcPipe;

/***********************************************/
// Non c'è ragione di creare una nuva classe
// Inglobare tutto in IpcPipe
/***********************************************/

namespace IpcPipes
{
	
	public delegate void DelegateElab(string linee);
	public delegate void DelegateBool(bool stat);
	public delegate void DelegateNull();
	public delegate IEnumerable<PipeConnection> DelegateIter();


	public class Lettore
	{
		static DelegateElab elaboraStringa;
		static DelegateBool segnalaCiclo;
		static DelegateNull dopoFineCiclo;
		static DelegateIter pipesIter;

		static bool _cicloAbilitato;
		



		public static bool CicloAbilitato
		{
			get
			{
				return _cicloAbilitato;
			}
			set
			{
				_cicloAbilitato = value;
				if (segnalaCiclo != null)
				{
					segnalaCiclo(_cicloAbilitato);
				}
			}
		}


		public Lettore(DelegateElab elaboratore, DelegateBool segnalatore, DelegateNull fineciclo, DelegateIter iter)
		{
			elaboraStringa = elaboratore;
			segnalaCiclo = segnalatore;
			dopoFineCiclo = fineciclo;
			pipesIter = iter;
		}


	}
}
