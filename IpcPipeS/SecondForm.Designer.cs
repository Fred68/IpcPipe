namespace IpcPipeS
{
	partial class SecondForm
	{
		/// <summary>
		/// Variabile di progettazione necessaria.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Pulire le risorse in uso.
		/// </summary>
		/// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
		protected override void Dispose(bool disposing)
		{
			if(disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Codice generato da Progettazione Windows Form

		/// <summary>
		/// Metodo necessario per il supporto della finestra di progettazione. Non modificare
		/// il contenuto del metodo con l'editor di codice.
		/// </summary>
		private void InitializeComponent()
		{
			this.btCreaPipe = new System.Windows.Forms.Button();
			this.btConnette = new System.Windows.Forms.Button();
			this.btCreaCmd = new System.Windows.Forms.Button();
			this.btStartCycle = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btCreaPipe
			// 
			this.btCreaPipe.Location = new System.Drawing.Point(59, 47);
			this.btCreaPipe.Name = "btCreaPipe";
			this.btCreaPipe.Size = new System.Drawing.Size(101, 23);
			this.btCreaPipe.TabIndex = 0;
			this.btCreaPipe.Text = "Crea Pipe";
			this.btCreaPipe.UseVisualStyleBackColor = true;
			this.btCreaPipe.Click += new System.EventHandler(this.btCreaPipe_Click);
			// 
			// btConnette
			// 
			this.btConnette.Location = new System.Drawing.Point(59, 77);
			this.btConnette.Name = "btConnette";
			this.btConnette.Size = new System.Drawing.Size(101, 23);
			this.btConnette.TabIndex = 1;
			this.btConnette.Text = "Connette";
			this.btConnette.UseVisualStyleBackColor = true;
			this.btConnette.Click += new System.EventHandler(this.btConnette_Click);
			// 
			// btCreaCmd
			// 
			this.btCreaCmd.Location = new System.Drawing.Point(59, 107);
			this.btCreaCmd.Name = "btCreaCmd";
			this.btCreaCmd.Size = new System.Drawing.Size(101, 23);
			this.btCreaCmd.TabIndex = 2;
			this.btCreaCmd.Text = "CreaCmd";
			this.btCreaCmd.UseVisualStyleBackColor = true;
			this.btCreaCmd.Click += new System.EventHandler(this.btCreaCmd_Click);
			// 
			// btStartCycle
			// 
			this.btStartCycle.Location = new System.Drawing.Point(59, 165);
			this.btStartCycle.Name = "btStartCycle";
			this.btStartCycle.Size = new System.Drawing.Size(101, 23);
			this.btStartCycle.TabIndex = 3;
			this.btStartCycle.Text = "Avvia ciclo";
			this.btStartCycle.UseVisualStyleBackColor = true;
			this.btStartCycle.Click += new System.EventHandler(this.btStartCycle_Click);
			// 
			// SecondForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(429, 232);
			this.Controls.Add(this.btStartCycle);
			this.Controls.Add(this.btCreaCmd);
			this.Controls.Add(this.btConnette);
			this.Controls.Add(this.btCreaPipe);
			this.Name = "SecondForm";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btCreaPipe;
		private System.Windows.Forms.Button btConnette;
		private System.Windows.Forms.Button btCreaCmd;
		private System.Windows.Forms.Button btStartCycle;
	}
}

