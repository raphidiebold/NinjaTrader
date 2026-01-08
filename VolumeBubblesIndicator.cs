#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Volume Bubbles Indicator - Zeigt gehandelte Volumina als Blasen an (Bid in Rot, Ask in Grün)
	/// Displays traded volumes as bubbles (Bid in Red, Ask in Green)
	/// </summary>
	public class VolumeBubblesIndicator : Indicator
	{
		private class VolumeBubble
		{
			public int BarIndex { get; set; }
			public double Price { get; set; }
			public long Volume { get; set; }
			public bool IsAsk { get; set; }
			public DateTime Time { get; set; }
		}

		private List<VolumeBubble> volumeBubbles;
		private Dictionary<int, Dictionary<double, long>> bidVolumeByBar;
		private Dictionary<int, Dictionary<double, long>> askVolumeByBar;
		private Brush cachedBidBrush;
		private Brush cachedAskBrush;
		private const int MaxBubbles = 5000; // Maximum number of bubbles to store

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Zeigt gehandelte Volumina als skalierbare Blasen an (Bid=Rot, Ask=Grün) / Displays traded volumes as scalable bubbles (Bid=Red, Ask=Green)";
				Name										= "Volume Bubbles Indicator";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				
				// Configurable properties / Konfigurierbare Eigenschaften
				MinimumVolume								= 100;
				MaxBubbleSize								= 20;
				MinBubbleSize								= 5;
				BidColor									= Brushes.Red;
				AskColor									= Brushes.Green;
				BubbleOpacity								= 0.6;
				ShowVolumeLabel								= true;
			}
			else if (State == State.Configure)
			{
				volumeBubbles = new List<VolumeBubble>();
				bidVolumeByBar = new Dictionary<int, Dictionary<double, long>>();
				askVolumeByBar = new Dictionary<int, Dictionary<double, long>>();
			}
			else if (State == State.DataLoaded)
			{
				// Initialize cached brushes with opacity
				cachedBidBrush = BidColor.Clone();
				cachedBidBrush.Opacity = BubbleOpacity;
				cachedBidBrush.Freeze();
				
				cachedAskBrush = AskColor.Clone();
				cachedAskBrush.Opacity = BubbleOpacity;
				cachedAskBrush.Freeze();
			}
			else if (State == State.Terminated)
			{
				// Clean up resources
				if (volumeBubbles != null)
					volumeBubbles.Clear();
				if (bidVolumeByBar != null)
					bidVolumeByBar.Clear();
				if (askVolumeByBar != null)
					askVolumeByBar.Clear();
			}
		}

		protected override void OnBarUpdate()
		{
			// This indicator works primarily with market depth/tick data
			// OnBarUpdate is used for maintaining bar-based data structures
		}

		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
			if (marketDataUpdate.MarketDataType == MarketDataType.Last)
			{
				// Get current bar index
				int barIndex = CurrentBar;
				double price = marketDataUpdate.Price;
				long volume = marketDataUpdate.Volume;

				// Determine if this is a bid or ask trade based on price movement
				// Check if Bid and Ask data are available first
				bool isAsk = false;
				if (marketDataUpdate.Ask > 0 && marketDataUpdate.Bid > 0)
				{
					// If price is closer to ask, it's a buy (ask), if closer to bid it's a sell (bid)
					double midPrice = (marketDataUpdate.Ask + marketDataUpdate.Bid) / 2;
					isAsk = marketDataUpdate.Price >= midPrice;
				}
				else
				{
					// Fallback: compare with previous price if bid/ask not available
					isAsk = marketDataUpdate.Price >= marketDataUpdate.Ask;
				}

				// Check if volume meets minimum threshold
				if (volume >= MinimumVolume)
				{
					// Store the volume bubble
					VolumeBubble bubble = new VolumeBubble
					{
						BarIndex = barIndex,
						Price = price,
						Volume = volume,
						IsAsk = isAsk,
						Time = marketDataUpdate.Time
					};

					volumeBubbles.Add(bubble);
					
					// Limit the number of stored bubbles to prevent memory issues
					if (volumeBubbles.Count > MaxBubbles)
					{
						// Remove oldest bubbles (first half)
						volumeBubbles.RemoveRange(0, MaxBubbles / 2);
					}

					// Also aggregate by bar and price for historical view
					if (isAsk)
					{
						if (!askVolumeByBar.ContainsKey(barIndex))
							askVolumeByBar[barIndex] = new Dictionary<double, long>();
						
						if (!askVolumeByBar[barIndex].ContainsKey(price))
							askVolumeByBar[barIndex][price] = 0;
						
						askVolumeByBar[barIndex][price] += volume;
					}
					else
					{
						if (!bidVolumeByBar.ContainsKey(barIndex))
							bidVolumeByBar[barIndex] = new Dictionary<double, long>();
						
						if (!bidVolumeByBar[barIndex].ContainsKey(price))
							bidVolumeByBar[barIndex][price] = 0;
						
						bidVolumeByBar[barIndex][price] += volume;
					}
				}
			}
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);

			if (Bars == null || chartControl == null || volumeBubbles == null)
				return;

			// Render volume bubbles - only those in visible range
			int visibleBubbleCount = 0;
			foreach (var bubble in volumeBubbles)
			{
				// Skip if bar is not in visible range
				if (bubble.BarIndex < ChartBars.FromIndex || bubble.BarIndex > ChartBars.ToIndex)
					continue;

				visibleBubbleCount++;

				// Calculate bubble size based on volume
				double bubbleSize = CalculateBubbleSize(bubble.Volume);

				// Get pixel coordinates
				int x = chartControl.GetXByBarIndex(ChartBars, bubble.BarIndex);
				int y = chartScale.GetYByValue(bubble.Price);

				// Select cached brush based on bid/ask
				Brush renderBrush = bubble.IsAsk ? cachedAskBrush : cachedBidBrush;

				// Draw the bubble (circle)
				SharpDX.Direct2D1.RenderTarget renderTarget = chartControl.RenderTarget;
				SharpDX.Direct2D1.Brush dxBrush = renderBrush.ToDxBrush(renderTarget);

				SharpDX.Direct2D1.Ellipse ellipse = new SharpDX.Direct2D1.Ellipse(
					new SharpDX.Vector2(x, y),
					(float)bubbleSize,
					(float)bubbleSize
				);

				renderTarget.FillEllipse(ellipse, dxBrush);

				// Draw outline
				Brush outlineBrush = bubble.IsAsk ? Brushes.DarkGreen : Brushes.DarkRed;
				SharpDX.Direct2D1.Brush dxOutlineBrush = outlineBrush.ToDxBrush(renderTarget);
				renderTarget.DrawEllipse(ellipse, dxOutlineBrush, 1);

				dxBrush.Dispose();
				dxOutlineBrush.Dispose();

				// Draw volume label if enabled and bubble is large enough
				if (ShowVolumeLabel && bubbleSize > MinBubbleSize * 1.5)
				{
					string volumeText = FormatVolume(bubble.Volume);
					
					SharpDX.DirectWrite.TextFormat textFormat = new SharpDX.DirectWrite.TextFormat(
						Core.Globals.DirectWriteFactory,
						"Arial",
						SharpDX.DirectWrite.FontWeight.Bold,
						SharpDX.DirectWrite.FontStyle.Normal,
						8.0f
					);

					SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(
						Core.Globals.DirectWriteFactory,
						volumeText,
						textFormat,
						200,
						20
					);

					SharpDX.Direct2D1.Brush textBrush = Brushes.White.ToDxBrush(renderTarget);

					renderTarget.DrawTextLayout(
						new SharpDX.Vector2(x - (float)(textLayout.Metrics.Width / 2), y - 4),
						textLayout,
						textBrush,
						SharpDX.Direct2D1.DrawTextOptions.None
					);

					textBrush.Dispose();
					textLayout.Dispose();
					textFormat.Dispose();
				}
			}
		}

		private double CalculateBubbleSize(long volume)
		{
			// Scale bubble size based on volume
			// Use logarithmic scaling for better visualization
			double scaleFactor = Math.Log10(volume) / Math.Log10(MinimumVolume);
			double size = MinBubbleSize + (scaleFactor * (MaxBubbleSize - MinBubbleSize));
			
			// Clamp to min/max bounds
			size = Math.Max(MinBubbleSize, Math.Min(MaxBubbleSize, size));
			
			return size;
		}

		private string FormatVolume(long volume)
		{
			if (volume >= 1000000)
				return (volume / 1000000.0).ToString("0.0") + "M";
			else if (volume >= 1000)
				return (volume / 1000.0).ToString("0.0") + "K";
			else
				return volume.ToString();
		}

		public override string DisplayName
		{
			get { return Name; }
		}

		#region Properties

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Minimum Volume", Description = "Mindestvolumen für Blasen-Anzeige / Minimum volume threshold for bubble display", Order = 1, GroupName = "Parameters")]
		public int MinimumVolume
		{ get; set; }

		[NinjaScriptProperty]
		[Range(5, 50)]
		[Display(Name = "Max Bubble Size", Description = "Maximale Blasengröße in Pixel / Maximum bubble size in pixels", Order = 2, GroupName = "Parameters")]
		public double MaxBubbleSize
		{ get; set; }

		[NinjaScriptProperty]
		[Range(2, 20)]
		[Display(Name = "Min Bubble Size", Description = "Minimale Blasengröße in Pixel / Minimum bubble size in pixels", Order = 3, GroupName = "Parameters")]
		public double MinBubbleSize
		{ get; set; }

		[XmlIgnore]
		[Display(Name = "Bid Color", Description = "Farbe für Bid-Blasen / Color for bid bubbles", Order = 4, GroupName = "Parameters")]
		public Brush BidColor
		{ get; set; }

		[Browsable(false)]
		public string BidColorSerializable
		{
			get { return Serialize.BrushToString(BidColor); }
			set { BidColor = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "Ask Color", Description = "Farbe für Ask-Blasen / Color for ask bubbles", Order = 5, GroupName = "Parameters")]
		public Brush AskColor
		{ get; set; }

		[Browsable(false)]
		public string AskColorSerializable
		{
			get { return Serialize.BrushToString(AskColor); }
			set { AskColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Range(0.1, 1.0)]
		[Display(Name = "Bubble Opacity", Description = "Transparenz der Blasen (0.1-1.0) / Bubble transparency (0.1-1.0)", Order = 6, GroupName = "Parameters")]
		public double BubbleOpacity
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Show Volume Label", Description = "Volumen-Text auf Blasen anzeigen / Show volume text on bubbles", Order = 7, GroupName = "Parameters")]
		public bool ShowVolumeLabel
		{ get; set; }

		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private VolumeBubblesIndicator[] cacheVolumeBubblesIndicator;
		public VolumeBubblesIndicator VolumeBubblesIndicator(int minimumVolume, double maxBubbleSize, double minBubbleSize, double bubbleOpacity, bool showVolumeLabel)
		{
			return VolumeBubblesIndicator(Input, minimumVolume, maxBubbleSize, minBubbleSize, bubbleOpacity, showVolumeLabel);
		}

		public VolumeBubblesIndicator VolumeBubblesIndicator(ISeries<double> input, int minimumVolume, double maxBubbleSize, double minBubbleSize, double bubbleOpacity, bool showVolumeLabel)
		{
			if (cacheVolumeBubblesIndicator != null)
				for (int idx = 0; idx < cacheVolumeBubblesIndicator.Length; idx++)
					if (cacheVolumeBubblesIndicator[idx] != null && cacheVolumeBubblesIndicator[idx].MinimumVolume == minimumVolume && cacheVolumeBubblesIndicator[idx].MaxBubbleSize == maxBubbleSize && cacheVolumeBubblesIndicator[idx].MinBubbleSize == minBubbleSize && cacheVolumeBubblesIndicator[idx].BubbleOpacity == bubbleOpacity && cacheVolumeBubblesIndicator[idx].ShowVolumeLabel == showVolumeLabel && cacheVolumeBubblesIndicator[idx].EqualsInput(input))
						return cacheVolumeBubblesIndicator[idx];
			return CacheIndicator<VolumeBubblesIndicator>(new VolumeBubblesIndicator(){ MinimumVolume = minimumVolume, MaxBubbleSize = maxBubbleSize, MinBubbleSize = minBubbleSize, BubbleOpacity = bubbleOpacity, ShowVolumeLabel = showVolumeLabel }, input, ref cacheVolumeBubblesIndicator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VolumeBubblesIndicator VolumeBubblesIndicator(int minimumVolume, double maxBubbleSize, double minBubbleSize, double bubbleOpacity, bool showVolumeLabel)
		{
			return indicator.VolumeBubblesIndicator(Input, minimumVolume, maxBubbleSize, minBubbleSize, bubbleOpacity, showVolumeLabel);
		}

		public Indicators.VolumeBubblesIndicator VolumeBubblesIndicator(ISeries<double> input , int minimumVolume, double maxBubbleSize, double minBubbleSize, double bubbleOpacity, bool showVolumeLabel)
		{
			return indicator.VolumeBubblesIndicator(input, minimumVolume, maxBubbleSize, minBubbleSize, bubbleOpacity, showVolumeLabel);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VolumeBubblesIndicator VolumeBubblesIndicator(int minimumVolume, double maxBubbleSize, double minBubbleSize, double bubbleOpacity, bool showVolumeLabel)
		{
			return indicator.VolumeBubblesIndicator(Input, minimumVolume, maxBubbleSize, minBubbleSize, bubbleOpacity, showVolumeLabel);
		}

		public Indicators.VolumeBubblesIndicator VolumeBubblesIndicator(ISeries<double> input , int minimumVolume, double maxBubbleSize, double minBubbleSize, double bubbleOpacity, bool showVolumeLabel)
		{
			return indicator.VolumeBubblesIndicator(input, minimumVolume, maxBubbleSize, minBubbleSize, bubbleOpacity, showVolumeLabel);
		}
	}
}

#endregion
