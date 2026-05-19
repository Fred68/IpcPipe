using NcForms;
using System.ComponentModel;

using System.Drawing;
using System.Reflection;
using System.Text;


namespace NcForms
{
#pragma warning disable CS8618
#pragma warning disable CS8622
#pragma warning disable CS8600
#pragma warning disable CS8603
    /// <summary>
    /// Non Client Area Window base class
    /// </summary>
    public class NcForm : Form
	{
		/************************/
		// Private members
		/************************/
		#region Private members

		public const int tsItemExtraWidth = 3;
		public const int contentMinHeight = 10;

		//private System.ComponentModel.IContainer components;
		private ToolStripDropDownButton tsMenu;
		private ToolStripButton tsQuit;
		private ToolStripButton tsHelp;
		private ToolStripMenuItem tsmiHelp;
		private ToolStripMenuItem tsmiQuit;
		private ToolStripMenuItem settingsToolStripMenuItem;
		private ToolStripLabel tsTitle;
		private ToolStrip tsUpper;
		private ToolStrip tsLower;
		private ToolStripLabel statLabel;
		private ToolStripLabel reszLabel;

		NcFormStyle ncStyle;
		NcFormColor ncColor;
		NcFormMsg ncMsg;

		Point startDrag, startCur, startResz;
		DateTime lastMouseDownEventTime;
		TimeSpan doubleClickDelay;			// Timespan identifying a double click on upper bar
		Size startSz, prevSz;
		bool dragging;
		bool resizing;
		float opacity;

		Size minTitleSz;
		bool showTsHelp, showTsMenu, showTsMaxMin, showTsBar, showTsTop;
		Color colorTitle, colorStatusBar;
		Color colorButtons;
		int availWidthUpper, availWidthLower;

		private ToolStripButton tsMin;
		private ToolStripButton tsMax;
		private ToolStripButton tsBar;
		private ToolStripButton tsTop;
		
		private List<string> baseFormControlNames;

		bool hasMenu;
		bool hasMinMax;
		bool hasHelp;
		bool isResizable;
		bool hasTopMost;
		bool hasLowerBar;

		bool askClose, closing, loading;
		int minWidth;

		NcFormWindowStates ncWindowsState, prevNcWindowsState;
		//private System.ComponentModel.IContainer components;

		#endregion
		/************************/

		/************************/
		// Constructors
		/************************/
		/// <summary>
		/// Empty constructor
		/// </summary>
		public NcForm() : this(NcFormStyle.Normal,NcFormColor.Normal,new NcFormMsg()) { }
		/// <summary>
		/// Constructor with same style and color
		/// </summary>
		/// <param name="ncf"></param>
		public NcForm(NcForm ncf) : this(ncf.ncStyle,ncf.ncColor,ncf.ncMsg) { }
		/// <summary>
		/// Constructor with params.
		/// NcForm potentially null fields are checked by a try...catch
		/// </summary>
		/// <param name="style"></param>		
		public NcForm(NcFormStyle style,NcFormColor color,NcFormMsg msgs)
		{
			ncStyle = style;
			ncColor = color;
			ncMsg = msgs;
			askClose = closing = false;
			loading = true;
			hasMenu = (ncStyle.ncWindowsStyle & NcWindowsStyles.Menu) != 0;
			hasMinMax = (ncStyle.ncWindowsStyle & NcWindowsStyles.MinMax) != 0;
			hasHelp = (ncStyle.ncWindowsStyle & NcWindowsStyles.Help) != 0;
			isResizable = (ncStyle.ncWindowsStyle & NcWindowsStyles.Resizable) != 0;
			hasTopMost = (ncStyle.ncWindowsStyle & NcWindowsStyles.TopMost) != 0;
			hasLowerBar = ((ncStyle.ncWindowsStyle & NcWindowsStyles.LowerBar) != 0) || isResizable;
			
			try
			{
				InitializeComponent();
			}
			catch(Exception ex)
			{
				MessageBox.Show($"{ncMsg.ERROR} {ex.Message}",ncMsg.ERROR);
				Environment.Exit(0);
			}
			dragging = resizing = false;
			opacity = 0.7f;
			showTsHelp = true;          // Show Help icon and menu item
			showTsMenu = true;          // Show dropdown menu
			showTsMaxMin = true;        // Show maximize / minimize buttons
			showTsBar = true;           // Show OnlyBar button
			showTsTop = true;           // Show Topmost button
			TopMost = hasTopMost;       // Imposta subito lo stato topMost in base al flag

			ncWindowsState = prevNcWindowsState = ncStyle.ncFormWindowState;

			TitleColor = ncColor.titleBarColor;
			ButtonsColor = ncColor.buttonsColor;
			StatusBarColor = ncColor.statusBarColor;
			BackColor = NcColor.backColor;
			Opacity = ncColor.opacity;
			lastMouseDownEventTime = DateTime.Now;
			DblClickDelaySeconds = NcFormStyle.dblclkOnBarSecondsDefault;
			baseFormControlNames = new List<string>();
			
			foreach (Control control in this.Controls)
			{
				baseFormControlNames.Add(control.Name);		// Controlli della classe base NcForm
			}

			tsTitle.Font = style.barsFontBold;				// Font del titolo
			
		}
		/************************/

