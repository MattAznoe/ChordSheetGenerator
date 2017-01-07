using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Gtk;


namespace CSGen.Operations
{
	public class EventManager
	{
		#region Private Members
		private enum ActionType
		{
			Insert,
			Delete
		}

		private enum ControlType
		{
			TextView,
			Entry
		}

		private class UIEvent
		{
			public UIEvent(object o, ControlType controlType, ActionType action, string text, int startOffset)
			{
				Control = o;
				Type = controlType;
				Action = action;
				Text = text;
				StartOffset = startOffset;
			}

			public object Control { get; private set; }
			public ControlType Type { get; private set; }
			public ActionType Action { get; private set; }
			public string Text { get; private set; }
			public int StartOffset { get; private set; }

		}

		private class EntryState
		{
			public EntryState(Entry widget)
			{
				EntryWidget = widget;
				Text = widget.Text;
			}

			public Entry EntryWidget { get; set; }
			public string Text { get; set; }

		}

		private List<EntryState> _entryStates = new List<EntryState>();
		private List<Widget> _registeredControls = new List<Widget>();

		private Stack<UIEvent> _undoEvents = new Stack<UIEvent>();
		private Stack<UIEvent> _redoEvents = new Stack<UIEvent>();

		private bool _updateInProgress = false;
		#endregion

		public EventManager ()
		{
		}

		public void RegisterControl(Widget control)
		{
			if (!_registeredControls.Contains (control))
			{
				_registeredControls.Add (control);
				TextView tv = control as TextView;
				if (tv != null)
				{
					tv.Buffer.InsertText += (o, args) => {
						if(!_updateInProgress)
							AddEvent(new UIEvent(o, ControlType.TextView, ActionType.Insert, args.Text, args.Pos.Offset - args.Text.Length));
					};

					tv.Buffer.DeleteRange += new DeleteRangeHandler(SaveTextBufferDeleteAction);
					return;
				}

				Entry en = control as Entry;
				if (en != null)
				{
					_entryStates.Add (new EntryState (en));

					en.KeyPressEvent += on_Entry_KeyPress;

					en.Changed += (object sender, EventArgs e) => {
						if (!_updateInProgress)
						{
							Entry widget = (Entry)sender;
							EntryState state = _entryStates.Find(m => m.EntryWidget == widget);
							if (state != null && state.Text != widget.Text)
							{
								AddEvent(new UIEvent(sender, ControlType.Entry, ActionType.Insert, state.Text, widget.CursorPosition ));
							}
						}
					};
				}
			}
		}

		public bool CanUndo { get { return _undoEvents.Count > 0; } }
		public bool CanRedo { get { return _redoEvents.Count > 0; } }

		public void ClearEvents()
		{
			_undoEvents = new Stack<UIEvent> ();
			_redoEvents = new Stack<UIEvent> ();
			OnStatusChanged (EventArgs.Empty);
		}

		public void Undo()
		{
			if (CanUndo)
			{
				CheckForEntryEvent ();

				UIEvent evnt = _undoEvents.Pop ();
				_redoEvents.Push (evnt);

				_updateInProgress = true;
				switch (evnt.Type)
				{
					case ControlType.TextView:
						{
							TextBuffer tb = (TextBuffer)evnt.Control;
							FocusOnTextView (tb);

							TextIter start = tb.GetIterAtOffset (evnt.StartOffset);
							if (evnt.Action == ActionType.Insert)
							{
								TextIter end = tb.GetIterAtOffset (evnt.StartOffset + evnt.Text.Length);
								tb.Delete (ref start, ref end);
							} else if (evnt.Action == ActionType.Delete)
							{
								tb.Insert (ref start, evnt.Text);
							}
						}
						break;

					case ControlType.Entry:
						{
							Entry en = (Entry)evnt.Control;
							if (en.Text == evnt.Text)
							{
								Undo ();
							} else
							{

								en.GrabFocus ();
								en.Text = evnt.Text;
								en.Position = evnt.StartOffset;
							}
						}
						break;
				}

				_updateInProgress = false;
				OnStatusChanged (EventArgs.Empty);
			}
		}


