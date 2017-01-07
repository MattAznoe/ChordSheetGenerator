using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Gtk;
using Cairo;
using CSGen.Model;
using CSGen.Operations;

namespace CSGen
{
	public class PageRenderer
	{
		#region Constants
		private static int X_DISPLAY_MARGIN = 10;
		private static int Y_DISPLAY_MARGIN = 10;
		private static int SHADOW_WIDTH = 4; // included in the margin.
		#endregion

		#region Private Members
		// Configuration
		private SongData _song;
		private PageSetup _pageSetup = new PageSetup();
		private DisplayScaleType _scaleType = DisplayScaleType.Scale;
		private int _scaleParam = 0;
		private bool _forDisplay = true;
		private FontInfo[] _fonts;

		// Calculated dimensions - altered by the scale (if applicable)
		private int _paperHeight = 0;
		private int _paperWidth = 0;
		private int _pageHeight = 0;
		private int _pageWidth = 0;
		private int _topMargin = 0;
		private int _leftMargin = 0;
		private double _scale;

		// Calculated dimensions for display only
		private int _displayWidthExtent = 0;
		private int _displayHeightExtent = 0;

		// Nodes and pages
		private Cairo.Context _cr;
		private List<Page> _pages = new List<Page>();
		private Page _curPage;
		private double _curPos;
		private double _curPageMax;
		#endregion

		public PageRenderer (SongData song, bool forDisplay = true, DisplayScaleType scaleType = DisplayScaleType.Scale, int scaleParam = 100)
		{
			_song = song;

			_forDisplay = forDisplay;
			_scaleType = scaleType;
			_scaleParam = scaleParam;
		}

		#region Public Properties
		public FontInfo[] Fonts
		{
			get
			{
				return _fonts;
			}
			set 
			{
				_fonts = value;
			}
		}

		public PageSetup PageSetup { get { return _pageSetup; } }
		public AppSettings AppSettings { get; set; }

		public int Pages { get { return _pages.Count; } }

		/// <summary>
		///  For display rendering only
		/// </summary>
		/// <value>The height of the page.</value>
		public int DisplayPaperHeight { get { return _paperHeight + Y_DISPLAY_MARGIN; } }


		public int ActualPaperHeight { get { return _paperHeight; } }

		/// <summary>
		///  For display rendering only
		/// </summary>
		/// <value>The height of the max.</value>
		public int DisplayHeightExtent { get { return _displayHeightExtent; } }

		public int DisplayWidthExtent { get { return _displayWidthExtent; } }

		#endregion