		/************************/
		// Public members
		/************************/

		/// <summary>
		/// NcStyle (readonly)
		/// </summary>
		public NcFormStyle NcStyle { get { return ncStyle; } }
		/// <summary>
		/// NcColor (readonly)
		/// </summary>
		public NcFormColor NcColor { get { return ncColor; } }	
		/// <summary>
		/// NcMsg (readonly)
		/// </summary>
		public NcFormMsg NcMsg { get { return ncMsg; } }

		/// <summary>
		/// Set/Get WindowsState: Minimized, Maximized, Normal, BarOnly
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public NcFormWindowStates NcWindowsState
		{
			get { return ncWindowsState; }
			protected set
			{
				switch(value)
				{
					case NcFormWindowStates.Normal:
						this.WindowState = FormWindowState.Normal;
						this.prevNcWindowsState = ncWindowsState;
						if(this.prevNcWindowsState == NcFormWindowStates.BarOnly)
						{
							this.Size = prevSz;
							tsLower.Visible = hasLowerBar;
							tsMin.Visible = tsMax.Visible = true;
							SetTitleBar();
							this.ncWindowsState = NcFormWindowStates.Normal;
						}
						else if(this.prevNcWindowsState == NcFormWindowStates.Minimized)
						{
							tsLower.Visible = hasLowerBar;
							this.ncWindowsState = NcFormWindowStates.Normal;
						}
						else if(this.prevNcWindowsState == NcFormWindowStates.Maximized)
						{
							tsLower.Visible = hasLowerBar;
							this.ncWindowsState = NcFormWindowStates.Normal;
						}
						tsMax.Text = "+";
						break;

					case NcFormWindowStates.Minimized:
						if(prevNcWindowsState == NcFormWindowStates.Normal)
						{
							prevSz = this.Size;
						}
						this.WindowState = FormWindowState.Minimized;
						this.prevNcWindowsState = ncWindowsState;
						this.ncWindowsState = NcFormWindowStates.Minimized;
						break;

					case NcFormWindowStates.Maximized:
						this.WindowState = FormWindowState.Maximized;
						tsMax.Text = "-";
						this.prevNcWindowsState = ncWindowsState;
						this.ncWindowsState = NcFormWindowStates.Maximized;
						break;

					case NcFormWindowStates.BarOnly:
						#warning CON BARONLY LA LARGHEZZA È TROPPO RIDOTTA E NON È POSSIBILE TRASCINARLA...
						#warning ...NASCONDERE I BUTTONS INUTILI O RICALCOLARE LARGHEZZA ?
						this.prevNcWindowsState = ncWindowsState;
						if(this.prevNcWindowsState == NcFormWindowStates.Normal)
						{
							this.WindowState = FormWindowState.Normal;
							prevSz = this.Size;
							this.ncWindowsState = NcFormWindowStates.BarOnly;
							tsLower.Visible = false;
							tsMin.Visible = tsMax.Visible = false;
							this.Size = new Size(Size.Width - int.Min(availWidthUpper,availWidthLower),tsUpper.Height);
							tsTitle.AutoSize = true;
						}
						break;

					default:
						break;
				}

			}

		}	
		/// <summary>
		/// Text on the status bar
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string StatusText
		{
			get { return statLabel.Text; }
			protected set { statLabel.Text = value; }
		}
		/// <summary>
		/// Title text
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Title
		{
			get { return tsTitle.Text; }
			protected set
			{
				SetTitleBar(value);
			}
		}	
		/// <summary>
		/// Opacity
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new float Opacity
		{
			get { return opacity; }
			protected set
			{
				opacity = value;
				base.Opacity = opacity;
			}
		}
		/// <summary>
		/// Title bar color
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color TitleColor
		{
			get { return colorTitle; }
			protected set
			{
				colorTitle = value;
				tsUpper.BackColor = colorTitle;
			}
		}
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color ButtonsColor
		{
			get { return colorButtons; }
			protected set
			{
				colorButtons = value;
				SetButtonsColor();
			}
		}
		/// <summary>
		/// Status bar color
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color StatusBarColor
		{
			get { return colorStatusBar; }
			protected set
			{
				colorStatusBar = value;
				tsLower.BackColor = colorStatusBar;
			}
		}
		/// <summary>
		/// Title height (readonly)
		/// </summary>
		public int UpperBarHeight
		{
			get { return minTitleSz.Height + tsItemExtraWidth; }
		}
		public int LowerBarHeight
		{
			get { return tsLower.Height + tsItemExtraWidth; }
		}
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int MinWidth
		{
			get {return minWidth;}
			protected set {SetMinWidth(value);}
		}
		/// <summary>
		/// TimeSpan to identify a double click on upper toolbar
		/// Default DoubleClickDelay = TimeSpan.FromSeconds(0.3);
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public double DblClickDelaySeconds
		{
			get {return doubleClickDelay.TotalSeconds;}
			protected set {doubleClickDelay = TimeSpan.FromSeconds(value);}
		}
		/// <summary>
		/// Ask before closing
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AskClose
		{
			get {	return askClose;	}
			protected set {
					askClose = value;
					tsQuit.ToolTipText = askClose ? ncMsg.CLOSE : ncMsg.CLOSE_IMM;
				}
		}
		/// <summary>
		/// Resize form to content
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected void ResizeToContent(int xDistance = 0, int yDistance = 0)
		{
			StringBuilder sb = new StringBuilder();
			Point pmin = new Point(int.MaxValue, int.MaxValue);
			Point pmax = new Point(int.MinValue, int.MinValue);
			bool xmin,xmax,ymin,ymax;
			xmin=xmax=ymin=ymax=false;
			foreach(Control control in this.Controls)
			{
				if(!IsBaseControl(control))
				{
					if(control.Dock == DockStyle.None)
					{
						if(control.Left < pmin.X) {pmin.X = control.Left; xmin = true;}
						if(control.Top < pmin.Y) {pmin.Y = control.Top; ymin = true;}
						if(control.Right > pmax.X) {pmax.X = control.Right; xmax = true;}
						if(control.Bottom > pmax.Y) {pmax.Y = control.Bottom; ymax = true;}
					}
				}

			}
			if(xmin && xmax && ymin && ymax)
			{
				Size newsize = new Size(pmax.X + xDistance, pmax.Y + tsLower.Height + yDistance);
				SetTitleBar();
				GetAdjustedSize(ref newsize);
				this.Size = newsize;
				ncWindowsState = NcFormWindowStates.Normal;
			}
		}
		/// <summary>
		/// Set bar font
		/// </summary>
		/// <param name="font"></param>
		/// <param name="ncbar"></param>
		protected void SetBarFont(Font font, NcBars ncbar)
		{
			int oldHgt = tsUpper.Height;
			if( (ncbar & NcBars.Upper) != 0 )		tsUpper.Font = font;
			if( (ncbar & NcBars.Lower) != 0 )		tsLower.Font = font;
			minTitleSz = tsUpper.Size;		// not tsTitle.Size;
			MoveControls(0, tsUpper.Height - oldHgt);
		}
		/// <summary>
		/// Return bar font (or null, if not specified)
		/// </summary>
		/// <param name="ncbar"></param>
		/// <returns></returns>
		protected Font GetBarFont(NcBars ncbar)
		{
			Font f = null;
			if(ncbar == NcBars.Upper)		
			{
				f = tsUpper.Font;
			}
			else if (ncbar == NcBars.Lower)
			{
				f = tsLower.Font;
			}
			return f;
		}		
		/// <summary>
		/// Version string
		/// </summary>
		/// <param name="execAssy">from Assembly.GetExecutingAssembly()</param>
		/// <returns></returns>
		virtual public string Version(Assembly asm, bool details = false)
		{
			StringBuilder strb = new StringBuilder();
			try
			{
				strb.AppendLine(Application.ProductName);
				if(asm != null)
				{
					System.Version? v = asm.GetName().Version;
					if(v != null) strb.AppendLine($"Version: {v.ToString()} ({BuildTime(asm)})");
					if(details)
					{
						string? n = asm.GetName().Name;
						if(n != null) strb.AppendLine("Assembly name: " + n);
						strb.AppendLine("BuildTime time: "+ File.GetCreationTime(asm.Location).ToString());
						strb.AppendLine("BuildTime number: " + BuildTime(asm,true));
						strb.AppendLine("Executable path: " + Application.ExecutablePath);
					}
				}
				strb.AppendLine("Copyright: " + Application.CompanyName);
			}
			catch	{}
			return strb.ToString();
		}
		/// <summary>
		/// Build string
		/// </summary>
		/// <param name="asm"></param>
		/// <returns></returns>
		public string BuildTime(Assembly asm, bool number = false)
		{
			StringBuilder strb = new StringBuilder();
			if(asm != null)
			{
				DateTime dt = File.GetCreationTime(asm.Location);
				if(number)
					strb.Append(String.Format("{0:yyMMddhh}.{0:mmss}",dt));
				else
					strb.Append(dt.ToString("d"));
			}	
			return strb.ToString();
		}
		/// <summary>
		/// Move controls
		/// </summary>
		/// <param name="xDelta"></param>
		/// <param name="yDelta"></param>
		protected void MoveControls(int xDelta, int yDelta)
		{
			foreach(Control control in this.Controls)
				{
					if(!IsBaseControl(control))
					{
						if(control.Dock == DockStyle.None)
						{
							control.Location = new Point(control.Location.X + xDelta, control.Location.Y + yDelta);
							//if(control.Left < pmin.X) {pmin.X = control.Left; xmin = true;}
							//if(control.Top < pmin.Y) {pmin.Y = control.Top; ymin = true;}
							//if(control.Right > pmax.X) {pmax.X = control.Right; xmax = true;}
							//if(control.Bottom > pmax.Y) {pmax.Y = control.Bottom; ymax = true;}

							//sb.AppendLine($"{control.Name} top={control.Top} left={control.Left} bottom={control.Bottom} right={control.Right}");
						}
					}

				}
			}
		/// <summary>
		/// Add needed events to control
		/// if created after base.OnLoad event
		/// </summary>
		/// <param name="control"></param>
		protected void SetupControlEvents(Control control)
		{
			control.MouseEnter += eMouseEnter;
			control.MouseLeave += eMouseLeave;
		}
		/************************/

