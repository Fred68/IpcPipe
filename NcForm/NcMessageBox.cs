//using NcForm;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NcForms
{

	//#warning  while(exitFlag == false) // Processes all the events in the queue Application.DoEvents();

	public class NcMessageBox:NcForms.NcForm
	{

		#region Private members
		const string STRING_EMPTY = "";
		const float MB_OPACITY = 1.0f;

		private System.Windows.Forms.Button button1;

		private System.Windows.Forms.Button[] bts;
		private RichTextBox richTextBox1;

		System.Windows.Forms.Timer? timer1 = null;

		#endregion

		/// <summary>
		/// Static object for the static funcion: NcMessageBox.Show(...)
		/// Alternative: ThreadLocal<NcMessageBox> _mb;
		/// </summary>
		[ThreadStatic]
		static NcMessageBox? _mb;
		static NcFormMsg? _mcfm;
		/// <summary>
		/// Show dialog
		/// </summary>
		/// <param name="ncf">Parent NcForm to copy colors from</param>
		/// <param name="text">Content of the message</param>
		/// <param name="caption">Caption on the title bar</param>
		/// <param name="buttons">System.Windows.Forms.MessageBoxButtons argument or null</param>
		/// <param name="timerDelay">Delay to close dialog</param>
		/// <returns></returns>
		public static DialogResult Show(NcForm? ncf, string text,string caption = STRING_EMPTY,MessageBoxButtons buttons = MessageBoxButtons.OK,int timerDelay = 0)
		{
			DialogResult res = DialogResult.Cancel;
			Font barF = (ncf != null) ? ncf.NcStyle.barsFont : SystemFonts.DefaultFont;
			int hgt = (ncf != null) ? (int)barF.Size : SystemFonts.DefaultFont.Height;
			NcFormStyle ncFs = new NcFormStyle(NcWindowsStyles.None,NcFormWindowStates.Normal,barF,hgt);     // NcWindowsStyles.None: no lower bar
			NcFormColor ncFc = (ncf != null) ? new NcFormColor(ncf.BackColor,ncf.TitleColor,ncf.StatusBarColor,ncf.ButtonsColor,MB_OPACITY) : NcFormColor.Normal;
			NcFormMsg ncMsg = (ncf != null) ? ncf.NcMsg : new NcFormMsg();
			using(_mb = new NcMessageBox(ncFs,ncFc,ncMsg,buttons,timerDelay))
			{
				_mb.richTextBox1.BackColor = ncFc.backColor;
				_mb.ButtonsColor = ncFc.buttonsColor;
				_mb.SetText(text,caption);
				res = _mb.ShowDialog();
			}
			return res;
		}

		#region Private functions
		private NcMessageBox(NcFormStyle style,NcFormColor color,NcFormMsg msgs,MessageBoxButtons buttons = MessageBoxButtons.OK,int tmDelay = 0) : base(style,color,msgs)
		{
			InitializeComponent();
			SetUpButtons((tmDelay>0) ? null : buttons);
			AdjustIfNoLowerBar();
			if(tmDelay > 0)
			{
				timer1 = new System.Windows.Forms.Timer();
				timer1.Interval = tmDelay;
				timer1.Tick += Timer1_Tick;
				timer1.Start();
			}

			Invalidate();
		}
		private NcMessageBox() : this(NcFormStyle.Fixed,NcFormColor.Normal,new NcFormMsg()) { }
		private NcMessageBox(NcForms.NcForm ncf) : this(ncf.NcStyle,ncf.NcColor,ncf.NcMsg) { }
		private NcMessageBox(NcForm ncf,string text,string caption = STRING_EMPTY,MessageBoxButtons buttons = MessageBoxButtons.OK) :
					this(ncf.NcStyle,ncf.NcColor,new NcFormMsg(), buttons)
		{
			BackColor = ncf.BackColor;
			SetText(text,caption);
		}
		private void InitializeComponent()
		{
			button1 = new System.Windows.Forms.Button();
			richTextBox1 = new RichTextBox();
			SuspendLayout();
			// 
			// button1
			// 
			button1.Location = new Point(408,384);
			button1.Name = "button1";
			button1.Size = new Size(75,40);
			button1.TabIndex = 3;
			button1.Text = "button1";
			button1.UseVisualStyleBackColor = true;
			// 
			// richTextBox1
			// 
			richTextBox1.BorderStyle = BorderStyle.None;
			richTextBox1.Dock = DockStyle.Top;
			richTextBox1.Location = new Point(0,25);
			richTextBox1.Name = "richTextBox1";
			richTextBox1.ReadOnly = true;
			richTextBox1.Size = new Size(483,353);
			richTextBox1.TabIndex = 5;
			richTextBox1.Text = "";
			// 
			// NcMessageBox
			// 
			ClientSize = new Size(483,452);
			Controls.Add(richTextBox1);
			Controls.Add(button1);
			Name = "NcMessageBox";
			FormClosing += NcMessageBox_FormClosing;
			Shown += NcMessageBox_Shown;
			Controls.SetChildIndex(button1,0);
			Controls.SetChildIndex(richTextBox1,0);
			ResumeLayout(false);
			PerformLayout();
		}

		private void AdjustIfNoLowerBar()
		{
			if((this.NcStyle.ncWindowsStyle & NcWindowsStyles.LowerBar) == 0)   // No lower bar
			{
				SuspendLayout();
				int i = 0;
				foreach(System.Windows.Forms.Button b in bts)
				{
					b.Location = new Point(b.Location.X,b.Location.Y + LowerBarHeight);
					i++;
				}
				richTextBox1.Size = new Size(richTextBox1.Size.Width,richTextBox1.Size.Height + LowerBarHeight);
				ResumeLayout(false);
				PerformLayout();
			}

		}
		private void SetUpButtons(MessageBoxButtons? buttons)
		{
			int nbuttons = 1;

			if(buttons != null)
			{
				switch(buttons)
				{
					case MessageBoxButtons.OK:
					{
						nbuttons = 1;
					}
					break;

					case MessageBoxButtons.OKCancel:
					case MessageBoxButtons.YesNo:
					case MessageBoxButtons.RetryCancel:
					{
						nbuttons = 2;
					}
					break;

					case MessageBoxButtons.YesNoCancel:
					case MessageBoxButtons.AbortRetryIgnore:
					case MessageBoxButtons.CancelTryContinue:
					{
						nbuttons = 3;
					}
					break;

					default:
						break;
				}
			}

			if(nbuttons > 0)
			{
				bts = new System.Windows.Forms.Button[nbuttons];
			}
			Size sz = button1.Size;
			Point lc = button1.Location;
			if(this.Width > lc.X + sz.Width)        // Calculate as right aligned
			{
				lc.X += this.Width - (lc.X + sz.Width);
			}

			button1.Visible = false;
			Controls.Remove(button1 as System.Windows.Forms.Button);

			for(int i = 0;i < nbuttons;i++)
			{
				bts[i] = new System.Windows.Forms.Button();
				bts[i].Size = sz;
				bts[i].Location = new Point(lc.X - sz.Width * i,lc.Y);
				bts[i].Visible = true;
				bts[i].BackColor = this.ButtonsColor;
				Controls.Add(bts[i]);
			}

			if(buttons != null)
			{
				switch(buttons)
				{
					case MessageBoxButtons.OK:
					{
						bts[0].DialogResult = DialogResult.OK;
						bts[0].Text = "OK";
					}
					break;
					case MessageBoxButtons.YesNo:
					{
						bts[0].DialogResult = DialogResult.No;
						bts[0].Text = "No";
						bts[1].DialogResult = DialogResult.Yes;
						bts[1].Text = "Yes";

					}
					break;
					case MessageBoxButtons.YesNoCancel:
					{
						bts[0].DialogResult = DialogResult.Cancel;
						bts[0].Text = "Cancel";
						bts[1].DialogResult = DialogResult.No;
						bts[1].Text = "No";
						bts[2].DialogResult = DialogResult.Yes;
						bts[2].Text = "Yes";
					}
					break;
					case MessageBoxButtons.AbortRetryIgnore:
					{
						bts[0].DialogResult = DialogResult.Ignore;
						bts[0].Text = "Ignore";
						bts[1].DialogResult = DialogResult.Retry;
						bts[1].Text = "Retry";
						bts[2].DialogResult = DialogResult.Abort;
						bts[2].Text = "Abort";
					}
					break;
					case MessageBoxButtons.RetryCancel:
					{
						bts[0].DialogResult = DialogResult.Cancel;
						bts[0].Text = "Cancel";
						bts[1].DialogResult = DialogResult.Retry;
						bts[1].Text = "Retry";
					}
					break;
					case MessageBoxButtons.CancelTryContinue:
					{
						bts[0].DialogResult = DialogResult.Continue;
						bts[0].Text = "Continue";
						bts[1].DialogResult = DialogResult.TryAgain;
						bts[1].Text = "TryAgain";
						bts[2].DialogResult = DialogResult.Cancel;
						bts[2].Text = "Cancel";
					}
					break;
					case MessageBoxButtons.OKCancel:
					{
						bts[0].DialogResult = DialogResult.Cancel;
						bts[0].Text = "Cancel";
						bts[1].DialogResult = DialogResult.OK;
						bts[1].Text = "OK";
					}
					break;
				}
				
			}
			else
			{
				bts[0].Visible = false;
			}
			Invalidate();
		}
		private void SetText(string text,string caption = "Help",string statusText = STRING_EMPTY)
		{
			richTextBox1.Text = text;
			Title = caption;
			if(statusText != STRING_EMPTY)
			{
				StatusText = statusText;
			}
			TopMost = true;
			AskClose = false;
		}
		private void NcMessageBox_Shown(object sender,EventArgs e)
		{
			richTextBox1.DeselectAll();
			bts[bts.Length - 1].Select();

			Size sz = GetAdjRtBoxOffset();

#if false
			MessageBox.Show($"Delta size= {sz}");
#endif

			if((sz.Width != 0) || (sz.Height != 0))
			{
				richTextBox1.Size += sz;                            // Resize richTextBox
				foreach(System.Windows.Forms.Button b in bts)       // Move buttons
				{
					b.Location += sz;
				}
				Size += sz;                                         // Resize Form
			}
			this.CenterToScreen();
		}
		private Size MeasureRtBoxText(RichTextBox rtb)
		{
			Size txtSz = new Size();
			Font fnt = rtb.Font;
			txtSz = TextRenderer.MeasureText(rtb.Text,fnt,txtSz,TextFormatFlags.ExpandTabs);
			return txtSz;
		}
		private Size GetAdjRtBoxOffset()
		{
			int dW, dH;
			dW = dH = 0;

			Size txtSz = MeasureRtBoxText(richTextBox1 as RichTextBox);
			Size rtBoxSz = richTextBox1.Size;
			Size btnSz = GetButtonsSize();
#if false
			MessageBox.Show($"RichTextBox size= {rtBoxSz}\n\rText size= {txtSz}\n\rButtons size= {btnSz}");
#endif

			if(rtBoxSz.Width > int.Max(txtSz.Width,btnSz.Width))
			{
				dW = int.Max(txtSz.Width,btnSz.Width) - rtBoxSz.Width;
			}

			if(txtSz.Height < rtBoxSz.Height)
			{
				dH = txtSz.Height - rtBoxSz.Height;
			}

			return new Size(dW,dH);
		}
		private Size GetButtonsSize()
		{
			int xmin, xmax, ymin, ymax;
			xmin = ymin = int.MaxValue;
			xmax = ymax = int.MinValue;
			foreach(System.Windows.Forms.Button b in bts)
			{
				if(b.Left < xmin)
					xmin = b.Left;
				if(b.Top < ymin)
					ymin = b.Top;
				if(b.Right > xmax)
					xmax = b.Right;
				if(b.Bottom > ymax)
					ymax = b.Bottom;
			}
			return new Size(xmax - xmin,ymax - ymin);
		}

		private void NcMessageBox_FormClosing(object sender,FormClosingEventArgs e)
		{
			if(timer1 != null)
			{
				timer1.Stop();
				//#if DEBUG
				//MessageBox.Show("Timer stopped");
				//#endif

			}
			#if false
			MessageBox.Show("Closing dialog");
			#endif

		}

		private /*static*/ void Timer1_Tick(object? sender,EventArgs e)
		{
			//#if DEBUG
			//MessageBox.Show("Timer event");
			//#endif
			Close();
		}


		#endregion

		
	}

}
