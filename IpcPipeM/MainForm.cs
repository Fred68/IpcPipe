
using IpcMyData;
using IpcPipes;
using NcForms;
using static Fred68.CfgReader.CfgReader;

namespace IpcPipeM
{
	public partial class MainForm:NcForm
	{

		CFG cfg;                                        // File di configurazione
		static Form? formRef;                           // Riferimento statico a Form1
		bool errorOnLoad;                               // Errore durante OnLoad()

		IpcPipe.Info nfo;                               // Info per IpcPipe
		IpcPipe ipc;

		int idConn_to_slave = IpcPipe.ID_ERROR;         // Id della prima connessione
		int idComm_esegui = IpcPipe.ID_ERROR;

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

			nfo = new IpcPipe.Info(cfg.PIPE_out[0],
									cfg.PIPE_in[0],
									cfg.PIPE_master,
#warning Impostare il delay da configurazione
									100,
									IpcPipe.InstanceCheck.Unique
									);  // Path.GetRandomFileName().Replace(".","") 

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


		public void SegnalaStatCiclo(bool stat)
		{
			NcMessageBox.Show(this,"Ciclo " + (stat ? "abilitato" : "disabilitato"));
		}

		public void SegnalaFineCiclo()
		{
			NcMessageBox.Show(this,"Ciclo arrestato");
		}



		public bool Esegui(MyClass myClass)
		{
			bool ok = true;
			MessageBox.Show(myClass.ToString());
			return ok;
		}

		/// <summary>
		/// Crea la connessione
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btCreaPipe_Click(object sender,EventArgs e)
		{
			if(ipc != null)
			{
				ipc.ClearErrMessages();

				int tmp = ipc.CreatePipeConnection(nfo);            // Crea una connessione e memorizza l'ID

				if(tmp == IpcPipe.ID_ERROR)
				{
					NcMessageBox.Show(this,ipc.GetErrMessageString());
				}
				else
				{
					idConn_to_slave = tmp;
				}
				NcMessageBox.Show(this,ipc.ToString());
			}
		}

		/// <summary>
		/// Sincronizza la connessione con la controparte
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btConnette_Click(object sender,EventArgs e)
		{
			if(ipc != null)
			{
				ipc.ClearErrMessages();
				if(ipc.ConnectPipe(idConn_to_slave))
				{
					// NcMessageBox.Show(this,"Connessione alla pipe riuscita.");

					if(ipc.Sync(1))
					{
						NcMessageBox.Show(this,"Sync riuscita.");
					}
					else
					{
						NcMessageBox.Show(this,ipc.GetErrMessageString());
					}
				}
				else
				{
					NcMessageBox.Show(this,ipc.GetErrMessageString());
				}
			}
		}


		private void btCreaCmd_Click(object sender,EventArgs e)
		{
			idComm_esegui = ipc.CreateCommand<MyClass>(10,idConn_to_slave,Esegui,"TEST");
			string msg = ipc.ToString();
			NcMessageBox.Show(this,msg);
			return;
		}

		private void btTestSerializz_Click(object sender,EventArgs e)
		{
			MyClass dati = new MyClass(11.1,"PIPPO");
			string serializzato;
			if(!ipc.Serializza<MyClass>(dati,idComm_esegui,idConn_to_slave,out serializzato))
			{
				NcMessageBox.Show(this,"Errore serializzazione in stringa","ERROR");
			}
			else
			{
				string msg = $"MyClass:\n{dati.ToString()}\nSerializzazione:\n{serializzato}";
				NcMessageBox.Show(this,msg,"SERIALIZZAZIONE");
			}

			MyClass deserializzato;
			if(!ipc.Deserializza<MyClass>(serializzato,idConn_to_slave,out deserializzato))
			{
				NcMessageBox.Show(this,"Errore deserializzazione da stringa","ERROR");
			}
			else
			{
				string msg = $"Myclass:\n{deserializzato.ToString()}";
				NcMessageBox.Show(this,msg,"DESERIALIZZAZIONE");
			}


			return;
		}

		private void bt_InviaCmd_Click(object sender,EventArgs e)
		{
			MyClass dati = new MyClass(22.2,"PLUTO");
			if(!ipc.InviaDati<MyClass>(dati,idComm_esegui,idConn_to_slave))
			{
				NcMessageBox.Show(this,"Errore invio dati","ERROR");
			}
			else
			{
				string msg = $"MyClass:\n{dati.ToString()}\nInviato correttamente.";
				NcMessageBox.Show(this,msg,"INVIO DATI");
			}
		}

		private void btStartCycle_Click(object sender,EventArgs e)
		{
			ipc.AvviaCiclo();
		}
	}

}
