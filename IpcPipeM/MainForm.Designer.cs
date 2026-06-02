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
			button1 = new Button();
			button2 = new Button();
			SuspendLayout();
			// 
			// button1
			// 
			button1.Location = new Point(55,44);
			button1.Name = "button1";
			button1.Size = new Size(75,23);
			button1.TabIndex = 0;
			button1.Text = "CreaPipe";
			button1.UseVisualStyleBackColor = true;
			button1.Click += button1_Click;
			// 
			// button2
			// 
			button2.Location = new Point(60,72);
			button2.Name = "button2";
			button2.Size = new Size(75,23);
			button2.TabIndex = 3;
			button2.Text = "Connette [1]";
			button2.UseVisualStyleBackColor = true;
			button2.Click += button2_Click;
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(7F,15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(406,311);
			Controls.Add(button2);
			Controls.Add(button1);
			Name = "MainForm";
			Text = "Form1";
			Load += MainForm_Load;
			Shown += MainForm_Shown;
			Controls.SetChildIndex(button1,0);
			Controls.SetChildIndex(button2,0);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Button button1;
		private Button button2;
	}
}
