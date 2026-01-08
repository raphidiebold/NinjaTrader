#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
	public class VolumeBubbleIndicator : Indicator
	{
		private class VolumeData
		{
			public long Volume { get; set; }
			public double Price { get; set; }
			public DateTime Time { get; set; }
			public bool IsAsk { get; set; }
			public int BarIndex { get; set; }
		}
		
private class AbsorptionCandidate
	{
		public double Price { get; set; }
		public int BarIndex { get; set; }
		public bool IsAsk { get; set; }
		public long Volume { get; set; }
		public bool IsChecked { get; set; }
	}
	
	private class AbsorptionData
	{
		public double Price { get; set; }
		public int BarIndex { get; set; }
		public bool IsAsk { get; set; }
		public long Volume { get; set; }
		public double HighAfter { get; set; }
		public double LowAfter { get; set; }
		}
		
		private class PriceLevelKey
		{
			public double Price { get; set; }
			public bool IsAsk { get; set; }
			public int BarIndex { get; set; }
			
			public override bool Equals(object obj)
			{
				if (obj is PriceLevelKey other)
					return Price == other.Price && IsAsk == other.IsAsk && BarIndex == other.BarIndex;
				return false;
			}
			
			public override int GetHashCode()
			{
				return Price.GetHashCode() ^ IsAsk.GetHashCode() ^ BarIndex.GetHashCode();
			}
		}

		private List<VolumeData> volumeDataList;
		private Dictionary<PriceLevelKey, long> currentBarVolumes;
		private int lastBarProcessed = -1;
		
	// Absorption-Erkennung
	private List<AbsorptionCandidate> absorptionCandidates;
	private List<AbsorptionData> detectedAbsorptions;
	
	// Hover-Funktionalität
	private Point mousePosition;
	private bool hasMousePosition = false;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = @"Volume Bubble Indicator mit Iceberg-Erkennung und Hover-Tooltips";
				Name = "VolumeBubbleIndicator";
				Calculate = Calculate.OnEachTick;
				IsOverlay = true;
				DisplayInDataBox = true;
				DrawOnPricePanel = true;
				DrawHorizontalGridLines = true;
				DrawVerticalGridLines = true;
				PaintPriceMarkers = true;
				ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive = false;

				// Einstellbare Parameter für Volumen-Bubbles
				MinVolume = 100;
				MaxBubbleSize = 30;
				MinBubbleSize = 5;
				BidColor = Brushes.Red;
				AskColor = Brushes.Lime;
				BubbleOpacity = 60;
				ShowVolumeText = false;
				
				// Absorption-Erkennungsparameter
				EnableAbsorptionDetection = true;
			MinVolumeForAbsorption = 10;
			AbsorptionLookForwardBars = 5;
			MaxPriceRangeTicks = 4;
			MinPriceReturnTicks = 2;
			AbsorptionColor = Brushes.Yellow;
			AbsorptionMarkerSize = 12;
			ShowAbsorptionLabel = true;
			}
			else if (State == State.Configure)
			{
				volumeDataList = new List<VolumeData>();
				currentBarVolumes = new Dictionary<PriceLevelKey, long>();
				absorptionCandidates = new List<AbsorptionCandidate>();
				detectedAbsorptions = new List<AbsorptionData>();
			}
			else if (State == State.DataLoaded)
			{
				// Registriere MouseMove-Event für Hover-Funktionalität
				if (ChartPanel != null)
				{
					ChartPanel.MouseMove += OnMouseMove;
				}
			}
			else if (State == State.Terminated)
			{
				// Cleanup: Entferne Event-Handler
				if (ChartPanel != null)
				{
					ChartPanel.MouseMove -= OnMouseMove;
				}
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 1)
				return;

			// Wenn ein neuer Bar beginnt, speichere die Volumendaten des vorherigen Bars
			if (lastBarProcessed != CurrentBar)
			{
				if (lastBarProcessed >= 0)
				{
					// Speichere alle Preisniveaus des vorherigen Bars
					foreach (var kvp in currentBarVolumes)
					{
						if (kvp.Value >= MinVolume)
						{
							volumeDataList.Add(new VolumeData
							{
								Volume = kvp.Value,
								Price = kvp.Key.Price,
								Time = Time[1],
								IsAsk = kvp.Key.IsAsk,
								BarIndex = CurrentBar - 1
							});
						}
					}
				}

				// Reset für neuen Bar
				currentBarVolumes.Clear();
				lastBarProcessed = CurrentBar;
			}
			
			// Absorption-Erkennung durchführen
			if (EnableAbsorptionDetection)
			{
				DetectAbsorption();
			}
		}

		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
			if (marketDataUpdate.MarketDataType == MarketDataType.Last)
			{
				double price = marketDataUpdate.Price;
				long volume = marketDataUpdate.Volume;
				double bid = marketDataUpdate.Bid;
				double ask = marketDataUpdate.Ask;

				// Bestimme ob Bid oder Ask basierend auf Trade-Preis vs Bid/Ask
				bool isAsk;
				
				if (price >= ask)
				{
					isAsk = true;
				}
				else if (price <= bid)
				{
					isAsk = false;
				}
				else
				{
					isAsk = (price - bid) > (ask - price);
				}

				// Erstelle Key für dieses Preisniveau
				var key = new PriceLevelKey
				{
					Price = price,
					IsAsk = isAsk,
					BarIndex = CurrentBar
				};
				
				// Addiere Volumen zu diesem Preisniveau
				if (currentBarVolumes.ContainsKey(key))
					currentBarVolumes[key] += volume;
				else
					currentBarVolumes[key] = volume;
			}
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);

			if (Bars == null || ChartControl == null)
				return;

			// Zeichne historische Bubbles
			foreach (var data in volumeDataList)
			{
				DrawBubble(chartScale, data);
			}

			// Zeichne aktuellen Bar Bubbles pro Preisniveau
			foreach (var kvp in currentBarVolumes)
			{
				if (kvp.Value >= MinVolume)
				{
					var data = new VolumeData
					{
						Volume = kvp.Value,
						Price = kvp.Key.Price,
						Time = Time[0],
						IsAsk = kvp.Key.IsAsk,
						BarIndex = CurrentBar
					};
					DrawBubble(chartScale, data);
				}
			}
			
		// Zeichne Absorption-Markierungen
		if (EnableAbsorptionDetection)
		{
			foreach (var absorption in detectedAbsorptions)
			{
				DrawAbsorptionMarker(chartScale, absorption);
			}
		}
	}

	private void OnMouseMove(object sender, MouseEventArgs e)
	{
		if (ChartControl != null)
		{
			// Verwende Screen-Koordinaten und transformiere zu Control-Koordinaten
			var screenPoint = ChartControl.PointToScreen(new Point(0, 0));
			var mouseScreenPoint = ChartControl.PointToScreen(Mouse.GetPosition(ChartControl));
			
			mousePosition = new Point(
				mouseScreenPoint.X - screenPoint.X,
				mouseScreenPoint.Y - screenPoint.Y
			);
			
			hasMousePosition = true;
			ChartControl.InvalidateVisual();
		}
	}

	private bool IsMouseOverBubble(int mouseX, int mouseY, int bubbleX, int bubbleY, double bubbleSize)
	{
		double distance = Math.Sqrt(Math.Pow(mouseX - bubbleX, 2) + Math.Pow(mouseY - bubbleY, 2));
		return distance <= bubbleSize;
	}
	
	private void DetectAbsorption()
	{
		// Prüfe aktuelle Bar auf große Volumen-Bubbles
		foreach (var kvp in currentBarVolumes)
		{
			if (kvp.Value >= MinVolumeForAbsorption)
			{
				// Füge als Absorption-Kandidat hinzu
				var candidate = new AbsorptionCandidate
				{
					Price = kvp.Key.Price,
					BarIndex = CurrentBar,
					IsAsk = kvp.Key.IsAsk,
					Volume = kvp.Value,
					IsChecked = false
				};
				absorptionCandidates.Add(candidate);
			}
		}
		
		// Prüfe bestehende Kandidaten
		var candidatesToRemove = new List<AbsorptionCandidate>();
		
		foreach (var candidate in absorptionCandidates)
		{
			if (candidate.IsChecked)
				continue;
				
			int barsElapsed = CurrentBar - candidate.BarIndex;
			
			if (barsElapsed >= AbsorptionLookForwardBars)
			{
				// Prüfe Preisbewegung nach der großen Order
				double highAfter = High[0];
				double lowAfter = Low[0];
				
				// Finde Hoch und Tief in den nächsten Bars
				for (int i = 1; i < barsElapsed && i < Bars.Count; i++)
				{
					highAfter = Math.Max(highAfter, High[i]);
					lowAfter = Math.Min(lowAfter, Low[i]);
				}
				
				bool isAbsorption = false;
				double maxPriceRange = MaxPriceRangeTicks * TickSize;
			double minReturnDistance = MinPriceReturnTicks * TickSize;
			
			if (!candidate.IsAsk) // Rote Bubble (Verkaufsdruck)
			{
				// Bei Verkaufsdruck sollte Preis fallen - wenn nicht = Absorption
				double priceDrop = candidate.Price - lowAfter;
				double currentDistance = candidate.Price - Close[0];
				
				// Strikte Bedingung: Preis muss in Range GEBLIEBEN sein (nicht nur zurückgekehrt)
				if (priceDrop < maxPriceRange)
				{
					// Preis ist nicht gefallen = klare Absorption
					isAbsorption = true;
				}
				// Nur wenn Preis weit genug fiel UND deutlich zurückkehrte
				else if (priceDrop >= maxPriceRange && currentDistance < minReturnDistance)
				{
					// Preis fiel, kam aber sehr nah zurück
					isAbsorption = true;
				}
			}
			else // Grüne Bubble (Kaufdruck)
			{
				// Bei Kaufdruck sollte Preis steigen - wenn nicht = Absorption
				double priceRise = highAfter - candidate.Price;
				double currentDistance = Close[0] - candidate.Price;
				
				// Strikte Bedingung: Preis muss in Range GEBLIEBEN sein (nicht nur zurückgekehrt)
				if (priceRise < maxPriceRange)
				{
					// Preis ist nicht gestiegen = klare Absorption
					isAbsorption = true;
				}
				// Nur wenn Preis weit genug stieg UND deutlich zurückfiel
				else if (priceRise >= maxPriceRange && currentDistance < minReturnDistance)
				{
					// Preis stieg, fiel aber sehr nah zurück
					isAbsorption = true;
				}
			}
			
			if (isAbsorption)
			{
				detectedAbsorptions.Add(new AbsorptionData
					{
						Price = candidate.Price,
						BarIndex = candidate.BarIndex,
						IsAsk = candidate.IsAsk,
						Volume = candidate.Volume,
						HighAfter = highAfter,
						LowAfter = lowAfter
					});
				}
				
				candidate.IsChecked = true;
				candidatesToRemove.Add(candidate);
			}
		}
		// Entferne geprüfte Kandidaten
		foreach (var candidate in candidatesToRemove)
		{
			absorptionCandidates.Remove(candidate);
		}
		
		// Begrenze Anzahl gespeicherter Absorptions-Events
		if (detectedAbsorptions.Count > 50)
		{
			detectedAbsorptions = detectedAbsorptions
				.OrderByDescending(a => a.BarIndex)
				.Take(50)
				.ToList();
		}
	}

		private void DrawBubble(ChartScale chartScale, VolumeData data)
		{
			// Berechne Bubble-Größe basierend auf Volumen
			double volumeRatio = Math.Min(1.0, (double)data.Volume / (MinVolume * 10));
			double bubbleSize = MinBubbleSize + (MaxBubbleSize - MinBubbleSize) * Math.Sqrt(volumeRatio);

			int barIndex = data.BarIndex;
			if (barIndex < 0 || barIndex >= Bars.Count)
				return;

			int x = ChartControl.GetXByBarIndex(ChartBars, barIndex);
			int y = chartScale.GetYByValue(data.Price);

			// Prüfe, ob Maus über dieser Bubble ist
			bool isHovered = false;
			if (hasMousePosition)
			{
				isHovered = IsMouseOverBubble((int)mousePosition.X, (int)mousePosition.Y, x, y, bubbleSize);
			}

			// Wähle Farbe basierend auf Bid/Ask
			Brush brush = data.IsAsk ? AskColor : BidColor;
			
			// Erstelle halbtransparente Farbe
			Color color = ((SolidColorBrush)brush).Color;
			color.A = (byte)(255 * BubbleOpacity / 100);
			Brush transparentBrush = new SolidColorBrush(color);
			transparentBrush.Freeze();

			SharpDX.Direct2D1.RenderTarget renderTarget = RenderTarget;
			
			var oldAntialiasMode = renderTarget.AntialiasMode;
			renderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			
			SharpDX.Direct2D1.Brush dxBrush = transparentBrush.ToDxBrush(renderTarget);
			SharpDX.Vector2 center = new SharpDX.Vector2(x, y);
			
			// Zeichne Bubble ohne dicke Umrandung (minimalistisch)
			renderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(center, (float)bubbleSize, (float)bubbleSize), dxBrush);
			
			renderTarget.AntialiasMode = oldAntialiasMode;

			// Zeichne Volumen-Text wenn aktiviert
			if (ShowVolumeText)
			{
				string volumeText = data.Volume.ToString("N0");
				SharpDX.DirectWrite.TextFormat textFormat = ChartControl.Properties.LabelFont.ToDirectWriteTextFormat();
				SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(
					NinjaTrader.Core.Globals.DirectWriteFactory,
					volumeText,
					textFormat,
					200,
					textFormat.FontSize);

				Brush textBrush = Brushes.White;
				SharpDX.Direct2D1.Brush dxTextBrush = textBrush.ToDxBrush(renderTarget);

				renderTarget.DrawTextLayout(
					new SharpDX.Vector2(x - textLayout.Metrics.Width / 2, y - textLayout.Metrics.Height / 2),
					textLayout,
					dxTextBrush,
					SharpDX.Direct2D1.DrawTextOptions.None);

				textLayout.Dispose();
				dxTextBrush.Dispose();
			}
			
			// Zeichne Tooltip wenn Maus über Bubble ist
			if (isHovered)
			{
				string tooltipText = string.Format("Vol: {0:N0}\n{1}\nPreis: {2}", 
					data.Volume, 
					data.IsAsk ? "Ask (Käufer)" : "Bid (Verkäufer)",
					data.Price.ToString("F2"));
				
				SharpDX.DirectWrite.TextFormat tooltipFormat = new SharpDX.DirectWrite.TextFormat(
					NinjaTrader.Core.Globals.DirectWriteFactory,
					"Arial",
					SharpDX.DirectWrite.FontWeight.Bold,
					SharpDX.DirectWrite.FontStyle.Normal,
					12f);
				
				SharpDX.DirectWrite.TextLayout tooltipLayout = new SharpDX.DirectWrite.TextLayout(
					NinjaTrader.Core.Globals.DirectWriteFactory,
					tooltipText,
					tooltipFormat,
					300,
					100);
				
				float tooltipX = x - tooltipLayout.Metrics.Width / 2;
				float tooltipY = y - (float)bubbleSize - tooltipLayout.Metrics.Height - 10;
				
				if (tooltipX < 0) tooltipX = 5;
				if (tooltipY < 0) tooltipY = y + (float)bubbleSize + 10;
				
				SharpDX.RectangleF tooltipRect = new SharpDX.RectangleF(
					tooltipX - 5,
					tooltipY - 5,
					tooltipLayout.Metrics.Width + 10,
					tooltipLayout.Metrics.Height + 10);
				
				Brush tooltipBgBrush = new SolidColorBrush(Color.FromArgb(230, 40, 40, 40));
				tooltipBgBrush.Freeze();
				SharpDX.Direct2D1.Brush dxTooltipBgBrush = tooltipBgBrush.ToDxBrush(renderTarget);
				renderTarget.FillRectangle(tooltipRect, dxTooltipBgBrush);
				
				Brush tooltipBorderBrush = data.IsAsk ? AskColor : BidColor;
				SharpDX.Direct2D1.Brush dxTooltipBorderBrush = tooltipBorderBrush.ToDxBrush(renderTarget);
				renderTarget.DrawRectangle(tooltipRect, dxTooltipBorderBrush, 2f);
				
				Brush tooltipTextBrush = Brushes.White;
				SharpDX.Direct2D1.Brush dxTooltipTextBrush = tooltipTextBrush.ToDxBrush(renderTarget);
				renderTarget.DrawTextLayout(
					new SharpDX.Vector2(tooltipX, tooltipY),
					tooltipLayout,
					dxTooltipTextBrush,
					SharpDX.Direct2D1.DrawTextOptions.None);
				
				tooltipLayout.Dispose();
				tooltipFormat.Dispose();
				dxTooltipBgBrush.Dispose();
				dxTooltipBorderBrush.Dispose();
				dxTooltipTextBrush.Dispose();
			}

			dxBrush.Dispose();
		}
		
		private void DrawAbsorptionMarker(ChartScale chartScale, AbsorptionData absorption)
		{
			int x = ChartControl.GetXByBarIndex(ChartBars, absorption.BarIndex);
			int y = chartScale.GetYByValue(absorption.Price);
			
			if (x < 0)
				return;
			
			SharpDX.Direct2D1.RenderTarget renderTarget = RenderTarget;
			
			Color markerColor = ((SolidColorBrush)AbsorptionColor).Color;
			markerColor.A = 255;
			Brush markerBrush = new SolidColorBrush(markerColor);
			markerBrush.Freeze();
			SharpDX.Direct2D1.Brush dxMarkerBrush = markerBrush.ToDxBrush(renderTarget);
			
			// Zeichne Absorption-Symbol: Quadrat mit "A" darin
			float size = AbsorptionMarkerSize;
			SharpDX.RectangleF markerRect = new SharpDX.RectangleF(
				x - size / 2,
				y - size / 2,
				size,
				size);
			
			renderTarget.FillRectangle(markerRect, dxMarkerBrush);
			
			// Zeichne "A" für Absorption
			SharpDX.DirectWrite.TextFormat textFormat = new SharpDX.DirectWrite.TextFormat(
				NinjaTrader.Core.Globals.DirectWriteFactory,
				"Arial",
				SharpDX.DirectWrite.FontWeight.Bold,
				SharpDX.DirectWrite.FontStyle.Normal,
				size * 0.7f);
			
			textFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
			textFormat.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
			
			Brush textBrush = Brushes.Black;
			SharpDX.Direct2D1.Brush dxTextBrush = textBrush.ToDxBrush(renderTarget);
			
			renderTarget.DrawText(
				"A",
				textFormat,
				markerRect,
				dxTextBrush,
				SharpDX.Direct2D1.DrawTextOptions.None);
			
			// Zeichne Label mit Details nur wenn aktiviert
			if (ShowAbsorptionLabel)
			{
				string labelText = string.Format("ABSORPTION: {0}\nVol: {1}",
					absorption.IsAsk ? "Kaufdruck absorbiert" : "Verkaufsdruck absorbiert",
					absorption.Volume.ToString("N0"));
				
				SharpDX.DirectWrite.TextFormat labelFormat = new SharpDX.DirectWrite.TextFormat(
					NinjaTrader.Core.Globals.DirectWriteFactory,
					"Arial",
					SharpDX.DirectWrite.FontWeight.Bold,
					SharpDX.DirectWrite.FontStyle.Normal,
					10f);
				
				SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(
					NinjaTrader.Core.Globals.DirectWriteFactory,
					labelText,
					labelFormat,
					300,
					50);
				
				float textX = x + size;
				float textY = y - textLayout.Metrics.Height / 2;
				
				SharpDX.RectangleF bgRect = new SharpDX.RectangleF(
					textX - 3,
					textY - 2,
					textLayout.Metrics.Width + 6,
					textLayout.Metrics.Height + 4);
				
				Brush bgBrush = new SolidColorBrush(Color.FromArgb(220, 0, 0, 0));
				bgBrush.Freeze();
				SharpDX.Direct2D1.Brush dxBgBrush = bgBrush.ToDxBrush(renderTarget);
				renderTarget.FillRectangle(bgRect, dxBgBrush);
				
				renderTarget.DrawTextLayout(
					new SharpDX.Vector2(textX, textY),
					textLayout,
					dxMarkerBrush,
					SharpDX.Direct2D1.DrawTextOptions.None);
				
				textLayout.Dispose();
				labelFormat.Dispose();
				dxBgBrush.Dispose();
			}
			
			textFormat.Dispose();
			dxTextBrush.Dispose();
			dxMarkerBrush.Dispose();
		}

		public override string DisplayName
	{
		get { return Name; }
	}

	#region Properties

	[NinjaScriptProperty]
	[Range(1, int.MaxValue)]
	[Display(Name = "Minimum Volume", Description = "Minimum volume for bubble display", Order = 1, GroupName = "Parameters")]
	public int MinVolume { get; set; }

	[NinjaScriptProperty]
	[Range(5, 100)]
	[Display(Name = "Max Bubble Size", Description = "Maximum size of bubble in pixels", Order = 2, GroupName = "Parameters")]
	public int MaxBubbleSize { get; set; }

	[NinjaScriptProperty]
	[Range(2, 50)]
	[Display(Name = "Min Bubble Size", Description = "Minimum size of bubble in pixels", Order = 3, GroupName = "Parameters")]
	public int MinBubbleSize { get; set; }

		[XmlIgnore]
		[Display(Name = "Bid Color", Description = "Color for bid bubbles", Order = 4, GroupName = "Colors")]
		public Brush BidColor { get; set; }

		[Browsable(false)]
		public string BidColorSerializable
		{
			get { return Serialize.BrushToString(BidColor); }
			set { BidColor = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "Ask Color", Description = "Color for ask bubbles", Order = 5, GroupName = "Colors")]
		public Brush AskColor { get; set; }

		[Browsable(false)]
		public string AskColorSerializable
		{
			get { return Serialize.BrushToString(AskColor); }
			set { AskColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Range(10, 100)]
		[Display(Name = "Opacity (%)", Description = "Transparency of bubbles (10-100%)", Order = 6, GroupName = "Colors")]
		public int BubbleOpacity { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Show Volume Text", Description = "Displays volume value in bubble", Order = 7, GroupName = "Display")]
		public bool ShowVolumeText { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Absorption Detection", Description = "Enables detection of absorption", Order = 1, GroupName = "Absorption Detection")]
		public bool EnableAbsorptionDetection { get; set; }
		
		[NinjaScriptProperty]
	[Range(10, 1000)]
	[Display(Name = "Min. Volume for Absorption", Description = "Minimum volume of a bubble to check for absorption", Order = 2, GroupName = "Absorption Detection")]
	public int MinVolumeForAbsorption { get; set; }
	
	[NinjaScriptProperty]
	[Range(1, 10)]
	[Display(Name = "Lookforward Bars", Description = "Number of bars after bubble to check price movement", Order = 3, GroupName = "Absorption Detection")]
	public int AbsorptionLookForwardBars { get; set; }
	
	[NinjaScriptProperty]
	[Range(1, 20)]
	[Display(Name = "Max. Price Range (Ticks)", Description = "Maximum price movement in ticks for absorption detection", Order = 4, GroupName = "Absorption Detection")]
	public int MaxPriceRangeTicks { get; set; }
	
	[NinjaScriptProperty]
	[Range(1, 10)]
	[Display(Name = "Min. Return Distance (Ticks)", Description = "Price must return at least this close for absorption", Order = 5, GroupName = "Absorption Detection")]
	public int MinPriceReturnTicks { get; set; }
	
	[XmlIgnore]
	[Display(Name = "Absorption Color", Description = "Color for absorption markers", Order = 6, GroupName = "Absorption Detection")]
	public Brush AbsorptionColor { get; set; }
	
	[Browsable(false)]
	public string AbsorptionColorSerializable
	{
		get { return Serialize.BrushToString(AbsorptionColor); }
		set { AbsorptionColor = Serialize.StringToBrush(value); }
	}
	
	[NinjaScriptProperty]
	[Range(8, 30)]
	[Display(Name = "Absorption Marker Size", Description = "Size of absorption markers in pixels", Order = 7, GroupName = "Absorption Detection")]
	public int AbsorptionMarkerSize { get; set; }
	
	[NinjaScriptProperty]
	[Display(Name = "Show Absorption Label", Description = "Shows text label next to absorption marker", Order = 8, GroupName = "Absorption Detection")]
	public bool ShowAbsorptionLabel { get; set; }

	#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private VolumeBubbleIndicator[] cacheVolumeBubbleIndicator;
		public VolumeBubbleIndicator VolumeBubbleIndicator(int minVolume, int maxBubbleSize, int minBubbleSize, int bubbleOpacity, bool showVolumeText)
		{
			return VolumeBubbleIndicator(Input, minVolume, maxBubbleSize, minBubbleSize, bubbleOpacity, showVolumeText);
		}

		public VolumeBubbleIndicator VolumeBubbleIndicator(ISeries<double> input, int minVolume, int maxBubbleSize, int minBubbleSize, int bubbleOpacity, bool showVolumeText)
		{
			if (cacheVolumeBubbleIndicator != null)
				for (int idx = 0; idx < cacheVolumeBubbleIndicator.Length; idx++)
					if (cacheVolumeBubbleIndicator[idx] != null && cacheVolumeBubbleIndicator[idx].MinVolume == minVolume && cacheVolumeBubbleIndicator[idx].MaxBubbleSize == maxBubbleSize && cacheVolumeBubbleIndicator[idx].MinBubbleSize == minBubbleSize && cacheVolumeBubbleIndicator[idx].BubbleOpacity == bubbleOpacity && cacheVolumeBubbleIndicator[idx].ShowVolumeText == showVolumeText && cacheVolumeBubbleIndicator[idx].EqualsInput(input))
						return cacheVolumeBubbleIndicator[idx];
			return CacheIndicator<VolumeBubbleIndicator>(new VolumeBubbleIndicator(){ MinVolume = minVolume, MaxBubbleSize = maxBubbleSize, MinBubbleSize = minBubbleSize, BubbleOpacity = bubbleOpacity, ShowVolumeText = showVolumeText }, input, ref cacheVolumeBubbleIndicator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VolumeBubbleIndicator VolumeBubbleIndicator(int minVolume, int maxBubbleSize, int minBubbleSize, int bubbleOpacity, bool showVolumeText)
		{
			return indicator.VolumeBubbleIndicator(Input, minVolume, maxBubbleSize, minBubbleSize, bubbleOpacity, showVolumeText);
		}

		public Indicators.VolumeBubbleIndicator VolumeBubbleIndicator(ISeries<double> input , int minVolume, int maxBubbleSize, int minBubbleSize, int bubbleOpacity, bool showVolumeText)
		{
			return indicator.VolumeBubbleIndicator(input, minVolume, maxBubbleSize, minBubbleSize, bubbleOpacity, showVolumeText);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VolumeBubbleIndicator VolumeBubbleIndicator(int minVolume, int maxBubbleSize, int minBubbleSize, int bubbleOpacity, bool showVolumeText)
		{
			return indicator.VolumeBubbleIndicator(Input, minVolume, maxBubbleSize, minBubbleSize, bubbleOpacity, showVolumeText);
		}

		public Indicators.VolumeBubbleIndicator VolumeBubbleIndicator(ISeries<double> input , int minVolume, int maxBubbleSize, int minBubbleSize, int bubbleOpacity, bool showVolumeText)
		{
			return indicator.VolumeBubbleIndicator(input, minVolume, maxBubbleSize, minBubbleSize, bubbleOpacity, showVolumeText);
		}
	}
}

#endregion