		public int CalculatePages(Cairo.Context cr)
		{
			// Initialize settings
			_cr = cr;
			_pages.Clear ();

			#region Page Setup
			double actualPaperHeight = _pageSetup.GetPaperHeight(Unit.Pixel);
			double actualPaperWidth = _pageSetup.GetPaperWidth(Unit.Pixel);
			double actualPageHeight = _pageSetup.GetPageHeight (Unit.Pixel);
			double actualPageWidth = _pageSetup.GetPageWidth (Unit.Pixel);
			double actualTopMargin = _pageSetup.GetTopMargin (Unit.Pixel);
			double actualLeftMargin = _pageSetup.GetLeftMargin (Unit.Pixel);

			// Determine the actual scale to use.
			if (_forDisplay)
			{
				if (_scaleType == DisplayScaleType.Scale)
				{
					_scale = (double)_scaleParam / 100.0;

					_paperHeight = Convert.ToInt32((long)actualPaperHeight);
					_paperWidth = Convert.ToInt32((long)actualPaperWidth);
					_pageHeight = Convert.ToInt32((long)actualPageHeight);
					_pageWidth = Convert.ToInt32 ((long)actualPageWidth);
					_topMargin = Convert.ToInt32 ((long)actualTopMargin);
					_leftMargin = Convert.ToInt32 ((long)actualLeftMargin);
				}
			} else
			{
				_paperHeight = Convert.ToInt32((long)actualPaperHeight);
				_paperWidth = Convert.ToInt32((long)actualPaperWidth);
				_pageHeight = Convert.ToInt32((long)actualPageHeight);
				_pageWidth = Convert.ToInt32 ((long)actualPageWidth);
				_topMargin = Convert.ToInt32 ((long)actualTopMargin);
				_leftMargin = Convert.ToInt32 ((long)actualLeftMargin);
			}
			#endregion

			// Generate the pages and print nodes.
			StartNewPage ();

			#region Standard Song Header
			// Generate the standard song header (title and info)
			if (!string.IsNullOrEmpty (_song.Title))
			{
				AddElement (PrintFontType.Title, _song.Title, newLine: true);
			}

			// Get the artist
			if (!string.IsNullOrEmpty (_song.Artist))
			{
				AddElement (PrintFontType.SongInfo, _song.Artist, newLine: true);
			}

			if (!string.IsNullOrEmpty (_song.Copyright))
			{
				AddElement (PrintFontType.SongInfo, _song.Copyright, newLine: true);
			}

			if (!string.IsNullOrEmpty (_song.CCLI))
			{
				AddElement (PrintFontType.SongInfo, string.Format("CCLI: {0}", _song.CCLI), newLine: true);
			}

			if (!string.IsNullOrEmpty (_song.Key))
			{
				AddElement (PrintFontType.SongInfo, string.Format("Key: {0}", _song.Key), newLine: true);
			}

			if (!string.IsNullOrEmpty (_song.Capo))
			{
				AddElement (PrintFontType.SongInfo, string.Format("Capo: {0}", _song.Capo), newLine: true);
			}
			#endregion

			string[] lines = _song.ContentText.Split ('\n');

			for (int i = 0; i < lines.Length; i++)
			{
				ProcessSongLine (lines [i]);
			}

			// Now that we know how many pages we need, determine the extent required to display it.
			if (_forDisplay)
			{
				_displayHeightExtent = ((Y_DISPLAY_MARGIN + _paperHeight) * _pages.Count) + Y_DISPLAY_MARGIN;
				_displayWidthExtent = (X_DISPLAY_MARGIN * 2) + _paperWidth;
			}

			return _pages.Count;

		}





		private void StartNewPage()
		{
			_curPage = new Page (_pages.Count + 1);
			_pages.Add (_curPage);

			// TODO: add headers and footers

			if (_forDisplay)
			{
				_curPos = ((Y_DISPLAY_MARGIN + _paperHeight) * (_curPage.PageNumber - 1)) + _topMargin;
				_curPageMax = _curPos + _pageHeight;
			} else
			{
				_curPos = _topMargin;
				_curPageMax = _pageHeight;
			}

			#region Header and Footer
			if(_song.HasData)
			{

				string textLeft;
				string textCenter;
				string textRight;

				FontInfo font = _fonts [(int)PrintFontType.HeaderFooterText];
				_cr.SetFont(font);

				Double savePosition = _curPos;

				if (AppSettings.ShowHeader)
				{
					textLeft = GetFieldText(AppSettings.HeaderFieldLeft);
					textCenter = GetFieldText(AppSettings.HeaderFieldCenter);
					textRight = GetFieldText(AppSettings.HeaderFieldRight);

					double lineHeight = RenderHeaderFooter(font, textLeft, textCenter, textRight);
					lineHeight = font.LineSpacing * lineHeight;
					DrawLine(AppSettings.HeaderLineType);

					savePosition = _curPos;

				}
				if (AppSettings.ShowFooter)
				{
					textLeft = GetFieldText(AppSettings.FooterFieldLeft);
					textCenter = GetFieldText(AppSettings.FooterFieldCenter);
					textRight = GetFieldText(AppSettings.FooterFieldRight);

					// Calculate the height of the footer
					double lineHeight = RenderHeaderFooter(font, textLeft, textCenter, textRight, calcHeightOnly: true);
					lineHeight = font.LineSpacing * lineHeight;
					lineHeight += DrawLine(AppSettings.FooterLineType, calcHeightOnly: true);

					// Render the footer
					_curPos = _curPageMax - lineHeight;
					DrawLine(AppSettings.FooterLineType);
					RenderHeaderFooter(font, textLeft, textCenter, textRight);

					//_curPageMax -= (lineHeight * 1.5);
					_curPageMax -= lineHeight;

					_curPos = savePosition;
				}
			}
			#endregion Header and Footer
		}

