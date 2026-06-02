
using NcForms;
using IpcPipes;

namespace IpcPipeM
{
	public partial class MainForm:NcForm
	{

		CFG cfg;                            // File di configurazione
		static Form? formRef;               // Riferimento statico a Form1
		bool errorOnLoad;                   // Errore durante OnLoad()

		IpcPipe.Info nfo;                    // Info per IpcPipe
		IpcPipe ipc;

		public MainForm(NcFormStyle style,NcFormColor color,NcFormMsg msgs,CFG cfg) : base(style,color,msgs)
		{
			InitializeComponent();          // Richiesto da Form Designer

			formRef = this;
			errorOnLoad = false;

			this.cfg = cfg;
			this.AskClose = false;
			this.Opacity = cfg.Opacity;
			this.MinWidth = cfg.MinWidth;
			this.Title = cfg.Titolo;
			this.StatusText = string.Empty;
			this.ShowInTaskbar = cfg.ShowInTaskbar;
			this.Name = cfg.Titolo;
			this.Text = cfg.Titolo;

			nfo = new IpcPipe.Info(cfg.PIPE_out[0],cfg.PIPE_in[0],cfg.PIPE_master,100,IpcPipe.InstanceCheck.Unique);
			// I nomi delle pipe erano: Path.GetRandomFileName().Replace(".","");

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
				NcMessageBox.Show(this,ex.Message);
				Close();
			}
		}

		private void MainForm_Load(object sender,EventArgs e)
		{
			if(errorOnLoad)    // Se errore durante OnLoad()
			{
				NcMessageBox.Show(this,"[Form1 .NET] Errore durante OnLoad(). Fine programma.");
			}

		}

		private void MainForm_Shown(object sender,EventArgs e)
		{
			int x = 1;
		}

		private void button1_Click(object sender,EventArgs e)
		{
			if(ipc != null)
			{
				if(ipc.CreatePipeConnection(nfo) == IpcPipe.ID_ERROR)
				{
					NcMessageBox.Show(this,ipc.GetLastErrMessage());
				}
				NcMessageBox.Show(this,ipc.ToString());
			}
		}

		private void button2_Click(object sender,EventArgs e)
		{
			if(ipc != null)
			{
				if(ipc.ConnectPipe(1))
				{
					NcMessageBox.Show(this,"Connessione alla pipe riuscita.");

					if(ipc.Sync(1))
					{
						NcMessageBox.Show(this,"Sync riuscita.");
					}
					else
					{
						NcMessageBox.Show(this,ipc.GetLastErrMessage());
					}
				}
				else
				{
					NcMessageBox.Show(this,ipc.GetLastErrMessage());
				}
			}
		}
	}
}
