using IpcPipes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using IpcMyData;

namespace IpcPipeS
{
	public partial class SecondForm : Form
	{

		IpcPipe.Info nfo;
		IpcPipe ipc;

		int idConn_to_master = IpcPipe.ID_ERROR;
		int idComm_ricevi = IpcPipe.ID_ERROR;

		public SecondForm(CFGfw cfg)
		{
			InitializeComponent();

			nfo = new IpcPipe.Info(cfg.PIPE_out[0],cfg.PIPE_in[0],cfg.PIPE_master,1200,IpcPipe.InstanceCheck.KillOther);
			try
			{
				ipc = new IpcPipe(SegnalaStatCiclo,SegnalaFineCiclo);

				if(!ipc.CheckProcInstances(nfo.instanceCheck))
				{
					throw new Exception(ipc.GetLastErrMessage());
				}
			}
			catch(Exception ex)
			{
				MessageBox.Show(this,ex.Message);
				Close();
			}
		}

		public void SegnalaStatCiclo(bool stat)
		{
			MessageBox.Show("Ciclo " + (stat ? "abilitato" : "disabilitato"));
		}

		public void SegnalaFineCiclo()
		{
			MessageBox.Show("Ciclo arrestato");
		}

		private void btCreaPipe_Click(object sender,EventArgs e)
		{
			if(ipc!=null)
			{
				int tmp = ipc.CreatePipeConnection(nfo);

				if(tmp == IpcPipe.ID_ERROR)
				{
					MessageBox.Show(this,ipc.GetErrMessageString());
				}
				else
				{
					idConn_to_master = tmp;
				}
				MessageBox.Show(ipc.ToString());
			}
		}

		private void btConnette_Click(object sender,EventArgs e)
		{
			if(ipc!=null)
			{
				if(ipc.ConnectPipe(idConn_to_master))
				{
					// MessageBox.Show(this,"Connessione riuscita");

					if(ipc.Sync(1))
					{
						MessageBox.Show(this,"Sync riuscita");
					}
					else
					{
						MessageBox.Show(this,ipc.GetErrMessageString());
					}
				}
				else
				{
					MessageBox.Show(this,ipc.GetErrMessageString());
				}

			}
		}

		public bool Ricevi(MyClass myClass)
		{
			bool ok = true;
			MessageBox.Show(myClass.ToString());
			return ok;
		}

		private void btCreaCmd_Click(object sender,EventArgs e)
		{
			idComm_ricevi = ipc.CreateCommand<MyClass>(10,idConn_to_master,Ricevi,"TEST");
			string msg = ipc.ToString();
			MessageBox.Show(msg);
			return;
		}
	}
}
