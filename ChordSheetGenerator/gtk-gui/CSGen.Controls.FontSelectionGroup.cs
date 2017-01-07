
// This file has been generated by the GUI designer. Do not modify.
namespace CSGen.Controls
{
	public partial class FontSelectionGroup
	{
		private global::Gtk.HBox hbox1;
		private global::Gtk.Label titleLabel;
		private global::Gtk.FontButton fontButton;
		private global::Gtk.Label label3;
		private global::Gtk.SpinButton spacingSpin;
		private global::Gtk.ColorButton colorButton;
		private global::Gtk.CheckButton cb_Underline;
		private global::Gtk.DrawingArea drawingArea;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget CSGen.Controls.FontSelectionGroup
			global::Stetic.BinContainer.Attach (this);
			this.Name = "CSGen.Controls.FontSelectionGroup";
			// Container child CSGen.Controls.FontSelectionGroup.Gtk.Container+ContainerChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.titleLabel = new global::Gtk.Label ();
			this.titleLabel.WidthRequest = 150;
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Xalign = 0F;
			this.titleLabel.LabelProp = global::Mono.Unix.Catalog.GetString ("Song Title:");
			this.hbox1.Add (this.titleLabel);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.titleLabel]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.fontButton = new global::Gtk.FontButton ();
			this.fontButton.WidthRequest = 200;
			this.fontButton.CanFocus = true;
			this.fontButton.Name = "fontButton";
			this.hbox1.Add (this.fontButton);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.fontButton]));
			w2.Position = 1;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.label3 = new global::Gtk.Label ();
			this.label3.Name = "label3";
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString ("Spacing:");
			this.hbox1.Add (this.label3);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.label3]));
			w3.Position = 2;
			w3.Expand = false;
			w3.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.spacingSpin = new global::Gtk.SpinButton (1, 2.5, 0.1);
			this.spacingSpin.CanFocus = true;
			this.spacingSpin.Name = "spacingSpin";
			this.spacingSpin.Adjustment.PageIncrement = 10;
			this.spacingSpin.ClimbRate = 0.1;
			this.spacingSpin.Digits = ((uint)(1));
			this.spacingSpin.Numeric = true;
			this.spacingSpin.Value = 1;
			this.hbox1.Add (this.spacingSpin);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.spacingSpin]));
			w4.Position = 3;
			w4.Expand = false;
			w4.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.colorButton = new global::Gtk.ColorButton ();
			this.colorButton.CanFocus = true;
			this.colorButton.Events = ((global::Gdk.EventMask)(784));
			this.colorButton.Name = "colorButton";
			this.hbox1.Add (this.colorButton);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.colorButton]));
			w5.Position = 4;
			w5.Expand = false;
			w5.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.cb_Underline = new global::Gtk.CheckButton ();
			this.cb_Underline.CanFocus = true;
			this.cb_Underline.Name = "cb_Underline";
			this.cb_Underline.Label = global::Mono.Unix.Catalog.GetString ("Underline");
			this.cb_Underline.DrawIndicator = true;
			this.cb_Underline.UseUnderline = true;
			this.hbox1.Add (this.cb_Underline);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.cb_Underline]));
			w6.Position = 5;
			// Container child hbox1.Gtk.Box+BoxChild
			this.drawingArea = new global::Gtk.DrawingArea ();
			this.drawingArea.WidthRequest = 250;
			this.drawingArea.Name = "drawingArea";
			this.hbox1.Add (this.drawingArea);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.drawingArea]));
			w7.Position = 6;
			w7.Expand = false;
			this.Add (this.hbox1);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();
			this.fontButton.FontSet += new global::System.EventHandler (this.on_Change);
			this.spacingSpin.ValueChanged += new global::System.EventHandler (this.on_Change);
			this.colorButton.ColorSet += new global::System.EventHandler (this.on_Change);
			this.cb_Underline.Toggled += new global::System.EventHandler (this.on_Change);
		}
	}
}
