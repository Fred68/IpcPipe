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
			btCreaPipe.Location = new Point(69,57);
			btCreaPipe.Margin = new Padding(3,4,3,4);
			btCreaPipe.Name = "btCreaPipe";
			btCreaPipe.Size = new Size(86,31);
			btCreaPipe.TabIndex = 0;
			btCreaPipe.Text = "CreaPipe";
			btCreaPipe.UseVisualStyleBackColor = true;
			btCreaPipe.Click += btCreaPipe_Click;
			// 
			// btConnette
			// 
			btConnette.Location = new Point(69,96);
			btConnette.Margin = new Padding(3,4,3,4);
			btConnette.Name = "btConnette";
			btConnette.Size = new Size(86,31);
			btConnette.TabIndex = 3;
			btConnette.Text = "Connette [1]";
			btConnette.UseVisualStyleBackColor = true;
			btConnette.Click += btConnette_Click;
			// 
			// btCreaCmd
			// 
			btCreaCmd.Location = new Point(69,135);
			btCreaCmd.Margin = new Padding(3,4,3,4);
			btCreaCmd.Name = "btCreaCmd";
			btCreaCmd.Size = new Size(86,31);
			btCreaCmd.TabIndex = 4;
			btCreaCmd.Text = "Crea cmd";
			btCreaCmd.UseVisualStyleBackColor = true;
			btCreaCmd.Click += btCreaCmd_Click;
			// 
			// btInviaCmd
			// 
			btInviaCmd.Location = new Point(69,247);
			btInviaCmd.Margin = new Padding(3,4,3,4);
			btInviaCmd.Name = "btInviaCmd";
			btInviaCmd.Size = new Size(86,31);
			btInviaCmd.TabIndex = 5;
			btInviaCmd.Text = "Invia cmd";
			btInviaCmd.UseVisualStyleBackColor = true;
			btInviaCmd.Click += button4_Click;
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(8F,20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(464,415);
			Controls.Add(btInviaCmd);
			Controls.Add(btCreaCmd);
			Controls.Add(btConnette);
			Controls.Add(btCreaPipe);
			Margin = new Padding(3,4,3,4);
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
