using System;
using Gtk;
using GLib;

namespace CSGen
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();

			ExceptionManager.UnhandledException += HandleUnhandledException;

			Application.Run ();

		}

		static void HandleUnhandledException (UnhandledExceptionArgs args)
		{
			Exception ex = (Exception)args.ExceptionObject;

			MessageDialog msgDialog = new MessageDialog (
				null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Ok,
				"Message: {0}\n\n Stacktrace: {1}", ex.Message, ex.StackTrace);
			msgDialog.Run ();
			msgDialog.Destroy ();

			args.ExitApplication = true;

		}

	}
}
