using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Gtk;
using Gtk.DotNet;
using Cairo;

using CSGen.Model;
using CSGen.Operations;
using System.Reflection;
using System.Linq.Expressions;
using Stetic;


public partial class MainWindow: Gtk.Window
{	
	#region Private Members
	private SongData _currentSong = new SongData();
	private Config _settings = new Config();
	private BindingLib _bindings = new BindingLib();
	private List<Button> _tagBarButtons = new List<Button>();
	private List<Label> _tagBarLabels = new List<Label>();
	private bool _updateInProgress = false;
	private EventManager _eventManger = new EventManager();

	private FontInfo[] _fonts = new FontInfo[Constants._fontCount];
	private FontInfo[] _workingFonts = null;
	private AppSettings _appSettings = new AppSettings();
	#endregion

	#region Initialization Methods and Events
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();

		_eventManger.RegisterControl (txtEditor);
		_eventManger.RegisterControl (txtTitle);
		_eventManger.RegisterControl (txtArtist);
		_eventManger.RegisterControl (txtCopyright);
		_eventManger.RegisterControl (txtCCLI);
		_eventManger.RegisterControl (txtCapo);
		_eventManger.RegisterControl (txtKey);

		_eventManger.StatusChanged += on_eventManger_StatusChanged;
		txtEditor.Buffer.Changed += new EventHandler (on_txtEditor_Changed);

		txtEditor.FocusOutEvent += on_edit_FocusOutEvent;
		txtTitle.FocusOutEvent += on_edit_FocusOutEvent;
		txtArtist.FocusOutEvent += on_edit_FocusOutEvent;
		txtCopyright.FocusOutEvent += on_edit_FocusOutEvent;
		txtCCLI.FocusOutEvent += on_edit_FocusOutEvent;
		txtCapo.FocusOutEvent += on_edit_FocusOutEvent;
		txtKey.FocusOutEvent += on_edit_FocusOutEvent;

		previewPane.Page = 0;
		drawingArea.ExposeEvent += on_drawingArea_Expose;

		// Initialize our status and settings.
		_bindings.AddBinding (btnShowInfo, _settings, "ShowSongInfo");
		_bindings.AddBinding (SongInformationAction, _settings, "ShowSongInfo");
		_bindings.AddBinding (TagChordBarAction, _settings, "ShowTagChordBar");
		_bindings.AddBinding (btnShowTagBar, _settings, "ShowTagChordBar");
		_bindings.UpdateBindings ();

		CreateTagBarControls ();

		for (int i = 0; i < Constants._fontCount; i++)
		{
			_fonts [i] = _settings.GetFont ((PrintFontType)i);
		}

		_appSettings.DefaultFolder = _settings.DefaultFolder;
		_appSettings.ShowHeader = _settings.ShowHeader;
		_appSettings.HeaderFieldLeft = (PrintFieldType)_settings.HeaderFieldLeft;
		_appSettings.HeaderFieldCenter = (PrintFieldType)_settings.HeaderFieldCenter;
		_appSettings.HeaderFieldRight = (PrintFieldType)_settings.HeaderFieldRight;
		_appSettings.HeaderLineType = (PrintLineType)_settings.HeaderLineType;
		_appSettings.ShowFooter = _settings.ShowFooter;
		_appSettings.FooterFieldLeft = (PrintFieldType)_settings.FooterFieldLeft;
		_appSettings.FooterFieldCenter = (PrintFieldType)_settings.FooterFieldCenter;
		_appSettings.FooterFieldRight = (PrintFieldType)_settings.FooterFieldRight;
		_appSettings.FooterLineType = (PrintLineType)_settings.FooterLineType;


