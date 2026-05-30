
using NcForms;
using IpcPipes;

namespace IpcPipeM
{
	public partial class MainForm:NcForm
	{

		CFG cfg;                            // File di configurazione
		static Form? formRef;               // Riferimento statico a Form1
		bool errorOnLoad;                   // Errore durante OnLoad()

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

			string wpp = Path.GetRandomFileName().Replace(".","");
			string rpp = Path.GetRandomFileName().Replace(".","");

			IpcPipe.Info nfo = new IpcPipe.Info(true,wpp,rpp,100,false);
			try
			{
				ipc = new IpcPipe(nfo);
				int inst = ipc.CountKillInstances(nfo.killInstances);
				if(inst != 1)
				{
					throw new Exception("Ammessa solo un'istanza del processo");
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
	}
}
