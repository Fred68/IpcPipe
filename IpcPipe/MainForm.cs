
using NcForms;

namespace IpcPipe
{
	public partial class MainForm:NcForm
	{

		CFG cfg;                            // File di configurazione
		static Form? formRef;               // Riferimento statico a Form1
		bool errorOnLoad;                   // Errore durante OnLoad()



		public MainForm()
		{
			InitializeComponent();
		}

		public MainForm(NcFormStyle style,NcFormColor color,NcFormMsg msgs,CFG cfg,string? path) : base(style,color,msgs)
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



		}

		private void MainForm_Load(object sender,EventArgs e)
		{
			Shown += MainForm_Shown;

			if(errorOnLoad)    // Se errore durante OnLoad()
			{
				NcMessageBox.Show(this,"[Form1 .NET] Errore durante OnLoad(). Fine programma.");
			}

		}

		private void MainForm_Shown(object sender,EventArgs e)
		{
			
		}
	}
}
