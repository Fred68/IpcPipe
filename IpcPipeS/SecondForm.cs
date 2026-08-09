using Fred68.GenDictionary;
using IpcMyData;
using IpcPipes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace IpcPipeS
{
	public partial class SecondForm : Form
	{

		IpcPipe.Info nfo;
		IpcPipe ipc;

		int idConn_to_master = IpcPipe.ID_ERROR;

		int idComm_ricevi = IpcPipe.ID_ERROR;
		int idComm_rispondi = IpcPipe.ID_ERROR;

		public SecondForm(CFGfw cfg)
		{
			InitializeComponent();

			nfo = new IpcPipe.Info(cfg.PIPE_out[0],cfg.PIPE_in[0],cfg.PIPE_master,cfg.PIPE_delay,IpcPipe.InstanceCheck.KillOther);
			try
			{
				ipc = new IpcPipe(new IpcPipe.CycleDelegates(SegnalaStatCiclo,SegnalaFineCiclo));

				ipc.RegisterTextMsgHandler(SegnalaMessaggioDiTesto);

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

		public void SegnalaMessaggioDiTesto(string msg)
		{
			this.BeginInvoke(new Action(() => MessageBox.Show(this, msg)));
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

		

		private void btCreaCmd_Click(object sender,EventArgs e)
		{
			idComm_ricevi   = ipc.CreateCommand<MyClass>(10,idConn_to_master,Ricevi,"Riceve e risponde");
			idComm_rispondi = ipc.CreateCommand<MyClass>(15,idConn_to_master,Vuoto,"Risponde");
			string msg = ipc.ToString();
			MessageBox.Show(msg);
			return;
		}

		private void btStartCycle_Click(object sender,EventArgs e)
		{
			ipc.StartCycle();
		}

		#region Handler dei comandi

		public bool Vuoto(MyClass myClass) {return true;}

		public bool Ricevi(MyClass myClass)
		{
			bool ok = true;
			MessageBox.Show("SecondForm::Ricevi:\n"+myClass.ToString());
			
			myClass.X = myClass.X + 1;
			myClass.Str = myClass.Str + "->RICEVUTA";

			if(!ipc.SendCommand<MyClass>(myClass,idComm_rispondi,idConn_to_master))
			{
				MessageBox.Show(this,"Errore invio dati","ERROR");
			}

			return ok;
		}



		#endregion

	}
}