		private double DrawLine(PrintLineType lineType, bool calcHeightOnly = false)
		{
			double startPos = _curPos;
			LineNode line;
			_curPos += 5;
			switch (lineType)
			{
				case PrintLineType.Single:
					line = new LineNode(_curPos, _leftMargin, _pageWidth, 0.5);
					if(!calcHeightOnly)
						_curPage.Nodes.Add(line);
					_curPos += 7;
					break;
				case PrintLineType.Double:
					line = new LineNode (_curPos, _leftMargin, _pageWidth, 0.5);
					if(!calcHeightOnly)
						_curPage.Nodes.Add (line);
					_curPos += 3;
					line = new LineNode(_curPos, _leftMargin, _pageWidth, 0.5);
					if(!calcHeightOnly)
						_curPage.Nodes.Add(line);
					_curPos += 7;
					break;
				case PrintLineType.Thick:
					line = new LineNode (_curPos, _leftMargin, _pageWidth, 2);
					if(!calcHeightOnly)
						_curPage.Nodes.Add(line);
					_curPos += 9;
					break;				
				case PrintLineType.None:
					break;
			}
			_curPos += 5;

			if (calcHeightOnly)
			{
				double retval = _curPos - startPos;
				_curPos = startPos;
				return retval;
			} else
			{
				return _curPos - startPos;
			}
		}


		private double RenderHeaderFooter(FontInfo font, string textLeft, string textCenter, string textRight, bool calcHeightOnly = false)
		{
			TextExtents extentsLeft = _cr.TextExtents (textLeft);
			TextExtents extentsCenter = _cr.TextExtents (textCenter);
			TextExtents extentsRight = _cr.TextExtents (textRight);

			double lineHeight = Math.Max(Math.Max(extentsLeft.Height, extentsRight.Height), extentsCenter.Height);

			if (!calcHeightOnly)
			{
				if (extentsLeft.Width > 0)
				{
					AddElement (font, textLeft, paramNode: true);
				}
				if (extentsCenter.Width > 0)
				{
					AddElement (font, textCenter, false, (_pageWidth - extentsCenter.Width) / 2.0, paramNode: true);
				}
				if (extentsRight.Width > 0)
				{
					AddElement (font, textRight, false, _pageWidth - extentsRight.Width, paramNode: true);
				}
			}
			return lineHeight;
		}


		private string GetFieldText(PrintFieldType fieldType)
		{
			switch (fieldType)
			{
				case PrintFieldType.Title:
					return _song.Title;
				case PrintFieldType.Artist:
					return _song.Artist;
				case PrintFieldType.CCLI:
					return _song.CCLI;
				case PrintFieldType.Copyright:
					return _song.Copyright;
				case PrintFieldType.PageNum:
					return "#PageNum"; //_pages.Count.ToString();
				case PrintFieldType.PageNumOfTotal:
					return "#PageNum of #PageTotal"; //, _pages.Count, _totalPageCount);
				default:
					return string.Empty;
			}
		}


		private TextExtents AddElement(PrintFontType fontType, string text, bool newLine = false, double _left = 0.0)
		{
			FontInfo font = _fonts [(int)fontType];
			_cr.SetFont (font);

			return AddElement (font, text, newLine, _left);
		}

