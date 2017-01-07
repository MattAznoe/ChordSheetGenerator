using System;

namespace CSGen.Model
{
	public enum TransposeDirection
	{
		Up = 0,
		Down = 1
	}


	public enum DisplayScaleType
	{
		Scale = 1,
		FitToScreen = 2
	}

	public enum PrintNodeType
	{
		Text = 1,
		Image = 2,
		Line = 3
	}


	public enum PrintFontType
	{
		Title = 0,
		Text = 1,
		Chords = 2,
		SongInfo = 3,
		Comments = 4,
		HeaderFooterText = 5
	}


	public enum RenderOption
	{
		ScreenView = 0,
		PrintPreview = 1,
		Print = 2
	}


	public enum PrintFieldType
	{
		None = 0,
		Title = 1,
		PageNum = 2,
		PageNumOfTotal = 3,
		Artist = 4,
		Copyright = 5,
		CCLI = 6
	}

	public enum PrintLineType
	{
		None = 0,
		Single = 1,
		Double = 2,
		Thick = 3
	}

}

