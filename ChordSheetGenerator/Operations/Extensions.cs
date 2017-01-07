using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Gtk;
using CSGen.Model;

namespace CSGen.Operations
{
	public static class Extensions
	{
		public static int IndexOf (this string[] array, string search, bool ignoreCase = false)
		{
			int idx = 0;
			while (idx < array.Length)
			{
				if (string.Compare (array [idx], search, ignoreCase) == 0)
					return idx;
				idx++;
			}
			return -1;
		}


		public static string GetSelectedText (this Gtk.Entry entry)
		{
			int start;
			int end;
			entry.GetSelectionBounds (out start, out end);
			return entry.Text.Substring (start, end - start);
		}


		public static bool ReplaceSelection (this Gtk.Entry entry, string newText)
		{
			bool retval = false;
			int start;
			int end;
			if (entry.GetSelectionBounds (out start, out end) || start == end)
			{
				entry.Text = string.Format ("{0}{1}{2}", entry.Text.Substring (0, start), newText, entry.Text.Substring (end));
				retval = true;

				entry.Position = start + newText.Length;
			}


			return retval;
		}



		public static string GetSelectedText (this Gtk.TextBuffer buffer)
		{
			TextIter start;
			TextIter end;
			return GetSelectedText (buffer, out start, out end);
		}


		public static string GetSelectedText (this Gtk.TextBuffer buffer, out TextIter start, out TextIter end)
		{
			string retval = string.Empty;
			if (buffer.GetSelectionBounds (out start, out end))
			{
				retval = buffer.GetText (start, end, true);
			}
			return retval;
		}



		public static bool ReplaceSelection (this Gtk.TextBuffer buffer, string newText, bool keepSelection = true)
		{
			bool retval = false;
			TextIter start;
			TextIter end;
			if (buffer.GetSelectionBounds (out start, out end))
			{

				TextMark startMark = buffer.CreateMark ("start", start, false);
				TextMark endMark = buffer.CreateMark ("end", end, true);

				buffer.DeleteSelection (true, true);

				TextIter start2 = buffer.GetIterAtMark (startMark);
				buffer.Insert (ref start2, newText);

				if (keepSelection)
				{
					TextIter start3 = buffer.GetIterAtMark (startMark);
					TextIter end3 = buffer.GetIterAtMark (endMark);

					buffer.SelectRange (start3, end3);
				}
				retval = true;
			}
			return retval;
		}


		/// <summary>
		/// Perform a deep Copy of the object.
		/// </summary>
		/// <typeparam name="T">The type of object being copied.</typeparam>
		/// <param name="source">The object instance to copy.</param>
		/// <returns>The copied object.</returns>
		public static T Clone<T>(this T source)
		{
			if (!typeof(T).IsSerializable)
			{
				throw new ArgumentException("The type must be serializable.", "source");
			}

			// Don't serialize a null object, simply return the default for that object
			if (object.ReferenceEquals(source, null))
			{
				return default(T);
			}

			IFormatter formatter = new BinaryFormatter();
			Stream stream = new MemoryStream();
			using (stream)
			{
				formatter.Serialize(stream, source);
				stream.Seek(0, SeekOrigin.Begin);
				return (T)formatter.Deserialize(stream);
			}
		}


		public static void SetFont(this Cairo.Context cr, FontInfo fontInfo)
		{
			cr.SetSourceRGB (fontInfo.Color.Red, fontInfo.Color.Green, fontInfo.Color.Blue);
			cr.SelectFontFace (fontInfo.Family, fontInfo.Slant, fontInfo.Weight);
			cr.SetFontSize (fontInfo.Scale);
		}
	}



}