		CheckStatuses ();
		txtTitle.IsFocus = true;
		on_btnSongInfo_Toggled (null, null);
		on_TagChordBarAction_Toggle (null, null);

	}


	#region Helper Events

	private Widget _lastWidget = null;
	private int	_lastStart = 0;
	private int _lastEnd = 0;

	void on_edit_FocusOutEvent (object o, FocusOutEventArgs args)
	{
		_lastWidget = (Widget)o;
		if (o.GetType ().IsAssignableFrom (typeof(TextView)))
		{
			TextIter start;
			TextIter end;

			((TextView)o).Buffer.GetSelectionBounds (out start, out end);
			_lastStart = start.Offset;
			_lastEnd = end.Offset;
		} else if (o.GetType ().IsAssignableFrom (typeof(Entry)))
		{
			((Entry)o).GetSelectionBounds (out _lastStart, out _lastEnd);
		}
	}

	void on_eventManger_StatusChanged (object sender, EventArgs e)
	{
		btnUndo.Sensitive = undoAction.Sensitive = _eventManger.CanUndo;
		btnRedo.Sensitive = redoAction.Sensitive = _eventManger.CanRedo;
	}
	#endregion

	#endregion

	#region Shutdown Methods and Events
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		if (ModifiedActionCheck (onExit: true))
		{
			SaveSettings ();
			Application.Quit ();
		}
		a.RetVal = true;
	}


	protected void on_Exit (object sender, EventArgs e)
	{
		if (ModifiedActionCheck (onExit: true))
		{
			SaveSettings ();
			Application.Quit ();
		}
	}

	protected void SaveSettings()
	{
		for (int i = 0; i < Constants._fontCount; i++)
		{
			_settings.SetFont (_fonts [i], (PrintFontType)i);
		}

		if (!string.IsNullOrEmpty (_appSettings.DefaultFolder))
			_settings.DefaultFolder = _appSettings.DefaultFolder;
		else 
			_settings.DefaultFolder = string.Empty;

		_settings.ShowHeader = _appSettings.ShowHeader;
		_settings.HeaderFieldLeft = (int)_appSettings.HeaderFieldLeft;
		_settings.HeaderFieldCenter = (int)_appSettings.HeaderFieldCenter;
		_settings.HeaderFieldRight = (int)_appSettings.HeaderFieldRight;
		_settings.HeaderLineType = (int)_appSettings.HeaderLineType;
		_settings.ShowFooter = _appSettings.ShowFooter;
		_settings.FooterFieldLeft = (int)_appSettings.FooterFieldLeft;
		_settings.FooterFieldCenter = (int)_appSettings.FooterFieldCenter;
		_settings.FooterFieldRight = (int)_appSettings.FooterFieldRight;
		_settings.FooterLineType = (int)_appSettings.FooterLineType;


		_settings.Save ();
	}


	/// <summary>
	///  Check to see if the user needs to be warned of unsaved changes.
	/// </summary>
	/// <returns><c>true</c>, if the action can continue to be processed, <c>false</c> if it should be cancelled.</returns>
	protected bool ModifiedActionCheck(bool onExit = false)
	{
		bool retVal = true;
		if (_currentSong.IsModified)
		{
			MessageDialog md = new MessageDialog (this
                            		              , DialogFlags.DestroyWithParent
                                      			  , MessageType.Warning
                                        		  , ButtonsType.None
                                        		  , "The current file has been modified.  Do you want to save it?");

			Button btn = (Button)md.AddButton ("Cancel", (int)ResponseType.Cancel);
			Image img = new Image ();
			img.Pixbuf = IconLoader.LoadIcon (this, "gtk-cancel", IconSize.Menu);
			btn.Image = img;

			btn = (Button)md.AddButton ("Discard", (int)ResponseType.No);
			Image img2 = new Image ();
			if(onExit)
				img2.Pixbuf = IconLoader.LoadIcon (this, "gtk-quit", IconSize.Menu);
			else
				img2.Pixbuf = IconLoader.LoadIcon (this, "gtk-delete", IconSize.Menu);
			btn.Image = img2;

			btn = (Button)md.AddButton (onExit ? "Save and Quit" : "Save", (int)ResponseType.Yes);
			Image img3 = new Image ();
			img3.Pixbuf = IconLoader.LoadIcon (this, "gtk-save", IconSize.Menu);
			btn.Image = img3;

			int response = md.Run ();

			if (response == (int)ResponseType.Yes)
			{
				on_SaveSongFile (null, null);
			} else if (response == (int)ResponseType.Cancel)
			{
				retVal = false;
			}

			md.Destroy ();
		}


		return retVal;
	}


	#endregion

	#region File Events
	
	protected void on_NewSongActivated (object sender, EventArgs e)
	{
		if (ModifiedActionCheck ())
		{
			_currentSong = new SongData ();
			SetSongFields ();
			_eventManger.ClearEvents ();
		}
	}

	protected void on_OpenSongFile (object sender, EventArgs e)
	{
		if (ModifiedActionCheck ())
		{

			Gtk.FileChooserDialog filechooser =
				new Gtk.FileChooserDialog ("Choose the song file to open",
				                           this,
				                           FileChooserAction.Open,
				                           "Cancel", ResponseType.Cancel,
				                           "Open", ResponseType.Accept);

			FileFilter filter = new FileFilter ();
			filter.AddPattern ("*.txt");
			filter.AddPattern ("*.dbt");
			filter.AddPattern ("*.chordpro");
			filter.AddPattern ("*.chopro");
			filter.AddPattern ("*.cho");
			filter.AddPattern ("*.crd");
			filter.AddPattern ("*.pro");

			filechooser.Filter = filter;

			string folder = _appSettings.DefaultFolder;
			if (!string.IsNullOrEmpty (folder)
			    && System.IO.Directory.Exists (folder))
			{
				filechooser.SetCurrentFolder (folder);
			}



			if (filechooser.Run () == (int)ResponseType.Accept)
			{
				//string text = System.IO.File.ReadAllText (filechooser.Filename);
				//txtEditor.Buffer.Text = text;

				_currentSong = FileOperations.OpenSongFile (filechooser.Filename);

				SetSongFields ();
			}

			filechooser.Destroy ();

			// Clear the undo/redo events
			_eventManger.ClearEvents ();
		}
	}

	protected void on_SaveSongFile (object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty (_currentSong.FilePath))
			on_SaveSongFileAs (sender, e);
		else
		{
			FileOperations.SaveSongFile (_currentSong);
			CheckStatuses ();
		}
	}

	protected void on_SaveSongFileAs (object sender, EventArgs e)
	{
		Gtk.FileChooserDialog filechooser =
			new Gtk.FileChooserDialog("Save the song file",
			                          this,
			                          FileChooserAction.Save,
			                          "Cancel",ResponseType.Cancel,
			                          "Save As",ResponseType.Accept);

		FileFilter filter = new FileFilter ();
		filter.AddPattern ("*.chordpro");

		filechooser.Filter = filter;

		if (!string.IsNullOrEmpty (_currentSong.FilePath))
		{
			filechooser.SetFilename(_currentSong.FilePath);
		} else
		{
			string title = _currentSong.Title.Trim ();
			title = title.Replace ("/", "-");
			title = title.Replace ("\\", "-");
			title = title.Replace (".", "");
			filechooser.CurrentName = title;
		}


		string folder = _appSettings.DefaultFolder;
		if (!string.IsNullOrEmpty (folder)
		    && System.IO.Directory.Exists (folder))
		{
			filechooser.SetCurrentFolder (folder);
		}


		if (filechooser.Run () == (int)ResponseType.Accept)
		{
			string fileName = filechooser.Filename;
			fileName = fileName + ".chordpro";

			_currentSong.FilePath = fileName;
			FileOperations.SaveSongFile (_currentSong);

			CheckStatuses ();
		}

		filechooser.Destroy();
	}

	protected void on_printAction_Activated (object sender, EventArgs e)
	{
		RenderSong (RenderOption.Print);
	}

	protected void on_printPreviewAction_Activated (object sender, EventArgs e)
	{
		RenderSong (RenderOption.PrintPreview);

	}

	protected void on_settingsAction_Activated (object sender, EventArgs e)
	{
		CSGen.SettingsDialog dialog = new CSGen.SettingsDialog (_appSettings, _fonts);
		dialog.ChordSheetOptionsChanged += HandleChordSheetOptionsChanged;
		int response = dialog.Run ();

		try
		{
			if (response == (int)Gtk.ResponseType.Ok)
			{
				if(_workingFonts != null)
				{
					_fonts = _workingFonts;
				}
				_appSettings = dialog.AppSettings.Clone();
			}

			_workingFonts = null;
			drawingArea.QueueDraw ();
		}
		catch(Exception ex)
		{
			MessageDialog msgDialog = new MessageDialog (
				this, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Ok,
				"Message: {0}\n\n Stacktrace: {1}", ex.Message, ex.StackTrace);
			msgDialog.Run ();
			msgDialog.Destroy ();

		}
		dialog.Destroy ();
	}

	void HandleChordSheetOptionsChanged (object sender, EventArgs e)
	{
		_workingFonts = ((CSGen.SettingsDialog)sender).WorkingFonts;
		drawingArea.QueueDraw();
	}

	#endregion

	#region Editor Events
	protected void on_txtTitle_Changed (object sender, EventArgs e)
	{
		_currentSong.Title = txtTitle.Text;
		CheckStatuses ();
	}

	protected void on_txtArtist_Changed (object sender, EventArgs e)
	{
		_currentSong.Artist = txtArtist.Text;
		CheckStatuses ();
	}

	protected void on_txtCopyright_Changed (object sender, EventArgs e)
	{
		if (txtCopyright.Text.Contains ("(c)"))
		{
			txtCopyright.Text = txtCopyright.Text.Replace ("(c)", "©");
			txtCopyright.Position -= 2;
		}
		if (txtCopyright.Text.Contains ("(C)"))
		{
			txtCopyright.Text = txtCopyright.Text.Replace ("(C)", "©");
			txtCopyright.Position -= 2;
		}

		_currentSong.Copyright = txtCopyright.Text;
		CheckStatuses ();
	}

	protected void on_txtCCLI_Changed (object sender, EventArgs e)
	{
		_currentSong.CCLI = txtCCLI.Text;
		CheckStatuses ();
	}

	protected void on_txtKey_Changed (object sender, EventArgs e)
	{
		_currentSong.Key = txtKey.Text;
		CheckStatuses ();
	}

	protected void on_txtCapo_Changed (object sender, EventArgs e)
	{
		_currentSong.Capo = txtCapo.Text;
		CheckStatuses ();
	}

	protected void on_txtEditor_Changed (object sender, EventArgs e)
	{
		_currentSong.ContentText = txtEditor.Buffer.Text;
		UpdateSyntaxHighlighting ();
		CheckStatuses ();
	}

	protected void on_txtEditor_KeyPress (object o, KeyPressEventArgs args)
	{
		Gdk.ModifierType modifiers = Accelerator.DefaultModMask;

		Gdk.Key key = args.Event.Key;
		uint keyVal = args.Event.KeyValue;

		if ((key == Gdk.Key.Control_L || key == Gdk.Key.Control_R)
		    && !btnChordBarOption.Active)
		{
			btnChordBarOption.Active = true;
		}
		else if ((key == Gdk.Key.Alt_L || key == Gdk.Key.Alt_R)
		         && !btnTagBarOption.Active)
		{
			btnTagBarOption.Active = true;
		}

		if (keyVal < 255 || (keyVal >= 65470 && keyVal <= 65481))
		{
			if ((args.Event.State & modifiers) == Gdk.ModifierType.ControlMask)			
			{
				for (int cnt =0; cnt < this._tagBarButtons.Count; cnt++)
				{
					if (keyVal == Gdk.Keyval.FromName(_settings.GetTagBarKeyMap (cnt))
					    && _tagBarButtons[cnt].Visible)
					{
						_tagBarButtons [cnt].Click ();
						args.RetVal = true;
						break;
					}
				}

			}
			else if ((args.Event.State & modifiers) == Gdk.ModifierType.Mod1Mask)
			{
				for (int cnt =0; cnt < this._tagBarButtons.Count; cnt++)
				{
					if (keyVal == Gdk.Keyval.FromName(_settings.GetChordBarKeyMap (cnt))
					    && _tagBarButtons[cnt].Visible)
					{
						_tagBarButtons [cnt].Click ();
						args.RetVal = true;
						break;
					}
				}

			}
		}
	}

	#region Syntax Highlighting Methods
	protected void UpdateSyntaxHighlighting()
	{
		TextTag chordFont = new TextTag (null);
		chordFont.Style = Pango.Style.Italic;
		chordFont.Font.PadRight (5);
		chordFont.ForegroundGdk = new Gdk.Color (130, 130, 130);

		TextTag tagFont = new TextTag (null);
		tagFont.Style = Pango.Style.Italic;
		tagFont.Font.PadRight (5);
		tagFont.ForegroundGdk = new Gdk.Color (0, 100, 0);

		txtEditor.Buffer.TagTable.Add (chordFont);
		txtEditor.Buffer.TagTable.Add (tagFont);

		// Mark all chords
		SetHighlighting (Constants._reChord, chordFont);
		SetHighlighting (Constants._reValueTag, tagFont);
		SetHighlighting (Constants._reSectionTags, tagFont);

	}

	protected void SetHighlighting(Regex re, TextTag textTag)
	{
		string text = txtEditor.Buffer.Text;

		MatchCollection mc = re.Matches (text);
		foreach (Match m in mc)
		{
			TextIter start = txtEditor.Buffer.GetIterAtOffset (m.Index);
			TextIter end = txtEditor.Buffer.GetIterAtOffset (m.Index + m.Groups [0].Length);

			txtEditor.Buffer.ApplyTag (textTag, start, end);
		}
	}
	#endregion

	#endregion

	#region Edit Menu Events
	protected void on_undoAction_Click (object sender, EventArgs e)
	{
		_eventManger.Undo ();
	}


	protected void on_redoAction_Click (object sender, EventArgs e)
	{
		_eventManger.Redo ();
	}

	protected void on_copyAction_Activated (object sender, EventArgs e)
	{
		Gtk.Clipboard clipboard = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));

		Widget widget = this.Focus;

		if (widget.GetType ().IsAssignableFrom (typeof(TextView)))
			clipboard.Text = ((TextView)widget).Buffer.GetSelectedText();
		else if(widget.GetType ().IsAssignableFrom (typeof(Entry))) 
		{
			clipboard.Text = ((Entry)widget).GetSelectedText ();
		}

	}


	protected void on_pasteAction_Activated (object sender, EventArgs e)
	{
		Gtk.Clipboard clipboard = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));
		string text = clipboard.WaitForText ();

		Widget widget = this.Focus;

		if (widget.GetType ().IsAssignableFrom (typeof(TextView)))
		{
			TextView tv = (TextView)widget;
			if (tv.Editable)
			{
				string selectedText = tv.Buffer.GetSelectedText ();
				if (selectedText.Length > 0)
					tv.Buffer.ReplaceSelection (text);
				else
					tv.Buffer.InsertAtCursor (text);
			}
		} else if (widget.GetType ().IsAssignableFrom (typeof(Entry)))
		{
			Entry en = (Entry)widget;
			if (en.IsEditable)
				((Entry)widget).ReplaceSelection (new string (text.Where (c => !char.IsControl (c)).ToArray ()));//.InsertText (text);
		}
	}


	protected void on_cutAction_Activated (object sender, EventArgs e)
	{
		Gtk.Clipboard clipboard = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));

		Widget widget = this.Focus;

		if (widget.GetType ().IsAssignableFrom (typeof(TextView)))
		{
			TextView tv = (TextView)widget;
			clipboard.Text = tv.Buffer.GetSelectedText ();
			if(tv.Editable)
				tv.Buffer.ReplaceSelection ("", false);
		}
		else if(widget.GetType ().IsAssignableFrom (typeof(Entry))) 
		{
			Entry en = (Entry)widget;
			clipboard.Text = en.GetSelectedText ();
			if(en.IsEditable)
				en.ReplaceSelection ("");
		}
	}
	#endregion

	#region Toolbar Events
	protected void on_btnSongInfo_Toggled (object sender, EventArgs e)
	{
		panelSongInfo.Visible = btnShowInfo.Active;
		if (panelSongInfo.Visible)
			btnShowInfo.TooltipText = "Hide Song Information";
		else
			btnShowInfo.TooltipText = "Show Song Information";

		int height = 10;
		if (panelSongInfo.Visible)
			height += panelSongInfo.Allocation.Height;

		panelTagHeader.HeightRequest = height;


		btnShowInfo.TriggerTooltipQuery ();
	}

	protected void on_btnOpen_Clicked (object sender, EventArgs e)
	{
		on_OpenSongFile (sender, e);
	}

	protected void on_btnSave_Clicked (object sender, EventArgs e)
	{
		on_SaveSongFile (sender, e);
	}

	protected void on_btnPrintPreview_Clicked (object sender, EventArgs e)
	{
		RenderSong (RenderOption.PrintPreview);
	}

	protected void on_btnPrint_Clicked (object sender, EventArgs e)
	{
		RenderSong (RenderOption.Print);
	}

	protected void on_btnRedo_Clicked (object sender, EventArgs e)
	{
		_eventManger.Redo ();
	}

	protected void on_btnUndoAction_Clicked (object sender, EventArgs e)
	{
		_eventManger.Undo ();
	}

	protected void on_btnCut_Click (object sender, EventArgs e)
	{
		SetLastWidgetFocus ();
		on_cutAction_Activated (sender, e);
	}

	protected void on_btnCopy_Click (object sender, EventArgs e)
	{
		SetLastWidgetFocus ();
		on_copyAction_Activated (sender, e);
	}

	protected void on_btnPaste_Click (object sender, EventArgs e)
	{
		SetLastWidgetFocus ();
		on_pasteAction_Activated (sender, e);
	}

	protected void on_btnTransposeDown_clicked (object sender, EventArgs e)
	{
		TextIter start;
		TextIter end;
		if (txtEditor.Buffer.GetSelectionBounds (out start, out end))
		{
			// Only transpose the selected text.
			string text = txtEditor.Buffer.GetText (start, end, true);
			text = _currentSong.ContentText = FileOperations.Transpose (text, TransposeDirection.Down, 1);

			txtEditor.Buffer.ReplaceSelection (text);

			_currentSong.ContentText = txtEditor.Buffer.Text;


		} else
		{
			// Do a global transpose.
			_currentSong.ContentText = FileOperations.Transpose (_currentSong.ContentText, TransposeDirection.Down, 1);
			txtEditor.Buffer.Text = _currentSong.ContentText;
		}
	}


	protected void on_btnTransposeUp_clicked (object sender, EventArgs e)
	{
		TextIter start;
		TextIter end;
		if (txtEditor.Buffer.GetSelectionBounds (out start, out end))
		{
			// Only transpose the selected text.
			string text = txtEditor.Buffer.GetText (start, end, true);
			text = _currentSong.ContentText = FileOperations.Transpose (text, TransposeDirection.Up, 1);

			txtEditor.Buffer.ReplaceSelection (text);

			_currentSong.ContentText = txtEditor.Buffer.Text;


		} else
		{
			_currentSong.ContentText = FileOperations.Transpose (_currentSong.ContentText, TransposeDirection.Up, 1);
			txtEditor.Buffer.Text = _currentSong.ContentText;
		}
	}

	protected void on_TagChordBarAction_Toggle (object sender, EventArgs e)
	{
		panelTagBar.Visible = TagChordBarAction.Active;

		if(panelTagBar.Visible)
			btnShowTagBar.TooltipText = "Hide the tag/chord insert bar.";
		else
			btnShowTagBar.TooltipText = "Show the tag/chord insert bar.";
	}

	#endregion
	
	#region Dynamic Status Checks
	protected void CheckStatuses()
	{	
		btnSave.Sensitive = _currentSong.IsModified;
		saveAction.Sensitive = _currentSong.IsModified;
		this.Title = string.Format ("Chord Sheet Generator{0}{1}"
		                            , string.IsNullOrWhiteSpace (_currentSong.Title) ? "" : " - "
		                            , _currentSong.Title);

		on_eventManger_StatusChanged (null, null);

		drawingArea.QueueDraw();
		UpdateTextView ();
	}

	#endregion

	#region Tag/Chord Bar Methods and Events
	protected void CreateTagBarControls()
	{
		for (int i = 0; i < _settings.NumberOfBarOptions; i++)
		{
			HBox hbox = new HBox ();
			hbox.Name = string.Format ("tabHBox{0}", i);
			hbox.Spacing = 6;

			Button btn = new Button ();
			btn.CanFocus = true;
			btn.Name = string.Format ("btnTag{0}", i);
			btn.UseUnderline = true;
			btn.Label = i.ToString ();
			hbox.PackStart (btn, true, true, 0);
			btn.Show ();

			_tagBarButtons.Add (btn);

			Label lbl = new Label ();
			lbl.Name = string.Format ("lblTag{0}", i);
			lbl.LabelProp = string.Format ("Alt-{0}", i);
			hbox.PackStart (lbl, false, false, 0);
			lbl.Show();

			_tagBarLabels.Add (lbl);

			panelTagBar.PackStart (hbox, false, false, 0);
			hbox.Show();

		}

		for (int i=0; i < _tagBarButtons.Count; i++)
		{
			_tagBarButtons [i].Clicked += on_TagBarButtons_Clicked;
		}

		LoadBarOptions ();

	}

	private void LoadBarOptions()
	{
		for (int i = 0; i < _tagBarButtons.Count; i++)
		{
			string btnLabel;
			string lblText;
			if (btnChordBarOption.Active)
			{
				btnLabel = _settings.GetChordBarOption (i);
				lblText = string.Format ("Ctrl+{0}", _settings.GetChordBarKeyMap (i));
			} else
			{
				btnLabel = _settings.GetTagBarOption (i);
				lblText = string.Format ("Alt+{0}", _settings.GetTagBarKeyMap (i));
			}

			string[] split = btnLabel.Split (',');

			if (string.IsNullOrWhiteSpace (split [0]))
			{
				_tagBarButtons [i].Visible = false;
				_tagBarLabels [i].Visible = false;
			}
			else
			{
				_tagBarButtons [i].Label = split [0];
				_tagBarButtons [i].TooltipText = split.Length == 2 ? split [1] : split [0];
				_tagBarButtons [i].Visible = true;

				_tagBarLabels [i].Text = lblText;
				_tagBarLabels [i].Visible = true;
			}
		}


	}



	protected void on_TagBarButtons_Clicked (object sender, EventArgs e)
	{
		//int cursor = txtEditor.Buffer.CursorPosition;
		string textToInsert = ((Button)sender).TooltipText;

		InsertTextClip (textToInsert);
	}


	private void InsertTextClip(string textToInsert)
	{
		int curpos = txtEditor.Buffer.CursorPosition;
		int offset = -1;

		// Check for certain tag types.
		if (Constants._reValueTag.IsMatch(textToInsert))
			offset = textToInsert.Length - 1;

		if (offset < 0)
		{
			int idx = textToInsert.IndexOf ("{", 2);
			if (idx > 0)
				offset = idx - 1;
		}

		string selectedText = txtEditor.Buffer.GetSelectedText ();
		if(selectedText.Length > 0 && offset >= 0)
		{
			// We have a selection to insert.
			textToInsert = string.Format ("{0}{1}{2}", textToInsert.Substring (0, offset), selectedText, textToInsert.Substring (offset));
			offset = textToInsert.Length;
		}

		if (offset < 0)
		{
			offset = textToInsert.Length;
		}

		if (selectedText.Length > 0)
		{
			txtEditor.Buffer.ReplaceSelection (textToInsert, false);
		} else
		{
			txtEditor.Buffer.InsertAtCursor (textToInsert);
		}

		curpos += offset;

		TextIter iter = txtEditor.Buffer.GetIterAtOffset (curpos);
		txtEditor.Buffer.PlaceCursor (iter);
		txtEditor.IsFocus = true;

	}





	protected void on_btnChordBarOption_Clicked (object sender, EventArgs e)
	{
		if (!_updateInProgress)
		{
			_updateInProgress = true;
			btnChordBarOption.Active = true;
			btnTagBarOption.Active = false;

			LoadBarOptions ();
			_updateInProgress = false;
		}
	}

	protected void on_btnTagBarOption_Clicked (object sender, EventArgs e)
	{
		if (!_updateInProgress)
		{
			_updateInProgress = true;
			btnTagBarOption.Active = true;
			btnChordBarOption.Active = false;

			LoadBarOptions ();
			_updateInProgress = false;
		}
	}


	#endregion

	#region Helper Methods
	private void SetSongFields()
	{
		txtTitle.Text = _currentSong.Title;
		txtArtist.Text = _currentSong.Artist;
		txtCopyright.Text = _currentSong.Copyright;
		txtCCLI.Text = _currentSong.CCLI;
		txtKey.Text = _currentSong.Key;
		txtCapo.Text = _currentSong.Capo;
		txtEditor.Buffer.Text = _currentSong.ContentText;

		UpdateSyntaxHighlighting ();
		CheckStatuses ();
	}


	private void SetLastWidgetFocus()
	{
		if (_lastWidget != null)
		{
			this.Focus = _lastWidget;
			if (_lastWidget.GetType ().IsAssignableFrom (typeof(TextView)))
			{
				TextView tv = (TextView)_lastWidget;
				tv.Buffer.SelectRange(tv.Buffer.GetIterAtOffset(_lastStart), tv.Buffer.GetIterAtOffset(_lastEnd));
			} else if (_lastWidget.GetType ().IsAssignableFrom (typeof(Entry)))
			{
				((Entry)_lastWidget).SelectRegion(_lastStart, _lastEnd);
			}
		}
	}


	private void RenderSong(RenderOption option, object o = null)
	{
		try
		{
			CSGen.PageRenderer renderer = new CSGen.PageRenderer (_currentSong, forDisplay: option == RenderOption.ScreenView);
			PageSetup pageSetup;
			DrawingArea area = new DrawingArea ();

			if (option == RenderOption.ScreenView)
			{
				area = (DrawingArea)o;
				pageSetup = renderer.PageSetup;
			} else
			{
				pageSetup = new PageSetup ();
			}

			// Load configuration options.
			if (_workingFonts != null)
				renderer.Fonts = _workingFonts;
			else
				renderer.Fonts = _fonts;
			renderer.AppSettings = _appSettings;

			pageSetup.SetLeftMargin (0.5, Unit.Inch);
			pageSetup.SetTopMargin (0.5, Unit.Inch);
			pageSetup.SetBottomMargin (0.5, Unit.Inch);
			pageSetup.SetRightMargin (0.5, Unit.Inch);

			if (option == RenderOption.ScreenView)
			{
				Cairo.Context cr = Gdk.CairoHelper.Create (area.GdkWindow);
				renderer.CalculatePages (cr);

				area.WidthRequest = renderer.DisplayWidthExtent;
				area.HeightRequest = renderer.DisplayHeightExtent;

				int offset = 0;
				for (int i = 0; i < renderer.Pages; i++)
				{
					renderer.DrawPage (i, offset);
					offset += renderer.DisplayPaperHeight;
				}

				((IDisposable)cr.GetTarget ()).Dispose ();                                      
				((IDisposable)cr).Dispose ();
			} else
			{
				PrintSettings printSettings = new PrintSettings ();
				printSettings.PaperSize = pageSetup.PaperSize;


				PrintOperation print = new PrintOperation ();
				print.DefaultPageSetup = pageSetup;
				print.PrintSettings = printSettings;
				print.BeginPrint += (object obj, BeginPrintArgs args) => {
					renderer.CalculatePages (args.Context.CairoContext);
					print.NPages = renderer.Pages;
				};
				print.DrawPage += (object obj, DrawPageArgs args) => {
					renderer.DrawPage (args.PageNr); };

				if (option == RenderOption.Print)		
					print.Run (PrintOperationAction.PrintDialog, null);
				else
					print.Run (PrintOperationAction.Preview, null);
			}
		} catch (Exception ex)
		{
			MessageDialog dialog = new MessageDialog (
				this, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Ok,
				"Message: {0}\n\n Stacktrace: {1}", ex.Message, ex.StackTrace);
			dialog.Run ();
			dialog.Destroy ();
		}
	}

	private void UpdateTextView()
	{
		StringBuilder text = new StringBuilder ();

		if (!string.IsNullOrEmpty (_currentSong.Title))
		{
			text.AppendLine(_currentSong.Title);
		}

		// Get the main title headings
		if (!string.IsNullOrEmpty (_currentSong.Artist))
		{
			text.AppendLine(_currentSong.Artist);
		}

		if (!string.IsNullOrEmpty (_currentSong.Copyright))
		{
			text.AppendLine(_currentSong.Copyright);
		}

		if (!string.IsNullOrEmpty (_currentSong.CCLI))
		{
			text.AppendFormat("CCLI: {0}", _currentSong.CCLI);
			text.AppendLine ();
		}

//		if (!string.IsNullOrEmpty (_currentSong.Key))
//		{
//			text.AppendFormat("Key: {0}", _currentSong.Key);
//			text.AppendLine ();
//		}
//
//		if (!string.IsNullOrEmpty (_currentSong.Capo))
//		{
//			text.AppendFormat("Capo: {0}", _currentSong.Capo);
//			text.AppendLine ();
//		}


		// Remove all tags except for the comment tag contents.
		string lyrics = _currentSong.ContentText;
		lyrics = Constants._reChord.Replace (lyrics, "");
		lyrics = Constants._reValueTag.Replace (lyrics, "$2");
		lyrics = Constants._reSectionTags.Replace (lyrics, "");

		// Now remove extra spaces
		string[] lyricsLines = lyrics.Split ('\n');
		for (int i = 0; i < lyricsLines.Length; i++)
		{
			if (i > 0)
			{
				if (lyricsLines [i].Trim ().Length == 0 &&
					lyricsLines [i - 1].Trim ().Length == 0)
					continue;
			}

			text.AppendLine (lyricsLines [i]);
		}

		txtTextView.Buffer.Text = text.ToString ();

	}


	#endregion


	void on_drawingArea_Expose (object o, ExposeEventArgs args)
	{
		RenderSong (RenderOption.ScreenView, o);
	}

	

}


