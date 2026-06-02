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

namespace IpcPipeS
{
	public partial class SecondForm : Form
	{

		IpcPipe.Info nfo;
		IpcPipe ipc;

		public SecondForm(CFGfw cfg)
		{
			InitializeComponent();

			nfo = new IpcPipe.Info(cfg.PIPE_out[0],cfg.PIPE_in[0],cfg.PIPE_master,1200,IpcPipe.InstanceCheck.KillOther);
			try
			{
				ipc = new IpcPipe();
				if(!ipc.CheckInstances(nfo.instanceCheck))
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

		private void button1_Click(object sender,EventArgs e)
		{
			if(ipc!=null)
			{
				if(ipc.CreatePipeConnection(nfo) == IpcPipe.ID_ERROR)
				{
					MessageBox.Show(this,ipc.GetLastErrMessage());
				}
				MessageBox.Show(ipc.ToString());
			}
		}

		private void button2_Click(object sender,EventArgs e)
		{
			if(ipc!=null)
			{
				if(ipc.ConnectPipe(1))
				{
					MessageBox.Show(this,"Connessione riuscita");
				}
				else
				{
					MessageBox.Show(this,ipc.GetLastErrMessage());
				}

			}
		}
	}
}
