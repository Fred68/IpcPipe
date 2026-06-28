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
			btInviaCmd = new Button();
			SuspendLayout();
			// 
			// btCreaPipe
			// 
			btCreaPipe.Location = new Point(60,43);
			btCreaPipe.Name = "btCreaPipe";
			btCreaPipe.Size = new Size(75,23);
			btCreaPipe.TabIndex = 0;
			btCreaPipe.Text = "CreaPipe";
			btCreaPipe.UseVisualStyleBackColor = true;
			btCreaPipe.Click += btCreaPipe_Click;
			// 
			// btConnette
			// 
			btConnette.Location = new Point(60,72);
			btConnette.Name = "btConnette";
			btConnette.Size = new Size(75,23);
			btConnette.TabIndex = 3;
			btConnette.Text = "Connette [1]";
			btConnette.UseVisualStyleBackColor = true;
			btConnette.Click += btConnette_Click;
			// 
			// btCreaCmd
			// 
			btCreaCmd.Location = new Point(60,101);
			btCreaCmd.Name = "btCreaCmd";
			btCreaCmd.Size = new Size(75,23);
			btCreaCmd.TabIndex = 4;
			btCreaCmd.Text = "Crea cmd";
			btCreaCmd.UseVisualStyleBackColor = true;
			btCreaCmd.Click += btCreaCmd_Click;
			// 
			// btInviaCmd
			// 
			btInviaCmd.Location = new Point(60,130);
			btInviaCmd.Name = "btInviaCmd";
			btInviaCmd.Size = new Size(75,23);
			btInviaCmd.TabIndex = 5;
			btInviaCmd.Text = "Invia cmd";
			btInviaCmd.UseVisualStyleBackColor = true;
			btInviaCmd.Click += button4_Click;
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(7F,15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(406,311);
			Controls.Add(btInviaCmd);
			Controls.Add(btCreaCmd);
			Controls.Add(btConnette);
			Controls.Add(btCreaPipe);
			Name = "MainForm";
			Text = "Form1";
			Load += MainForm_Load;
			Shown += MainForm_Shown;
			Controls.SetChildIndex(btCreaPipe,0);
			Controls.SetChildIndex(btConnette,0);
			Controls.SetChildIndex(btCreaCmd,0);
			Controls.SetChildIndex(btInviaCmd,0);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Button btCreaPipe;
		private Button btConnette;
		private Button btCreaCmd;
		private Button btInviaCmd;
	}
}
