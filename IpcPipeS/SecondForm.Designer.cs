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
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.connessioneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.listaErroriToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.cancellaErroriToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// btCreaPipe
			// 
			this.btCreaPipe.Location = new System.Drawing.Point(79, 58);
			this.btCreaPipe.Margin = new System.Windows.Forms.Padding(4);
			this.btCreaPipe.Name = "btCreaPipe";
			this.btCreaPipe.Size = new System.Drawing.Size(135, 28);
			this.btCreaPipe.TabIndex = 0;
			this.btCreaPipe.Text = "Crea Pipe";
			this.btCreaPipe.UseVisualStyleBackColor = true;
			this.btCreaPipe.Click += new System.EventHandler(this.btCreaPipe_Click);
			// 
			// btConnette
			// 
			this.btConnette.Location = new System.Drawing.Point(79, 95);
			this.btConnette.Margin = new System.Windows.Forms.Padding(4);
			this.btConnette.Name = "btConnette";
			this.btConnette.Size = new System.Drawing.Size(135, 28);
			this.btConnette.TabIndex = 1;
			this.btConnette.Text = "Connette (1)";
			this.btConnette.UseVisualStyleBackColor = true;
			this.btConnette.Click += new System.EventHandler(this.btConnette_Click);
			// 
			// btCreaCmd
			// 
			this.btCreaCmd.Location = new System.Drawing.Point(79, 132);
			this.btCreaCmd.Margin = new System.Windows.Forms.Padding(4);
			this.btCreaCmd.Name = "btCreaCmd";
			this.btCreaCmd.Size = new System.Drawing.Size(135, 28);
			this.btCreaCmd.TabIndex = 2;
			this.btCreaCmd.Text = "CreaCmd";
			this.btCreaCmd.UseVisualStyleBackColor = true;
			this.btCreaCmd.Click += new System.EventHandler(this.btCreaCmd_Click);
			// 
			// btStartCycle
			// 
			this.btStartCycle.Location = new System.Drawing.Point(79, 168);
			this.btStartCycle.Margin = new System.Windows.Forms.Padding(4);
			this.btStartCycle.Name = "btStartCycle";
			this.btStartCycle.Size = new System.Drawing.Size(135, 28);
			this.btStartCycle.TabIndex = 3;
			this.btStartCycle.Text = "Avvia ciclo";
			this.btStartCycle.UseVisualStyleBackColor = true;
			this.btStartCycle.Click += new System.EventHandler(this.btStartCycle_Click);
			// 
			// menuStrip1
			// 
			this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connessioneToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(572, 28);
			this.menuStrip1.TabIndex = 4;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// connessioneToolStripMenuItem
			// 
			this.connessioneToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.listaErroriToolStripMenuItem,
            this.cancellaErroriToolStripMenuItem});
			this.connessioneToolStripMenuItem.Name = "connessioneToolStripMenuItem";
			this.connessioneToolStripMenuItem.Size = new System.Drawing.Size(106, 24);
			this.connessioneToolStripMenuItem.Text = "Connessione";
			// 
			// listaErroriToolStripMenuItem
			// 
			this.listaErroriToolStripMenuItem.Name = "listaErroriToolStripMenuItem";
			this.listaErroriToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
			this.listaErroriToolStripMenuItem.Text = "Lista errori";
			this.listaErroriToolStripMenuItem.Click += new System.EventHandler(this.listaErroriToolStripMenuItem_Click);
			// 
			// cancellaErroriToolStripMenuItem
			// 
			this.cancellaErroriToolStripMenuItem.Name = "cancellaErroriToolStripMenuItem";
			this.cancellaErroriToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
			this.cancellaErroriToolStripMenuItem.Text = "Cancella errori";
			this.cancellaErroriToolStripMenuItem.Click += new System.EventHandler(this.cancellaErroriToolStripMenuItem_Click);
			// 
			// SecondForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(572, 286);
			this.Controls.Add(this.btStartCycle);
			this.Controls.Add(this.btCreaCmd);
			this.Controls.Add(this.btConnette);
			this.Controls.Add(this.btCreaPipe);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "SecondForm";
			this.Text = "Form1";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btCreaPipe;
		private System.Windows.Forms.Button btConnette;
		private System.Windows.Forms.Button btCreaCmd;
		private System.Windows.Forms.Button btStartCycle;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem connessioneToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem listaErroriToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem cancellaErroriToolStripMenuItem;
	}
}

