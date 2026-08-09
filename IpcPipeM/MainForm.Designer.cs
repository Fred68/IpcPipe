namespace IpcPipeM
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			btCreaPipe = new Button();
			btConnette = new Button();
			btCreaCmd = new Button();
			bt_InviaCmd = new Button();
			btStartCycle = new Button();
			menuStrip1 = new MenuStrip();
			fileToolStripMenuItem = new ToolStripMenuItem();
			listaErroriToolStripMenuItem = new ToolStripMenuItem();
			cancellaErroriToolStripMenuItem = new ToolStripMenuItem();
			button1 = new Button();
			btPing = new Button();
			menuStrip1.SuspendLayout();
			SuspendLayout();
			// 
			// btCreaPipe
			// 
			btCreaPipe.Location = new Point(69,68);
			btCreaPipe.Margin = new Padding(3,4,3,4);
			btCreaPipe.Name = "btCreaPipe";
			btCreaPipe.Size = new Size(110,31);
			btCreaPipe.TabIndex = 0;
			btCreaPipe.Text = "CreaPipe";
			btCreaPipe.UseVisualStyleBackColor = true;
			btCreaPipe.Click += btCreaPipe_Click;
			// 
			// btConnette
			// 
			btConnette.Location = new Point(69,113);
			btConnette.Margin = new Padding(3,4,3,4);
			btConnette.Name = "btConnette";
			btConnette.Size = new Size(110,31);
			btConnette.TabIndex = 3;
			btConnette.Text = "Connette [1]";
			btConnette.UseVisualStyleBackColor = true;
			btConnette.Click += btConnette_Click;
			// 
			// btCreaCmd
			// 
			btCreaCmd.Location = new Point(69,152);
			btCreaCmd.Margin = new Padding(3,4,3,4);
			btCreaCmd.Name = "btCreaCmd";
			btCreaCmd.Size = new Size(110,31);
			btCreaCmd.TabIndex = 4;
			btCreaCmd.Text = "Crea cmd";
			btCreaCmd.UseVisualStyleBackColor = true;
			btCreaCmd.Click += btCreaCmd_Click;
			// 
			// bt_InviaCmd
			// 
			bt_InviaCmd.Location = new Point(254,191);
			bt_InviaCmd.Margin = new Padding(3,4,3,4);
			bt_InviaCmd.Name = "bt_InviaCmd";
			bt_InviaCmd.Size = new Size(110,31);
			bt_InviaCmd.TabIndex = 6;
			bt_InviaCmd.Text = "Invia Cmd";
			bt_InviaCmd.UseVisualStyleBackColor = true;
			bt_InviaCmd.Click += bt_InviaCmd_Click;
			// 
			// btStartCycle
			// 
			btStartCycle.Location = new Point(69,191);
			btStartCycle.Margin = new Padding(3,4,3,4);
			btStartCycle.Name = "btStartCycle";
			btStartCycle.Size = new Size(110,31);
			btStartCycle.TabIndex = 7;
			btStartCycle.Text = "Avvia ciclo";
			btStartCycle.UseVisualStyleBackColor = true;
			btStartCycle.Click += btStartCycle_Click;
			// 
			// menuStrip1
			// 
			menuStrip1.ImageScalingSize = new Size(20,20);
			menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
			menuStrip1.Location = new Point(0,36);
			menuStrip1.Name = "menuStrip1";
			menuStrip1.Size = new Size(464,28);
			menuStrip1.TabIndex = 8;
			menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { listaErroriToolStripMenuItem,cancellaErroriToolStripMenuItem });
			fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			fileToolStripMenuItem.Size = new Size(106,24);
			fileToolStripMenuItem.Text = "Connessione";
			// 
			// listaErroriToolStripMenuItem
			// 
			listaErroriToolStripMenuItem.Name = "listaErroriToolStripMenuItem";
			listaErroriToolStripMenuItem.Size = new Size(188,26);
			listaErroriToolStripMenuItem.Text = "Lista errori";
			listaErroriToolStripMenuItem.Click += listaErroriToolStripMenuItem_Click;
			// 
			// cancellaErroriToolStripMenuItem
			// 
			cancellaErroriToolStripMenuItem.Name = "cancellaErroriToolStripMenuItem";
			cancellaErroriToolStripMenuItem.Size = new Size(188,26);
			cancellaErroriToolStripMenuItem.Text = "Cancella errori";
			cancellaErroriToolStripMenuItem.Click += cancellaErroriToolStripMenuItem_Click;
			// 
			// button1
			// 
			button1.Location = new Point(221,314);
			button1.Name = "button1";
			button1.Size = new Size(8,8);
			button1.TabIndex = 9;
			button1.Text = "button1";
			button1.UseVisualStyleBackColor = true;
			// 
			// btPing
			// 
			btPing.Location = new Point(69,230);
			btPing.Margin = new Padding(3,4,3,4);
			btPing.Name = "btPing";
			btPing.Size = new Size(110,31);
			btPing.TabIndex = 10;
			btPing.Text = "Ping (1)";
			btPing.UseVisualStyleBackColor = true;
			btPing.Click += btPing_Click;
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(8F,20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(464,415);
			Controls.Add(btPing);
			Controls.Add(button1);
			Controls.Add(btStartCycle);
			Controls.Add(bt_InviaCmd);
			Controls.Add(btCreaCmd);
			Controls.Add(btConnette);
			Controls.Add(btCreaPipe);
			Controls.Add(menuStrip1);
			MainMenuStrip = menuStrip1;
			Margin = new Padding(3,4,3,4);
			Name = "MainForm";
			Text = "Form1";
			Load += MainForm_Load;
			Shown += MainForm_Shown;
			Controls.SetChildIndex(menuStrip1,0);
			Controls.SetChildIndex(btCreaPipe,0);
			Controls.SetChildIndex(btConnette,0);
			Controls.SetChildIndex(btCreaCmd,0);
			Controls.SetChildIndex(bt_InviaCmd,0);
			Controls.SetChildIndex(btStartCycle,0);
			Controls.SetChildIndex(button1,0);
			Controls.SetChildIndex(btPing,0);
			menuStrip1.ResumeLayout(false);
			menuStrip1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Button btCreaPipe;
		private Button btConnette;
		private Button btCreaCmd;
		private Button bt_InviaCmd;
		private Button btStartCycle;
		private MenuStrip menuStrip1;
		private ToolStripMenuItem fileToolStripMenuItem;
		private ToolStripMenuItem listaErroriToolStripMenuItem;
		private ToolStripMenuItem cancellaErroriToolStripMenuItem;
		private Button button1;
		private Button btPing;
	}
}
