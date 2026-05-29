using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fred68.CfgReader;

namespace IpcPipeS
{


	#pragma warning disable CS8618	// Non nullable...
	public class CFGfw : CfgReader
	{

		// Sezione: Scambio dati
		public List<string> PIPE_out;
		public List<string> PIPE_in;
		public bool PIPE_master;

		
	}
}