		/**************************************/
		// Virtual functions (to be overridden)
		/**************************************/
		
		/// <summary>
		/// Help function
		/// </summary>
		protected virtual void OnHelp()
		{
			NcMessageBox.Show(this,Version(Assembly.GetExecutingAssembly()));
		}

		/// <summary>
		/// Ask confirmation onFormClosing event
		/// </summary>
		/// <returns>true to cancel event</returns>
		protected virtual bool OnClosingCancelEvent()
		{
			bool cancel = false;
			if(NcMessageBox.Show(this,ncMsg.QUIT,ncMsg.QUIT_ASK,MessageBoxButtons.YesNo) != DialogResult.Yes)
			{
				cancel = true;	
			}
			return cancel;
		}

		public void NcForm_FormClosing(object sender,FormClosingEventArgs e)
		{
			closing = true;
			if(askClose && !loading)
			{
				if(OnClosingCancelEvent())
				{
					e.Cancel = true;
					closing = false;
				}
			}
		}

		/************************/

		/*********************************/
		// Private functions and handlers
		/*********************************/
		#region Private functions and handlers
		private void NcForm_Load(object sender,EventArgs e)
		{
			SetupControls(this);		// Executed on derived class controls, not only base class controls.
			NcWindowsState = ncWindowsState;
			SetBarFont(ncStyle.barsFont,NcBars.All);
			loading = false;
		}
		private void SetTitleBar(string? txt = null)
		{
			showTsMenu = hasMenu;               // Usa i flag del costruttore
			showTsHelp = hasHelp;
			showTsMaxMin = hasMinMax;
			showTsTop = hasTopMost;

			// Imposta visibilità
			reszLabel.Visible = isResizable;    // Usa il flag del costruttore
			tsMenu.Visible = showTsMenu;
			tsHelp.Visible = tsmiHelp.Visible = showTsHelp;
			tsMax.Visible = tsMin.Visible = tsBar.Visible = showTsMaxMin;
			tsTop.Visible = showTsTop;
			
			if(ncWindowsState != NcFormWindowStates.BarOnly)
			{
				tsLower.Visible = hasLowerBar;
			}

			if(txt != null) tsTitle.Text = txt;
			tsTitle.AutoSize = true;
			minTitleSz = tsTitle.Size;

			availWidthUpper = this.Width
								- (showTsMenu ? tsMenu.Width + tsItemExtraWidth : tsItemExtraWidth)
								- (showTsHelp ? tsHelp.Width + tsItemExtraWidth : tsItemExtraWidth)
								- (showTsTop ? tsTop.Width + tsItemExtraWidth : tsItemExtraWidth)
								- (showTsMaxMin ? tsMax.Width + tsMin.Width + tsBar.Width + 3 * tsItemExtraWidth : 3 * tsItemExtraWidth)
								- (tsQuit.Width + tsItemExtraWidth);
			availWidthLower = this.Width - statLabel.Width - reszLabel.Width;

			#warning ERRORE NEL CALCOLO DELLA VISIBILITÀ E DELLA LARGHEZZA.
			if(availWidthUpper < minTitleSz.Width)
			{
				tsTitle.Visible = false;
			}
			else
			{
				tsTitle.AutoSize = false;
				tsTitle.Visible = true;
				tsTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
				tsTitle.Size = new Size(availWidthUpper,minTitleSz.Height);
			}
		}
		private void InitializeComponent()
		{
			tsUpper = new ToolStrip();
			tsMenu = new ToolStripDropDownButton();
			settingsToolStripMenuItem = new ToolStripMenuItem();
			tsmiHelp = new ToolStripMenuItem();
			tsmiQuit = new ToolStripMenuItem();
			tsTop = new ToolStripButton();
			tsTitle = new ToolStripLabel();
			tsQuit = new ToolStripButton();
			tsHelp = new ToolStripButton();
			tsMax = new ToolStripButton();
			tsMin = new ToolStripButton();
			tsBar = new ToolStripButton();
			tsLower = new ToolStrip();
			statLabel = new ToolStripLabel();
			reszLabel = new ToolStripLabel();
			tsUpper.SuspendLayout();
			tsLower.SuspendLayout();
			SuspendLayout();
			// 
			// tsUpper
			// 
			tsUpper.BackColor = SystemColors.Control;
			tsUpper.CanOverflow = false;
			tsUpper.Items.AddRange(new ToolStripItem[] { tsMenu,tsTop,tsTitle,tsQuit,tsHelp,tsMax,tsMin,tsBar });
			tsUpper.Location = new Point(0,0);
			tsUpper.Name = "tsUpper";
			tsUpper.Size = new Size(491,25);
			tsUpper.TabIndex = 0;
			tsUpper.Text = "tsUpper";
			tsUpper.MouseDown += ts_MouseDown;
			tsUpper.MouseUp += ts_MouseUp;
			// 
			// tsMenu
			// 
			tsMenu.AccessibleDescription = "";
			tsMenu.AccessibleName = "";
			tsMenu.AutoToolTip = false;
			tsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
			tsMenu.DropDownItems.AddRange(new ToolStripItem[] { settingsToolStripMenuItem,tsmiHelp,tsmiQuit });
			tsMenu.Name = "tsMenu";
			tsMenu.Size = new Size(26,22);
			tsMenu.Text = "v";
			// 
			// settingsToolStripMenuItem
			// 
			settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
			settingsToolStripMenuItem.Size = new Size(116,22);
			settingsToolStripMenuItem.Text = ncMsg.SETTINGS;
			settingsToolStripMenuItem.Click += settingsToolStripMenuItem_Click;
			// 
			// tsmiHelp
			// 
			tsmiHelp.Name = "tsmiHelp";
			tsmiHelp.Size = new Size(116,22);
			tsmiHelp.Text = ncMsg.HELP;
			tsmiHelp.Click += tsHelp_Click;
			// 
			// tsmiQuit
			// 
			tsmiQuit.Name = "tsmiQuit";
			tsmiQuit.Size = new Size(116,22);
			tsmiQuit.Text = ncMsg.QUIT;
			tsmiQuit.Click += tsmiQuit_Click;
			// 
			// tsTop
			// 
			tsTop.DisplayStyle = ToolStripItemDisplayStyle.Text;
			tsTop.ImageTransparentColor = Color.Magenta;
			tsTop.Name = "tsTop";
			tsTop.Size = new Size(23,22);
			tsTop.Text = "T";
			tsTop.ToolTipText = ncMsg.ONTOP;
			tsTop.Click += tsTop_Click;
			// 
			// tsTitle
			// 
			tsTitle.AutoSize = false;
			tsTitle.BackColor = SystemColors.Control;
			tsTitle.DisplayStyle = ToolStripItemDisplayStyle.Text;
			tsTitle.Font = new Font("Segoe UI",9F,FontStyle.Bold);
			tsTitle.ImageTransparentColor = Color.White;
			tsTitle.Name = "tsTitle";
			tsTitle.Size = new Size(100,22);
			tsTitle.Text = "Title";
			tsTitle.TextImageRelation = TextImageRelation.TextBeforeImage;
			tsTitle.MouseDown += tsTitle_MouseDown;
			tsTitle.MouseUp += tsTitle_MouseUp;
			// 
			// tsQuit
			// 
			tsQuit.Alignment = ToolStripItemAlignment.Right;
			tsQuit.DisplayStyle = ToolStripItemDisplayStyle.Text;
			tsQuit.Name = "tsQuit";
			tsQuit.Size = new Size(23,22);
			tsQuit.Text = "X";
			tsQuit.ToolTipText = ncMsg.QUIT;
			tsQuit.Click += tsClose_Click;
			// 
			// tsHelp
			// 
			tsHelp.Alignment = ToolStripItemAlignment.Right;
			tsHelp.DisplayStyle = ToolStripItemDisplayStyle.Text;
			tsHelp.Name = "tsHelp";
			tsHelp.RightToLeft = RightToLeft.No;
			tsHelp.Size = new Size(23,22);
			tsHelp.Text = "?";
			tsHelp.ToolTipText = ncMsg.HELP;
			tsHelp.Click += tsHelp_Click;
			// 
			// tsMax
			// 
			tsMax.Alignment = ToolStripItemAlignment.Right;
			tsMax.DisplayStyle = ToolStripItemDisplayStyle.Text;
			tsMax.ImageTransparentColor = Color.Magenta;
			tsMax.Name = "tsMax";
			tsMax.RightToLeft = RightToLeft.Yes;
			tsMax.Size = new Size(23,22);
			tsMax.Text = "+";
			tsMax.ToolTipText = ncMsg.MAXIMIZE;
			tsMax.Click += tsMax_Click;
			// 
			// tsMin
			// 
			tsMin.Alignment = ToolStripItemAlignment.Right;
			tsMin.DisplayStyle = ToolStripItemDisplayStyle.Text;
			tsMin.ImageTransparentColor = Color.Magenta;
			tsMin.Name = "tsMin";
			tsMin.RightToLeft = RightToLeft.Yes;
			tsMin.Size = new Size(23,22);
			tsMin.Text = "_";
			tsMin.ToolTipText = ncMsg.MINIMIZE;
			tsMin.Click += tsMin_Click;
			// 
			// tsBar
			// 
			tsBar.Alignment = ToolStripItemAlignment.Right;
			tsBar.DisplayStyle = ToolStripItemDisplayStyle.Text;
			tsBar.ImageTransparentColor = Color.Magenta;
			tsBar.Name = "tsBar";
			tsBar.RightToLeft = RightToLeft.Yes;
			tsBar.Size = new Size(23,22);
			tsBar.Text = "=";
			tsBar.ToolTipText = ncMsg.BAR_ONLY;
			tsBar.Click += tsBar_Click;
			// 
			// tsLower
			// 
			tsLower.CanOverflow = false;
			tsLower.Dock = DockStyle.Bottom;
			tsLower.Items.AddRange(new ToolStripItem[] { statLabel,reszLabel });
			tsLower.Location = new Point(0,318);
			tsLower.Name = "tsLower";
			tsLower.Size = new Size(491,25);
			tsLower.TabIndex = 2;
			tsLower.Text = "tsLower";
			// 
			// statLabel
			// 
			statLabel.DisplayStyle = ToolStripItemDisplayStyle.Text;
			statLabel.Name = "statLabel";
			statLabel.Size = new Size(39,22);
			statLabel.Text = ncMsg.STATUS;
			// 
			// reszLabel
			// 
			reszLabel.Alignment = ToolStripItemAlignment.Right;
			reszLabel.BackColor = SystemColors.ActiveCaption;
			reszLabel.DisplayStyle = ToolStripItemDisplayStyle.Text;
			reszLabel.Name = "reszLabel";
			reszLabel.Size = new Size(23,22);
			reszLabel.Text = "<>";
			reszLabel.MouseDown += reszLabel_MouseDown;
			reszLabel.MouseEnter += reszLabel_MouseEnter;
			reszLabel.MouseLeave += reszLabel_MouseLeave;
			// 
			// NcForm
			// 
			BackColor = SystemColors.Control;
			ClientSize = new Size(491,343);
			Controls.Add(tsLower);
			Controls.Add(tsUpper);
			FormBorderStyle = FormBorderStyle.None;
			Name = "NcForm";
			FormClosing += NcForm_FormClosing;
			Load += NcForm_Load;
			ResizeEnd += NcForm_ResizeEnd;
			MouseEnter += eMouseEnter;
			MouseLeave += eMouseLeave;
			MouseUp += NcForm_MouseUp;
			Resize += NcForm_Resize;
			tsUpper.ResumeLayout(false);
			tsUpper.PerformLayout();
			tsLower.ResumeLayout(false);
			tsLower.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		/// <summary>
		/// Set-up form controls' handlers
		/// </summary>
		/// <param name="control"></param>
		private void SetupControls(Control control)
		{
			SetButtonsColor();
			SetEnterLeaveForControls(control,eMouseEnter,eMouseLeave);
			SetEnterLeaveForSingleControl(tsUpper,eMouseEnter,eMouseLeave,etsMouseEnter,etsMouseLeave);
			SetMouseMoveSingleControl(tsUpper,Mouse_Move);
			SetTitleBar();
		}
		private bool IsBaseControl(Control control)
		{
			return baseFormControlNames.Contains(control.Name);
		}
		private string GetControlNames()
		{	
			StringBuilder sb = new StringBuilder();
			foreach (Control control in this.Controls)
			{
				sb.AppendLine(control.Name);
			}
			return sb.ToString();
		}
		private void SetMinWidth(int mw)
		{
			if(mw >= 0)
			{
				minWidth = mw;
			}
		}
		private bool GetAdjustedSize(ref Size size)
		{
			bool resize = false;
			Size newsize = size;
			
			int minWidth = this.Width - int.Min(availWidthUpper,availWidthLower);
			int minHeight = tsUpper.Height + tsLower.Height + contentMinHeight;

			if(size.Width < minWidth)
			{
				newsize.Width = minWidth;
				resize = true;
			}
			if(size.Height < minHeight)
			{
				newsize.Height = minHeight;
				resize = true;
			}

			if(resize)		size = newsize;
			return resize;
		}
		private void AdjustSize()
		{
			Size newsize = this.Size;
			if(GetAdjustedSize(ref newsize))
			{
				this.Size = newsize;
			}
			if(this.Width < minWidth)
			{
				this.Width = minWidth;
			}
		}
		private void SwitchBarOnly()
		{
			if(NcWindowsState == NcFormWindowStates.Normal)
			{
				NcWindowsState = NcFormWindowStates.BarOnly;
			}
			else if(NcWindowsState == NcFormWindowStates.BarOnly)
			{
				NcWindowsState = NcFormWindowStates.Normal;
			}
		}
		private void SetEnterLeaveForControls(Control control,EventHandler eVmouseEnter,EventHandler eVmouseLeave)
		{
			foreach(Control childControl in control.Controls)
			{
				childControl.MouseEnter += eVmouseEnter;
				childControl.MouseLeave += eVmouseLeave;
				SetEnterLeaveForControls(childControl,eVmouseEnter,eVmouseLeave);
			}

		}
		private void SetButtonsColor()
		{
			foreach(Control control in this.Controls)
				{
					if(control.GetType() == typeof(Button))
					{
						control.BackColor = ButtonsColor;
					}
				}
		}
		private void ClearEnterLeaveForControls(Control control,EventHandler eVmouseEnter,EventHandler eVmouseLeave)
		{
			foreach(Control childControl in control.Controls)
			{
				childControl.MouseEnter -= eVmouseEnter;
				childControl.MouseLeave -= eVmouseLeave;
				ClearEnterLeaveForControls(childControl,eVmouseEnter,eVmouseLeave);
			}

		}
		private void SetEnterLeaveForSingleControl(Control control,EventHandler oldVmouseEnter,EventHandler oldVmouseLeave,EventHandler eVmouseEnter,EventHandler eVmouseLeave)
		{
			control.MouseEnter -= oldVmouseEnter;
			control.MouseLeave -= oldVmouseLeave;
			control.MouseEnter += eVmouseEnter;
			control.MouseLeave += eVmouseLeave;
		}
		private void SetMouseMoveSingleControl(Control control,MouseEventHandler eVmouseMove)
		{
			this.MouseMove += eVmouseMove;
		}
		private void tsClose_Click(object sender,EventArgs e)
		{
			Close();
		}
		private void tsmiQuit_Click(object sender,EventArgs e)
		{
			Close();
		}
		private void settingsToolStripMenuItem_Click(object sender,EventArgs e)
		{
			#warning DA COMPLETARE
			NcMessageBox.Show(this,"Funzione da scrivere ex-novo","DA COMPLETARE",MessageBoxButtons.OK);

			//config.ShowDialog();

		}
		private void eMouseEnter(object sender,EventArgs e)
		{
			base.Opacity = 1f;
			this.Focus();	// Get focus, no need of an extra click to get it
		}
		/// <summary>
		/// MouseLeave handler.
		/// If closing, no action is performed (the control might have been destroyed)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void eMouseLeave(object sender,EventArgs e)
		{
			if(!closing)
			{
				base.Opacity = Opacity;
			}
		}
		private void etsMouseEnter(object sender,EventArgs e)
		{
			eMouseEnter(sender,e);
			this.Cursor = Cursors.Cross;
			dragging = false;   // Se esce, non cattura il MouseUp. Al rientro deve interrompere il dragging
		}
		private void etsMouseLeave(object sender,EventArgs e)
		{
			eMouseLeave(sender,e);
			this.Cursor = Cursors.Default;
		}
		private void ts_MouseDown(object sender,MouseEventArgs e)
		{
			Mouse_Down(sender,e);
		}
		private void ts_MouseUp(object sender,MouseEventArgs e)
		{
			Mouse_Up(sender,e);
		}
		private void tsTitle_MouseUp(object sender,MouseEventArgs e)
		{
			Mouse_Up(sender,e);
		}
		private void tsTitle_MouseDown(object sender,MouseEventArgs e)
		{
			Mouse_Down(sender,e);
		}
		private void Mouse_Down(object sender,MouseEventArgs e)
		{
			DateTime dateTime = DateTime.Now;
			if(dateTime - lastMouseDownEventTime < doubleClickDelay)
			{
				tsBar_DoubleClick();
				return;
			}
			lastMouseDownEventTime = dateTime;
			if((NcWindowsState == NcFormWindowStates.Normal) || (NcWindowsState == NcFormWindowStates.BarOnly))
			{
				startDrag = this.Location;
				startCur = Cursor.Position;
				dragging = true;
				this.Capture = true;
			}
		}
		private void Mouse_Up(object sender,MouseEventArgs e)
		{
			if(dragging)
			{
				dragging = false;
			}
			if(resizing)
			{
				resizing = false;
			}
			this.Capture = false;
			//SetTextStatus($"MouseUp {e.Location.ToString()}");
		}
		private void Mouse_Move(object sender,MouseEventArgs e)
		{
			if(dragging && ((NcWindowsState == NcFormWindowStates.Normal) || (NcWindowsState == NcFormWindowStates.BarOnly)))
			{
				Point dif = Point.Subtract(Cursor.Position,new Size(startCur));
				this.Location = Point.Add(startDrag,new Size(dif));
				Invalidate();
			}
			if(resizing && (NcWindowsState == NcFormWindowStates.Normal))
			{
				Point dif = Point.Subtract(Cursor.Position,new Size(startResz));
				Size newSz = Size.Add(startSz,new Size(dif));
				this.Size = newSz;
				Invalidate();
			}

		}
		private void reszLabel_MouseEnter(object sender,EventArgs e)
		{
			Cursor = Cursors.SizeNWSE;
			//statLabel.Text = "Enter";
		}
		private void reszLabel_MouseLeave(object sender,EventArgs e)
		{
			Cursor = Cursors.Default;
		}
		private void reszLabel_MouseDown(object sender,MouseEventArgs e)
		{
			if(NcWindowsState == NcFormWindowStates.Normal)
			{
				resizing = true;
				startResz = Cursor.Position;
				startSz = this.Size;
				this.Capture = true;
			}
		}
		/// <summary>
		/// With this.Capture mouse events are captured by the form, not by reszLabel_MouseUp(...)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NcForm_MouseUp(object sender,MouseEventArgs e)
		{
			if(resizing)
			{
				this.Capture = false;
				resizing = false;
				NcForm_ResizeEnd(sender,e);
			}
		}
		private void NcForm_ResizeEnd(object sender,EventArgs e)
		{
			SetTitleBar();
			AdjustSize();
		}
		private void tsMin_Click(object sender,EventArgs e)
		{
			NcWindowsState = NcFormWindowStates.Minimized;
		}
		private void tsMax_Click(object sender,EventArgs e)
		{
			if(NcWindowsState == NcFormWindowStates.Normal)
			{
				NcWindowsState = NcFormWindowStates.Maximized;
			}
			else
			{
				NcWindowsState = NcFormWindowStates.Normal;
			}
		}
		private void tsBar_Click(object sender,EventArgs e)
		{
			SwitchBarOnly();
		}
		/// <summary>
		/// Called when the form is restored after having been minimized
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NcForm_Resize(object sender,EventArgs e)
		{
			if(NcWindowsState == NcFormWindowStates.Minimized)
			{
				NcWindowsState = NcFormWindowStates.Normal;
			}
			SetTitleBar();
		}
		private void tsTop_Click(object sender,EventArgs e)
		{
			TopMost = !(TopMost);   // Scambia lo stato
			tsTop.Text = TopMost ? "T" : "t";
		}
		private void tsHelp_Click(object sender,EventArgs e)
		{
			OnHelp();		// Chiama la funzione virtuale (che può subire l'override)
		}		
		private void tsBar_DoubleClick()
		{
			SwitchBarOnly();
		}
		#endregion
		/************************/

	}
	#pragma warning restore
}