		public void Redo()
		{
			if (CanRedo)
			{
				UIEvent evnt = _redoEvents.Pop ();
				_undoEvents.Push (evnt);

				_updateInProgress = true;
				switch (evnt.Type)
				{
					case ControlType.TextView:
						{
							TextBuffer tb = (TextBuffer)evnt.Control;
							FocusOnTextView (tb);

							TextIter start = tb.GetIterAtOffset (evnt.StartOffset);
							if (evnt.Action == ActionType.Insert)
							{
								tb.Insert (ref start, evnt.Text);
							} else if (evnt.Action == ActionType.Delete)
							{
								TextIter end = tb.GetIterAtOffset (start.Offset + evnt.Text.Length);
								tb.Delete (ref start, ref end);
							}
						}
						break;

					case ControlType.Entry:
						{
							Entry en = (Entry)evnt.Control;
							if (en.Text == evnt.Text)
							{
								Redo ();
							}
							else
							{
								en.GrabFocus ();
								en.Text = evnt.Text;
								en.Position = evnt.StartOffset;
							}
						}
						break;
				}

				_updateInProgress = false;
				OnStatusChanged (EventArgs.Empty);
			}
		}

		public event EventHandler StatusChanged;

		#region Private Methods
		protected void OnStatusChanged(EventArgs e)
		{
			EventHandler handler = this.StatusChanged;
			if (handler != null)
			{
				handler (this, e);
			}
		}

		[GLib.ConnectBefore]
		private void on_Entry_KeyPress (object o, KeyPressEventArgs args)
		{
			Entry widget = (Entry)o;
			EntryState state = _entryStates.Find(m => m.EntryWidget == widget);
			if (state != null)
			{
				state.Text = widget.Text;
			}
		}

		[GLib.ConnectBefore]
		private void SaveTextBufferDeleteAction(object o, DeleteRangeArgs args)
		{
			if (!_updateInProgress)
			{
				string txt = ((TextBuffer)o).GetText(args.Start, args.End, true);
				AddEvent(new UIEvent(o, ControlType.TextView, ActionType.Delete, txt, args.Start.Offset));
			}
		}



		private void AddEvent(UIEvent evnt)
		{
			CheckForEntryEvent (evnt.Control);
			_undoEvents.Push (evnt);

			// Clear the redo events on each new event.
			_redoEvents = new Stack<UIEvent> ();
			
			OnStatusChanged (EventArgs.Empty);
		}


		private void FocusOnTextView(TextBuffer tb)
		{
			TextView view = _registeredControls.Find (delegate(Widget obj)
            {
				TextView vw = obj as TextView;
				return (vw != null && vw.Buffer == tb);
			}) as TextView;

			if (view != null)
				view.GrabFocus ();
		}

		/// <summary>
		///   Check to see if the last event was an event for an Entry control.  If so, make sure we register
		///   the last update to the control.
		/// </summary>
		private void CheckForEntryEvent(object newObj = null)
		{
			bool needsEvent = false;
			bool addToRedo = false;
			if (_undoEvents.Count > 0)
			{
				UIEvent evnt = _undoEvents.Peek ();
				if (evnt.Type == ControlType.Entry)
				{
					Entry lastObj = (Entry)evnt.Control;
					if (newObj != null && evnt.Control != newObj)
						needsEvent = true;
					else if (newObj == null)
					{
						// This is the case of an undo action.
						if (_redoEvents.Count == 0 && lastObj.Text != evnt.Text)
						{
							needsEvent = true;
							addToRedo = true;
						}
						else if (_redoEvents.Count > 0)
						{
							UIEvent redoEvent = _redoEvents.Peek ();
							if (redoEvent.Control != evnt.Control
							    && lastObj.Text != evnt.Text)
								needsEvent = true;
						}
					}
					if (needsEvent)
					{
						if (addToRedo)
							_redoEvents.Push (new UIEvent (lastObj, ControlType.Entry, ActionType.Insert, lastObj.Text, lastObj.CursorPosition));
						else
							_undoEvents.Push (new UIEvent (lastObj, ControlType.Entry, ActionType.Insert, lastObj.Text, lastObj.CursorPosition));
					}
				}
			}

		}
		#endregion
	}
}