		private TextExtents AddElement(FontInfo font, string text, bool newLine = false, double _left = 0.0, bool paramNode = false)
		{
			TextExtents extents = _cr.TextExtents (text);

			if (_curPos + extents.Height > _curPageMax)
				StartNewPage ();

			if (font.Underline)
				extents.Height += 2.0 + font.UnderlineHeight;

			if (newLine)
				_curPos += extents.Height;

			if(paramNode)
				_curPage.Nodes.Add(new TextParameterNode (_curPos, _leftMargin + _left, font, text));
			else
				_curPage.Nodes.Add(new TextNode (_curPos, _leftMargin + _left, font, text));

			if (newLine)
				_curPos += (font.LineSpacing - 1) * extents.Height;

			return extents;
		}

		private void ProcessSongLine(string line)
		{
			TextExtents extents;
			FontInfo font;

			if (string.IsNullOrWhiteSpace (line))
			{
				font = _fonts [(int)PrintFontType.Text];
				_cr.SetFont (font);
				extents = _cr.TextExtents ("S");

				_curPos += extents.Height;

				if (_curPos > _curPageMax)
					StartNewPage ();
			} else
			{
				List<TextNode> chordList = new List<TextNode> ();
				List<TextNode> textList = new List<TextNode> ();
				double topLineSpacing = 0.0;

				MatchCollection matches = Constants._reSectionTags.Matches (line);
				if (matches.Count > 0)
				{
					// For now we will just ignore them.
					string tag = matches [0].Groups [1].Value;
					switch (tag.ToLower ())
					{
						case "new_page":
						case "np":
						case "npp":
							StartNewPage ();
							break;
					}

					line = line.Substring (matches [0].Length);
				}


				// Enter value tags (comments, etc.)
				matches = Constants._reValueTag.Matches (line);
				if (matches.Count > 0)
				{
					string tag = matches [0].Groups[1].Value;
					string text = matches [0].Groups [2].Value;

					TextNode tagNode;
					switch (tag.ToLower())
					{
						case "c":
						case "cl":
						case "comment":
							font = _fonts [(int)PrintFontType.Comments];
							_cr.SetFont (font);

							tagNode = new TextNode (text);
							tagNode.Font = font;
							extents = _cr.TextExtents (text);
							extents.Height += tagNode.ULOffset;
							tagNode._Width = extents.Width;
							tagNode._Height = extents.Height;

							topLineSpacing = font.LineSpacing;

							chordList.Add (tagNode);
							textList.Add (new TextNode (""));
							break;
					}

					line = line.Substring (matches [0].Length);
				}

				// Now, see if we have any chords to process. 
				matches = Constants._reChord.Matches (line);
				if (matches.Count > 0)
				{
					int txtPos = 0;
					foreach (Match match in matches)
					{
						string tmpt = line.Substring (txtPos, match.Index - txtPos);
						textList.Add (new TextNode (tmpt));
						string tmp = string.Format ("{0}{1}{2}", match.Groups [1], match.Groups [2], match.Groups [4]);
						chordList.Add (new TextNode (tmp));

						txtPos = match.Index + match.Length;
					}
					textList.Add (new TextNode (line.Substring (txtPos)));
				}


				if (chordList.Count > 0)
				{
					// Go through each list and determine the extents
					double chordHeight = 0.0;  // Height and offset from curpos for the chord line
					double textHeight = 0.0;
					double totalHeight = 0.0; 
					double textOffset = 0.0;   // Offset from curpos for the text line.
					double chordSpacer = 0.0;
					double textSpacer = 0.0;
					double textSpacerSmall = 0.0;
					double textDash = 0.0;

					#region Determine element height and width
					font = _fonts [(int)PrintFontType.Chords];
					_cr.SetFont (font);

					// Get the spacer for chords
					extents = _cr.TextExtents ("_");
					chordSpacer = extents.Width;

					// Process the chord nodes
					foreach (TextNode node in chordList)
					{
						// If we have not determined the width already (for a tag), then determine it now.
						if(node._Width == 0)
						{
							extents = _cr.TextExtents (node.Text);
							node.Font = font;
							extents.Height += node.ULOffset;
							node._Width = extents.Width;
							node._Height = extents.Height;
						}
						chordHeight = Math.Max (chordHeight, node._Height);
					}

					totalHeight += chordHeight + ((Math.Max(font.LineSpacing, topLineSpacing) - 1) * chordHeight);

					font = _fonts [(int)PrintFontType.Text];
					_cr.SetFont (font);

					// Get the spacer for text.
					extents = _cr.TextExtents ("_");
					textSpacer = extents.Width;
					textSpacerSmall = textSpacer / 4.0;

					extents = _cr.TextExtents ("-");
					textDash = extents.Width;

					foreach (TextNode node in textList)
					{
						if (!string.IsNullOrEmpty(node.Text))
						{
							extents = _cr.TextExtents (node.Text);
							node.Font = font;
							node._Width = extents.Width;
							extents.Height += node.ULOffset;

							if(node.Text.EndsWith(" "))
								node._Width += textSpacer;

							textHeight = Math.Max (textHeight, extents.Height);
						}
					}

					textOffset = totalHeight + textHeight;
					totalHeight += textHeight + ((font.LineSpacing - 1) * textHeight);

					// Check to see if we have scrolled over onto a new page.
					if (_curPos + totalHeight > _curPageMax)
						StartNewPage ();
					#endregion

					double curLeft = _leftMargin;
					for (int i =0; i < textList.Count; i++)
					{
						// Add the text (if we have any)
						if (!string.IsNullOrWhiteSpace (textList [i].Text))
						{
							textList [i].Left = curLeft;
							textList [i].Bottom = _curPos + textOffset;
							_curPage.Nodes.Add (textList [i]);

							if (i > 0 && ((chordList [i - 1]._Width + chordSpacer) > textList [i]._Width))
							{
								curLeft += chordList [i - 1]._Width + chordSpacer;
								if (!textList [i].Text.EndsWith (" ") 
								    && i < textList.Count - 1)  // Don't worry if we are at the end of the line.
								{
									double midPoint = ((curLeft - (textList [i].Left + textList [i]._Width) - textDash) / 2);
									midPoint += textList [i].Left + textList [i]._Width;
									TextNode spacerNode = new TextNode (textList [i].Bottom, midPoint, textList [i].Font, "-");
									_curPage.Nodes.Add (spacerNode);
								}
							} else
							{
								curLeft += textList [i]._Width;
								if (!textList [i].Text.EndsWith (" "))
									curLeft += textSpacerSmall;
								else
									curLeft += textSpacer;
							}
						} else if(i > 0)
						{
							curLeft += chordList [i - 1]._Width + chordSpacer;
						}

						if (i < chordList.Count)
						{
							chordList [i].Left = curLeft;
							chordList [i].Bottom = _curPos + chordHeight;
							_curPage.Nodes.Add (chordList [i]);
						}
					}

					_curPos += totalHeight;

				} else
				{
					AddElement (PrintFontType.Text, line, true);
				}

			}
		}




		/// <summary>
		///   Render the actual print nodes to the page or screen.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="offset">Offset.</param>
		public void DrawPage (int index, int offset = 0)
		{
			if (_forDisplay)
			{
				// Create the shadow effect
				_cr.SetSourceRGBA (0.5, 0.5, 0.5, 0.5);
				_cr.Rectangle (X_DISPLAY_MARGIN + SHADOW_WIDTH
				               , Y_DISPLAY_MARGIN + SHADOW_WIDTH + offset
				               , _paperWidth
				               , _paperHeight);
				_cr.Fill ();



				_cr.SetSourceRGB (1, 1, 1);
				_cr.Rectangle (X_DISPLAY_MARGIN
				               , Y_DISPLAY_MARGIN + offset
				               , _paperWidth
				               , _paperHeight);
				_cr.Fill ();
			}

			_cr.SetSourceRGB(0.1, 0.1, 0.1);

			foreach (PrintNode node in _pages[index].Nodes)
			{
				if (node is TextParameterNode)
				{
					((TextParameterNode)node).PageNumber = index + 1;
					((TextParameterNode)node).PageTotal = _pages.Count;
				}

				node.Render (_cr);

			}
		}

	}
}

