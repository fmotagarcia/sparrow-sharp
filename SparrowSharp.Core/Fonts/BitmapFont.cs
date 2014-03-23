using System;
using System.Collections.Generic;
using Sparrow.Textures;
using System.Xml;
using System.IO;
using Sparrow.Geom;
using Sparrow.Utils;
using Sparrow.Display;
using SparrowSharp.Core;
using Sparrow.ResourceLoading;

namespace Sparrow.Fonts
{
	public class BitmapFont
	{
		private const uint MAX_TEXT_CHAR_COUNT = 8192;
		private readonly Image _helperImage;
		private readonly IAssetManager _assetManager;
		private readonly Texture _fontTexture;
		private readonly Dictionary<int, BitmapChar> _chars;
		private string _name;
		private float _size;
		private float _lineHeight;
		private float _baseline;

		public TextureSmoothing Smoothing
		{
			get { return _fontTexture.Smoothing; }
			private set { _fontTexture.Smoothing = value; }
		}

		public BitmapFont(TextureLoader textureLoader)
		{
			// TODO: load external font
			byte[] fontTextureData = Convert.FromBase64String(MiniFontImageDataBase64);
			byte[] fontXmlData = Convert.FromBase64String(MiniFontXmlDataBase64);

			_name = @"unknown";
			_lineHeight = _size = _baseline = 10f; // FIXME
			_chars = new Dictionary<int,BitmapChar>();

			_fontTexture = TextureReferencedByXmlData(textureLoader, new MemoryStream(fontXmlData));
			_helperImage = new Image(_fontTexture);
		}

		public void FillQuadBatch(QuadBatch quadBatch, float width, float height, string text, float size, uint color, HAlign hAlign, VAlign vAlign, bool autoScale, bool kerning)
		{
			List<CharLocation> charLocations = ArrangeCharsInArea(width, height, text, size, hAlign, vAlign, autoScale, kerning);
			_helperImage.Color = color;

			if (charLocations.Count > MAX_TEXT_CHAR_COUNT)
			{
				throw new InvalidDataException(string.Format("Bitmap font text is limited to {0} characters", MAX_TEXT_CHAR_COUNT));
			}

			CharLocation charLocation;
			for (int i = 0; i < charLocations.Count; i++)
			{
				charLocation = charLocations[i];
				_helperImage.Texture = charLocation.BitmapChar.Texture;
				_helperImage.X = charLocation.X;
				_helperImage.Y = charLocation.Y;
				_helperImage.ScaleX = _helperImage.ScaleY = charLocation.Scale;
				_helperImage.ReadjustSize();
				quadBatch.AddQuad(_helperImage);
			}

		}

		private Texture TextureReferencedByXmlData(TextureLoader textureLoader, Stream fontXmlData)
		{
			XmlDocument xml = new XmlDocument();
			xml.Load(fontXmlData);

			Texture texture = null;
			XmlNodeList pageNodes = xml.GetElementsByTagName("page");
			for (int i = 0; i < pageNodes.Count; i++)
			{
				XmlAttributeCollection attributes = pageNodes[i].Attributes;
				int id = Convert.ToInt32(attributes["id"]);
				if (id != 0)
				{
					throw new Exception("Bitmap fonts with multiple pages are not supported");
				}

				string filename = attributes["file"].Value;
				string absolutePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Resources), filename); // NSBundle.MainBundle.BundlePath
				texture = textureLoader.LoadLocalImage(absolutePath);
			}

			if (texture == null)
			{
				throw new InvalidDataException("Font XML doesn't contain path to texture");
			}

