using System;
using System.Collections.Generic;
using System.Reflection;

using Gtk;

namespace CSGen.Operations
{
	public class BindingLib
	{
		#region Private Definitions
		internal class DataSource
		{
			private object _object;
			private PropertyInfo _propInfo;
			private string _propName;

			public DataSource (object obj, string propName)
			{
				_object = obj;
				_propName = propName;

				_propInfo = obj.GetType().GetProperty(propName);
				if(_propInfo == null)
					throw new Exception("Unable to find property name");

			}

			public void UpdateSource(object src)
			{
				_object = src;
			}

			public Type DataType { get { return _propInfo.PropertyType; } }
			public Type SourceType { get { return _object.GetType (); } }
			public string Key
			{
				get { return string.Format ("{0}-{1}", _object.GetType ().ToString (), _propName);}
			}

			public object Value
			{
				get { return _propInfo.GetValue (_object, null); }
				set { _propInfo.SetValue (_object, value, null); }
			}

		}

		private enum ObjectType
		{
			None = 0,
			Entry = 1,
			TextView = 2,
			ToggleButton = 3,
			ToggleAction = 4
		}

		private class BindingDef
		{
			public BindingDef(DataSource src, GLib.Object control, ObjectType type)
			{
				DataSource = src;
				Control = control;
				WidgetType = type;
			}

			public DataSource DataSource {get; private set;}
			public GLib.Object Control { get; private set; }
			public ObjectType WidgetType { get; private set; }
		}
		#endregion

		private bool _updateInProgress = false;
		private List<BindingDef> _bindings = new List<BindingDef>();

		public void AddBinding(GLib.Object control, object dataSource, string propertyName)
		{
			// Get the type of control being bound.
			ObjectType widgetType = ObjectType.None;
			Type ctrlType = control.GetType ();

			if (IsSameOrSubclass (ctrlType, typeof(Entry)))
				widgetType = ObjectType.Entry;
			else if (IsSameOrSubclass (ctrlType, typeof(TextView)))
				widgetType = ObjectType.TextView;
			else if (IsSameOrSubclass (ctrlType, typeof(ToggleButton)))
				widgetType = ObjectType.ToggleButton;
			else if (IsSameOrSubclass (ctrlType, typeof(ToggleAction)))
				widgetType = ObjectType.ToggleAction;

			// Get the datasource and ensure that the types are compatible.
			DataSource src = new DataSource (dataSource, propertyName);
			bool typeIsValid = false;

			switch (widgetType)
			{
				case ObjectType.Entry:
				case ObjectType.TextView:
					if (src.DataType == typeof(string))
						typeIsValid = true;
					break;

				case ObjectType.ToggleButton:
				case ObjectType.ToggleAction:
					if (src.DataType == typeof(bool))
						typeIsValid = true;
					break;
			}

			if (!typeIsValid)
			{
				string name = "unknown";
				if (IsSameOrSubclass(ctrlType, typeof(Gtk.Action)))
				    name = ((Gtk.Action)control).Name;
				else if (IsSameOrSubclass(ctrlType, typeof(Gtk.Widget)))
				    name = ((Gtk.Widget)control).Name;

				throw new Exception (string.Format ("The binding for control '{0}' to '{1}' failed: incompatible data types.", name, propertyName));
			}

			// Add an event handler for the control.
			switch (widgetType)
			{
				case ObjectType.Entry:
					((Entry)control).Changed += (object sender, EventArgs e) => {
						if (!_updateInProgress)
						{
							BindingDef def = _bindings.Find (x => x.Control == sender);
							if (def.DataSource.DataType == typeof(string))
								def.DataSource.Value = ((Entry)sender).Text;

							UpdateBindings (def.DataSource.Key, sender);
						}
					};
					break;

				case ObjectType.ToggleButton:
					((ToggleButton)control).Toggled += (object sender, EventArgs e) => {
						if (!_updateInProgress)
						{
							BindingDef def = _bindings.Find (x => x.Control == sender);
							def.DataSource.Value = ((ToggleButton)sender).Active;

							UpdateBindings (def.DataSource.Key, sender);
						}
					};
					break;

				case ObjectType.ToggleAction:
					((ToggleAction)control).Toggled += (object sender, EventArgs e) => {
						if (!_updateInProgress)
						{
							BindingDef def = _bindings.Find (x => x.Control == sender);
							def.DataSource.Value = ((ToggleAction)sender).Active;

							UpdateBindings (def.DataSource.Key, sender);
						}
					};
					break;

			}

			_bindings.Add (new BindingDef(src, control, widgetType));
		}

		public void UpdateBindings()
		{
			UpdateBindings (_bindings);
		}

		public void UpdateBindings(object dataSource)
		{
			List<BindingDef> defs = _bindings.FindAll (x => x.DataSource.SourceType == dataSource.GetType());
			foreach (BindingDef def in defs)
			{
				def.DataSource.UpdateSource (dataSource);
			}
			UpdateBindings (defs);
		}

		private void UpdateBindings(string key, object skipControl = null)
		{
			List<BindingDef> defs = _bindings.FindAll (x => x.DataSource.Key == key);
			UpdateBindings(defs);
		}


		private void UpdateBindings(List<BindingDef> defs, object skipControl = null)
		{
			_updateInProgress = true;
			foreach (BindingDef def in defs)
			{
				if (skipControl != null && def.Control == skipControl)
					continue;

				switch (def.WidgetType)
				{
					case ObjectType.Entry:
						((Entry)def.Control).Text = def.DataSource.Value.ToString ();
						break;
					case ObjectType.TextView:
						((TextView)def.Control).Buffer.Text = def.DataSource.Value.ToString ();
						break;
					case ObjectType.ToggleButton:
						((ToggleButton)def.Control).Active = (bool)def.DataSource.Value;
						break;
					case ObjectType.ToggleAction:
						((ToggleAction)def.Control).Active = (bool)def.DataSource.Value;
						break;
				}
			}
			_updateInProgress = false;
		}



		private bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant)
		{
			return potentialDescendant.IsSubclassOf(potentialBase)
				|| potentialDescendant == potentialBase;
		}
	}


//	public static class BindingExtensions
//	{
//		public static void AddBinding(this Gtk.Entry ctrl, object dataSource, string propertyName)
//		{
//			BindingLib.Bindings.AddBinding ();
//
//		}
//
//
//	}
}