			return texture;
		}

		private bool ParseFontData(Stream data)
		{
			if (_fontTexture == null)
			{
				throw new InvalidOperationException("Font parsing requires texture to be set");
			}

			XmlDocument xml = new XmlDocument();
			xml.Load(data);

			float scale = _fontTexture.Scale;
			ParseAndLoadChars(xml, scale);
			ParseAndLoadKerning(xml, scale);
			ParseAndLoadInfo(xml);
			ParseAndLoadCommon(xml);

			return false;
		}

		private void ParseAndLoadChars(XmlDocument xml, float scale)
		{
			XmlNodeList charNodes = xml.GetElementsByTagName("char");
			for (int i = 0; i < charNodes.Count; i++)
			{
				XmlAttributeCollection attributes = charNodes[i].Attributes;

				float x = Convert.ToSingle(attributes["x"].Value);
				float y = Convert.ToSingle(attributes["y"].Value);
				float width = Convert.ToSingle(attributes["width"].Value);
				float height = Convert.ToSingle(attributes["height"].Value);

				Rectangle region = new Rectangle(x / scale + _fontTexture.Frame.X, y / scale + _fontTexture.Frame.X, width / scale, height / scale);
				SubTexture texture = new SubTexture(region, _fontTexture);

				int charId = Convert.ToInt32(attributes["id"].Value);
				float xOffset = Convert.ToSingle(attributes["xoffset"].Value);
				float yOffset = Convert.ToSingle(attributes["yoffset"].Value);
				float xAdvance = Convert.ToSingle(attributes["xadvance"].Value);

				BitmapChar bitmapChar = new BitmapChar(charId, texture, xOffset, yOffset, xAdvance);
				_chars.Add(charId, bitmapChar);
			}
		}

		private void ParseAndLoadKerning(XmlDocument xml, float scale)
		{
			XmlNodeList kerningNodes = xml.GetElementsByTagName("kerning");
			for (int i = 0; i < kerningNodes.Count; i++)
			{
				XmlAttributeCollection attributes = kerningNodes[i].Attributes;

				int first = Convert.ToInt32(attributes["first"].Value);
				int second = Convert.ToInt32(attributes["second"].Value);
				float amount = Convert.ToSingle(attributes["ammount"].Value) / scale;

				_chars[second].AddKerning(amount, first);
			}
		}

		private void ParseAndLoadInfo(XmlDocument xml)
		{
			XmlNodeList infoNodes = xml.GetElementsByTagName("info");
			if (infoNodes.Count > 0)
			{
				XmlAttributeCollection attributes = infoNodes[0].Attributes;

				_name = attributes["face"].Value;
				_size = Convert.ToSingle(attributes["size"].Value);

				if (attributes["smooth"].Value == "0")
				{
					Smoothing = TextureSmoothing.None;
				}
			}
		}

		private void ParseAndLoadCommon(XmlDocument xml)
		{
			XmlNodeList commonNode = xml.GetElementsByTagName("common");
			if (commonNode.Count > 0)
			{
				XmlAttributeCollection attributes = commonNode[0].Attributes;
				_lineHeight = Convert.ToSingle(attributes["lineHeight"]);
				_baseline = Convert.ToSingle(attributes["base"]);
			}
		}

		private BitmapChar CharById(int charId)
		{
			return _chars[charId];
		}

		private List<CharLocation> ArrangeCharsInArea(float width, float height, string text, float size, HAlign hAlign, VAlign vAlign, bool autoScale, bool kerning)
		{
			if (text.Length == 0)
			{
				return new List<CharLocation>();
			}

			if (size < 0)
			{
				size *= -_size;
			}

			bool isFinished = false;
			float scale = 0;
			float containerWidth = 0;
			float containerHeight = 0;

			List<List<CharLocation>> lines = new List<List<CharLocation>>();
			while (!isFinished)
			{
				scale = size / _size;
				containerWidth = width / scale;
				containerHeight = height / scale;

				if (_lineHeight <= containerHeight)
				{
					int lastWhiteSpace = -1;
					int lastCharId = -1;
					int numChars = text.Length;
					float currentX = 0;
					float currentY = 0;

					List<CharLocation> currentLine = new List<CharLocation>();
					for (int i = 0; i < numChars; i++)
					{
						bool isLineFull = false;
						int charId = text[i];
						BitmapChar bitmapChar = CharById(charId);

						if (charId == NewLineAsciiCode || charId == CarriageReturnAsciiCode)
						{
							isLineFull = true;
						}
						else if (bitmapChar == null)
						{
							Console.WriteLine(@"Missing character: %d", charId);
						}
						else
						{
							if (charId == SpaceAsciiCode || charId == TabAsciiCode)
							{
								lastWhiteSpace = i;
							}
								
							if (kerning)
							{
								currentX += bitmapChar.KerningToChar(lastCharId);
							}

							CharLocation charLocation = CharLocation.Create(bitmapChar, currentX + bitmapChar.XOffset, currentY + bitmapChar.YOffset);
							currentLine.Add(charLocation);
							currentX += bitmapChar.XAdvance;
							lastCharId = charId;

							if (charLocation.X + bitmapChar.Width > containerWidth)
							{
								int numCharsToRemove = (lastWhiteSpace == -1) ? 1 : i - lastWhiteSpace;
								int removeIndex = currentLine.Count - numCharsToRemove;
								currentLine.RemoveRange(removeIndex, numCharsToRemove);

								if (currentLine.Count == 0)
								{
									break;
								}

								i -= numCharsToRemove;
								isLineFull = true;
							}
						}

						if (i == numChars - 1)
						{
							lines.Add(currentLine);
							isFinished = true;
						}
						else if (isLineFull)
						{
							lines.Add(currentLine);

							if (lastWhiteSpace == i)
							{
								currentLine.RemoveAt(currentLine.Count - 1);
							}

							if (currentY + 2 * _lineHeight <= containerHeight)
							{
								currentLine = new List<CharLocation>();
								currentX = 0;
								currentY += _lineHeight;
								lastWhiteSpace = -1;
								lastCharId = -1;
							}
							else
							{
								break;
							}
						}
					}
				}

				if (autoScale && !isFinished)
				{
					size -= 1;
					lines.Clear();
				}
				else
				{
					isFinished = true;
				}
			}

			List<CharLocation> finalLocations = new List<CharLocation>();
			int numLines = lines.Count;
			float bottom = numLines * _lineHeight;
			int yOffset = 0;

			if (vAlign == VAlign.Bottom)
			{
				yOffset = (int)(containerHeight - bottom);
			}
			else if (vAlign == VAlign.Center)
			{
				yOffset = (int)((containerHeight - bottom) / 2);
			}

			List<CharLocation> line;
			for (int i = 0; i < lines.Count; i++)
			{
				line = lines[i];
				int numChars = line.Count;
				if (numChars == 0)
				{
					continue;
				}

				int xOffset = 0;
				CharLocation lastLocation = line[lines.Count - 1];
				float right = lastLocation.X - lastLocation.BitmapChar.XOffset + lastLocation.BitmapChar.XAdvance;

				if (hAlign == HAlign.Right)
				{
					xOffset = (int)(containerWidth - right);
				}
				else if (hAlign == HAlign.Center)
				{
					xOffset = (int)((containerWidth - right) / 2);
				}

				CharLocation charLocation;
				for (int j = 0; j < line.Count; j++)
				{
					charLocation = line[i];
					charLocation.X = scale * (charLocation.X + xOffset);
					charLocation.Y = scale * (charLocation.Y + yOffset);
					charLocation.Scale = scale;

					if (charLocation.BitmapChar.Width > 0 && charLocation.BitmapChar.Height > 0)
					{
						finalLocations.Add(charLocation);
					}
				}
			}
				
			return finalLocations;
		}

		#region Embedded Mini Font

		private const string MiniFontXmlDataBase64 = 
			"H4sIAAAAAAAAA7Wc3XIbKRCF7/MUKt2nPM0/VXau8wZ7rbXlWLW2lFp5f7JPvxpxxgHBiEGhb1yyYn0+6Qb60DC+fz7s37" +
			"98Wq3ud/vnw+p587h9WL/t9rv16rj77/TarVe/H16fHtbDerV737zuHs8vj2+Hw/vL+PLu/OnHw9vbYb963e23X7e7by/v" +
			"4ZOb4wlhTz/+uHnd/vawJuHwzdeHtVHr1ffNt+3x9P746vGP7VNEfNn8eVw9Hv7an1jkaT2+i/dXu6fxPb1e/Xv+7I/z13" +
			"92T6Ok07svkOBPP3B4fj5u38+if0yvP4vTP2ye/t7sx/+uCb8xZovBn9mWg00y6JYccA24v4CrOnyI2brEDgEXqj+bvAjs" +
			"S91RUNxcUKgSFPIhKFKzwNUZrq6Mw1+A2wC/MhBvhothOMP1lYH4C3AKcI6EiiGMFsORUEEhLJYjoYJCWCxLQimExbEklM" +
			"JQdCwJJXeGe56EhsWcBpaMCgKdZ46aQM+qhSzTR+IMXpTwCvhL8aIPHiVaDMvw87Ep0OUpNJEBKAbezsDrC6+JDUATe6iy" +
			"fWIAmuB155IagK7KBYXhIjQHXIR1V96Szzo8KJdZQlUjvGSMRBjmSrDATWIBOsPDoq5ZlMtQjjSLchnGueFRHkaL4VEeRo" +
			"tlUa5Sf9EZDn/Bozwk1PEoDwn1PMpDQj2P8sm8sEjXQ2peOtNhjYhH+4V3aaKLCp0E6qjItMtGe6FK9KR34buyDdZ0BrYY" +
			"AtsvZCd2MUlowc/R5HXlJV005lPmcIWBbm5gU41NieXqyvaBPY3CvmyZuLmubILhGht+KZzKcGoYhi4ERQ6X7MgpmhsNNB" +
			"k0i7KRsgBe3VZYmzjFvsoRFpWFfAFc1uDYV2h1CVeN8NJii8aFzmZQFzicIoty7P4Ni3ITwmJvUV4rcEKElou9RTlV4cG1" +
			"uFuUV+EyxNxxxFygUHgW5SpUOM8Scy3g5XiC7kC/Vvtj+ufZFb1Q+zVc7tXavyinJdcipybatep/s3YhLfDXDMCiwJdckX" +
			"eRVRz362D7n2y9ZA2goQSHLcrorpHuC1U6hAW9v4htG9muVOtcbC96w31sAfrCMY+UumSbRrYtFYxgunSWzg5wMQTlOgt5" +
			"B7iTkI4jnb50k3RcIrZuZBeMkUGjOItKD7aNq2hnNk5cstnZgx3mj8/mTwe2nToWHMm001kORzYt6jNxpNOmJy2d4TYqQW" +
			"PjtSMaLpQDnRyz9GU7jELPwcZVCM3BxuaWI5UuOWLpzMYJC0succDCkkvUTJZchvFtOHIJX2g4comes+XIJTrOliOX2F85" +
			"jlzi2pbnyCX65J4ll9hZDSzJtMmWsy9cYVtFHOlUUC448gmnDHSxy3fztBdxNe6KnjbJkoGtkjsPnXXrpBr31S2SatyVTd" +
			"j0SI5kEhy+4sgmWZUU5L5wuCvNkU9BJinJfeFYVAxHQhFym+VTNbILrUOb7JD7stEccxy6CfdjHYdwws07z6Mcx7QcynEL" +
			"/KPkd4Xj8G3agHcOi5/uI3BIF2La33NoFx/3EVgGDBausG6N91ny3v4idqH7bsIs0hzoEHHPgKapzyQ54Nhy4uZKX7iQoT" +
			"aLpclMDjsSevGSRtKdiOiiUXrpIgWUOwa2TTofEZsa2cWgwGtlg6ULPNQ4mUW8Axy3KGU2EGfYs0OldAXE4JpGNj9bhRfh" +
			"6Nrw5DOxtxE8OgBSM/DkNLJ0uoTHEXSmvANcSKy2Wcw7wH1iQCO2amSXjBweuBMMbBqSS7Gd4XhcyLEox+NCjkd5WLU8j3" +
			"JYUB7l02UEFuliOmJi0f5hEzm0C+SUcmvRgz41zQTPkIkbW7p44XYRu2jnXORC29h1q+gjG9qZjYMP2NAITo3wQplDfaYs" +
			"KFFDQS4JeLGJQ7EL7Q2HCbU3wFUNjnsrMgt5F+Um9kTlwTIHrw0WPOSgMuGizjYVv6VxbcVcGYcxmxom5/QEtbtkR4uKWL" +
			"RhKV61DxHXWVRm6C1LFp4T0plyWWfXhOMJRDzgVM7mInbJPeP82mRTvwscV3myqCyA2wocm1ubLSvUKLywHuIPSthrE5+W" +
			"6C61+sHO5r2qs1VlFBL2trgn1BmOi0Lu2ooVw2cnZ1E5LGgW8hl403JoYgdaHipzbFMZKoSLFD6LygK4qsDx3CSWWvOBHn" +
			"6ihxk0idIEur8b2ccvn+7vzn/v5X/zg7A/9kUAAA==";
		private const string MiniFontImageDataBase64 = 
			"iVBORw0KGgoAAAANSUhEUgAAAIAAAABABAMAAAAg+GJMAAAAJFBMVEUAAAD///////////////////////////////////" +
			"////////+0CY3pAAAAC3RSTlMAAgQGCg4QFNn5/aulndcAAANHSURBVFhH7ZYxrhtHEESf4J+9RLGu4NCRoHQBBZv5EEp8" +
			"AAVMfAQf4R+hAgIK6nIOenZJSt+GjW/IiRrN4XA4XV1dPcshvNrevFkubyFAELybfzshRATg3bvl4dkjNHw5YV6eKAkAz8" +
			"/LH23Q/41JIs3ptuO3FTydHAwakUYS3fabsyjfrZzROQHcdieQxDOrrc3yu8QLQG4ArbpI9HHjXzO4B0Cp2w75KtM3Gtz8" +
			"a4ARD0eV721zMhpyOoSix+wtJIKY20wgQAsjyw1SJMkxe9YpmtzPwCFAI4xaD0h/b3b2NkeD8NNv4qg5Q+y0926NOGfmad" +
			"qAK/d5YrZc9xk+5nqZgXNtywEwDCYOEfzlwyPAzjUzvAQw9a/gLA3GF/G7EsithHNtuvBakxFFqYlluh8xFut8yog69Mk6" +
			"MECmb7OS6xan03JUTSzw5XIjrfNakUc0SYjQ5gEg0Dl7lh45l+mHO4DrlgZCs9pfmuCW605z1W2V8DIDi2tpkRRiB0BeBD" +
			"gkCQmkpU1Yz4sUVm8zJVjiocGh2OrCgH5fa1szNDLVBwsWm3mjx9imjV01g7/+DFQGYCTjy+cFuRNy3ZKnhBk5PKNR22CS" +
			"SJL8npCVvdltJiuBPI3EpGnTALKORyKReThXaxaDI/c9g5wMcKGbeZ+WreKDJeReg8CdBq82UZykU6/tLC4/LznWb9fNEU" +
			"yNbruMjyzKdDWwNorO7PPFz5d1meEYHgxyA1j7oaU5qTBEZ8Ps7XGbZ+U/0wvBqRXBSQ+67eRBg5k3yMkDOe7YMN/euSPj" +
			"a+3IjRynwyNHhwqrGJyKmgYJdELDVGo7MOv/xK5bYQEUa8kpSyNhXTATnQyGVkurF9sBeMpVSQJzSWRffYWQA0No3Hb3ol" +
			"53wHuAOtUcDBh5uWkw39GgS4PSTglLI6EJyn9ggxMy/MZqJFJ7XIYNJwdJKzFgCfHiBcTDM6/tenFL8GOiW8oUUQjlWiCC" +
			"DEyOB+MGkAHYiW5hqTBi053pQKYYmXAX/dD1GNEJmxOc+xJGg+OILAlOgb6HqTHaEm2dmvLTHyRJiM7T2Kr9hp5BOmcrjH" +
			"wXwvv3ujr2dcijOSoMA1BCXLL+E5M5NT/sh/2v9idsZLc1sYX4WAAAAABJRU5ErkJggg==";

		#endregion

		#region ASCII Codes

		private const int SpaceAsciiCode = 32;
		private const int TabAsciiCode = 39;
		private const int NewLineAsciiCode = 10;
		private const int CarriageReturnAsciiCode = 13;

		#endregion
	}
}

