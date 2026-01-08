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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Volume Bubbles Indicator - Displays traded volume as bubbles for Bid and Ask
	/// Shows bubbles historically and in real-time with customizable colors and minimum volume threshold
	/// </summary>
	public class VolumeBubbles : Indicator
	{
		#region Variables
		private Dictionary<int, VolumeData> volumeDataByBar = new Dictionary<int, VolumeData>();
		private System.Windows.Controls.ToolTip chartToolTip;
		private List<BubbleDrawing> drawnBubbles = new List<BubbleDrawing>();
		#endregion
		
		#region Bubble Drawing Class
		private class BubbleDrawing
		{
			public int BarIndex { get; set; }
			public double Price { get; set; }
			public double Volume { get; set; }
			public string Type { get; set; }
			public SharpDX.Vector2 Center { get; set; }
			public float Radius { get; set; }
		}
		#endregion
		
		#region Volume Data Class
		private class VolumeData
		{
			public double BidVolume { get; set; }
			public double AskVolume { get; set; }
			public double Price { get; set; }
			public int BarIndex { get; set; }
		}
		#endregion

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Displays traded volume as bubbles for Bid and Ask volumes";
				Name										= "Volume Bubbles";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= false;
				
				// User configurable parameters
				MinimumVolume								= 100;
				AskBubbleColor								= Brushes.Green;
				BidBubbleColor								= Brushes.Red;
				BubbleOpacity								= 0.6;
				MinBubbleSize								= 5;
				MaxBubbleSize								= 30;
				ShowBidBubbles								= true;
				ShowAskBubbles								= true;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{
				volumeDataByBar.Clear();
				drawnBubbles.Clear();
			}
			else if (State == State.Historical)
			{
				if (ChartControl != null)
				{
					ChartControl.MouseMove += OnMouseMove;
				}
			}
			else if (State == State.Terminated)
			{
				if (ChartControl != null)
				{
					ChartControl.MouseMove -= OnMouseMove;
				}
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 1)
				return;
			
			// For historical data, use the bar's volume and approximate bid/ask distribution
			// This provides visual representation even when detailed market data isn't available
			if (State == State.Historical)
			{
				int barIndex = CurrentBar;
				
				if (!volumeDataByBar.ContainsKey(barIndex))
				{
					// Approximate bid/ask split based on close relative to high/low
					double totalVolume = Volume[0];
					double priceRange = High[0] - Low[0];
					
					if (priceRange > 0)
					{
						// If close is near high, more volume was likely at ask
						// If close is near low, more volume was likely at bid
						double closePct = (Close[0] - Low[0]) / priceRange;
						
						volumeDataByBar[barIndex] = new VolumeData
						{
							AskVolume = totalVolume * closePct,
							BidVolume = totalVolume * (1 - closePct),
							Price = Close[0],
							BarIndex = barIndex
						};
					}
					else
					{
						// No price movement, split volume evenly
						volumeDataByBar[barIndex] = new VolumeData
						{
							AskVolume = totalVolume * 0.5,
							BidVolume = totalVolume * 0.5,
							Price = Close[0],
							BarIndex = barIndex
						};
					}
				}
			}
		}
		
		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
			if (marketDataUpdate.MarketDataType == MarketDataType.Last)
			{
				// Get the current bar index
				int barIndex = CurrentBar;
				
				if (!volumeDataByBar.ContainsKey(barIndex))
				{
					volumeDataByBar[barIndex] = new VolumeData
					{
						BidVolume = 0,
						AskVolume = 0,
						Price = marketDataUpdate.Price,
						BarIndex = barIndex
					};
				}
				
				// Determine if trade was at bid or ask
				if (marketDataUpdate.Price <= marketDataUpdate.Bid)
				{
					// Trade at bid (selling pressure)
					volumeDataByBar[barIndex].BidVolume += marketDataUpdate.Volume;
				}
				else if (marketDataUpdate.Price >= marketDataUpdate.Ask)
				{
					// Trade at ask (buying pressure)
					volumeDataByBar[barIndex].AskVolume += marketDataUpdate.Volume;
				}
				else
				{
					// Trade between bid and ask, split volume
					double midPoint = (marketDataUpdate.Bid + marketDataUpdate.Ask) / 2;
					if (marketDataUpdate.Price > midPoint)
					{
						volumeDataByBar[barIndex].AskVolume += marketDataUpdate.Volume;
					}
					else
					{
						volumeDataByBar[barIndex].BidVolume += marketDataUpdate.Volume;
					}
				}
				
				// Update price for this bar
				volumeDataByBar[barIndex].Price = marketDataUpdate.Price;
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			
			if (Bars == null || chartControl == null || volumeDataByBar == null)
				return;
			
			// Clear previously drawn bubbles list
			drawnBubbles.Clear();
			
			// Get visible bar range
			int firstVisibleBar = ChartBars.FromIndex;
			int lastVisibleBar = ChartBars.ToIndex;
			
			// Clean up old data that's no longer visible (keep a buffer)
			CleanupOldData(firstVisibleBar - 100);
			
			// Draw bubbles for visible bars
			foreach (var kvp in volumeDataByBar)
			{
				int barIndex = kvp.Key;
				VolumeData data = kvp.Value;
				
				// Only draw bubbles within visible range
				if (barIndex < firstVisibleBar || barIndex > lastVisibleBar)
					continue;
				
				// Draw Ask bubble (green)
				if (ShowAskBubbles && data.AskVolume >= MinimumVolume)
				{
					DrawVolumeBubble(chartControl, chartScale, barIndex, data.Price, data.AskVolume, AskBubbleColor, "Ask");
				}
				
				// Draw Bid bubble (red)
				if (ShowBidBubbles && data.BidVolume >= MinimumVolume)
				{
					DrawVolumeBubble(chartControl, chartScale, barIndex, data.Price, data.BidVolume, BidBubbleColor, "Bid");
				}
			}
		}
		
		private void CleanupOldData(int keepFromBar)
		{
			// Remove data older than keepFromBar to prevent memory bloat
			var keysToRemove = volumeDataByBar.Keys.Where(k => k < keepFromBar).ToList();
			foreach (var key in keysToRemove)
			{
				volumeDataByBar.Remove(key);
			}
		}
		
		private void DrawVolumeBubble(ChartControl chartControl, ChartScale chartScale, int barIndex, double price, double volume, Brush color, string type)
		{
			// Calculate bubble size based on volume
			double normalizedVolume = Math.Min(volume / (MinimumVolume * 10), 1.0);
			double bubbleSize = MinBubbleSize + (normalizedVolume * (MaxBubbleSize - MinBubbleSize));
			
			// Get screen coordinates
			int x = chartControl.GetXByBarIndex(ChartBars, barIndex);
			int y = chartScale.GetYByValue(price);
			
			// Store bubble information for tooltip
			drawnBubbles.Add(new BubbleDrawing
			{
				BarIndex = barIndex,
				Price = price,
				Volume = volume,
				Type = type,
				Center = new SharpDX.Vector2(x, y),
				Radius = (float)bubbleSize
			});
			
			// Create semi-transparent brush
			Brush bubbleBrush = color.Clone();
			bubbleBrush.Opacity = BubbleOpacity;
			bubbleBrush.Freeze();
			
			// Draw the bubble
			SharpDX.Direct2D1.Brush dxBrush = bubbleBrush.ToDxBrush(RenderTarget);
			SharpDX.Direct2D1.Ellipse ellipse = new SharpDX.Direct2D1.Ellipse(
				new SharpDX.Vector2(x, y),
				(float)bubbleSize,
				(float)bubbleSize
			);
			
			RenderTarget.FillEllipse(ellipse, dxBrush);
			
			// Draw border
			SharpDX.Direct2D1.Brush borderBrush = Brushes.Black.ToDxBrush(RenderTarget);
			RenderTarget.DrawEllipse(ellipse, borderBrush, 1);
			
			// Cleanup
			dxBrush.Dispose();
			borderBrush.Dispose();
		}
		
		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (ChartControl == null || drawnBubbles == null || drawnBubbles.Count == 0)
				return;
			
			// Get mouse position
			Point mousePos = e.GetPosition(ChartControl as IInputElement);
			
			// Check if mouse is over any bubble
			foreach (var bubble in drawnBubbles)
			{
				double distance = Math.Sqrt(
					Math.Pow(mousePos.X - bubble.Center.X, 2) + 
					Math.Pow(mousePos.Y - bubble.Center.Y, 2)
				);
				
				if (distance <= bubble.Radius)
				{
					// Mouse is over this bubble - show tooltip
					if (chartToolTip == null)
					{
						chartToolTip = new System.Windows.Controls.ToolTip();
					}
					
					chartToolTip.Content = string.Format("{0} Volume: {1:N0}", bubble.Type, bubble.Volume);
					chartToolTip.IsOpen = true;
					chartToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse;
					chartToolTip.PlacementTarget = ChartControl as UIElement;
					return;
				}
			}
			
			// Mouse is not over any bubble - hide tooltip
			if (chartToolTip != null)
			{
				chartToolTip.IsOpen = false;
			}
		}
		
		public override string DisplayName
		{
			get { return Name; }
		}

		#region Properties
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Minimum Volume", Description = "Minimum volume threshold to display bubbles", Order = 1, GroupName = "Parameters")]
		public int MinimumVolume
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name = "Ask Bubble Color", Description = "Color for Ask volume bubbles", Order = 2, GroupName = "Parameters")]
		public Brush AskBubbleColor
		{ get; set; }
		
		[Browsable(false)]
		public string AskBubbleColorSerializable
		{
			get { return Serialize.BrushToString(AskBubbleColor); }
			set { AskBubbleColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name = "Bid Bubble Color", Description = "Color for Bid volume bubbles", Order = 3, GroupName = "Parameters")]
		public Brush BidBubbleColor
		{ get; set; }
		
		[Browsable(false)]
		public string BidBubbleColorSerializable
		{
			get { return Serialize.BrushToString(BidBubbleColor); }
			set { BidBubbleColor = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Range(0.1, 1.0)]
		[Display(Name = "Bubble Opacity", Description = "Opacity of the bubbles (0.1 - 1.0)", Order = 4, GroupName = "Parameters")]
		public double BubbleOpacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, 100)]
		[Display(Name = "Minimum Bubble Size", Description = "Minimum bubble radius in pixels", Order = 5, GroupName = "Parameters")]
		public int MinBubbleSize
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, 100)]
		[Display(Name = "Maximum Bubble Size", Description = "Maximum bubble radius in pixels", Order = 6, GroupName = "Parameters")]
		public int MaxBubbleSize
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Show Bid Bubbles", Description = "Show bubbles for Bid volume", Order = 7, GroupName = "Parameters")]
		public bool ShowBidBubbles
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Show Ask Bubbles", Description = "Show bubbles for Ask volume", Order = 8, GroupName = "Parameters")]
		public bool ShowAskBubbles
		{ get; set; }
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private VolumeBubbles[] cacheVolumeBubbles;
		public VolumeBubbles VolumeBubbles(int minimumVolume, double bubbleOpacity, int minBubbleSize, int maxBubbleSize, bool showBidBubbles, bool showAskBubbles)
		{
			return VolumeBubbles(Input, minimumVolume, bubbleOpacity, minBubbleSize, maxBubbleSize, showBidBubbles, showAskBubbles);
		}

		public VolumeBubbles VolumeBubbles(ISeries<double> input, int minimumVolume, double bubbleOpacity, int minBubbleSize, int maxBubbleSize, bool showBidBubbles, bool showAskBubbles)
		{
			if (cacheVolumeBubbles != null)
				for (int idx = 0; idx < cacheVolumeBubbles.Length; idx++)
					if (cacheVolumeBubbles[idx] != null && cacheVolumeBubbles[idx].MinimumVolume == minimumVolume && cacheVolumeBubbles[idx].BubbleOpacity == bubbleOpacity && cacheVolumeBubbles[idx].MinBubbleSize == minBubbleSize && cacheVolumeBubbles[idx].MaxBubbleSize == maxBubbleSize && cacheVolumeBubbles[idx].ShowBidBubbles == showBidBubbles && cacheVolumeBubbles[idx].ShowAskBubbles == showAskBubbles && cacheVolumeBubbles[idx].EqualsInput(input))
						return cacheVolumeBubbles[idx];
			return CacheIndicator<VolumeBubbles>(new VolumeBubbles(){ MinimumVolume = minimumVolume, BubbleOpacity = bubbleOpacity, MinBubbleSize = minBubbleSize, MaxBubbleSize = maxBubbleSize, ShowBidBubbles = showBidBubbles, ShowAskBubbles = showAskBubbles }, input, ref cacheVolumeBubbles);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VolumeBubbles VolumeBubbles(int minimumVolume, double bubbleOpacity, int minBubbleSize, int maxBubbleSize, bool showBidBubbles, bool showAskBubbles)
		{
			return indicator.VolumeBubbles(Input, minimumVolume, bubbleOpacity, minBubbleSize, maxBubbleSize, showBidBubbles, showAskBubbles);
		}

		public Indicators.VolumeBubbles VolumeBubbles(ISeries<double> input , int minimumVolume, double bubbleOpacity, int minBubbleSize, int maxBubbleSize, bool showBidBubbles, bool showAskBubbles)
		{
			return indicator.VolumeBubbles(input, minimumVolume, bubbleOpacity, minBubbleSize, maxBubbleSize, showBidBubbles, showAskBubbles);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VolumeBubbles VolumeBubbles(int minimumVolume, double bubbleOpacity, int minBubbleSize, int maxBubbleSize, bool showBidBubbles, bool showAskBubbles)
		{
			return indicator.VolumeBubbles(Input, minimumVolume, bubbleOpacity, minBubbleSize, maxBubbleSize, showBidBubbles, showAskBubbles);
		}

		public Indicators.VolumeBubbles VolumeBubbles(ISeries<double> input , int minimumVolume, double bubbleOpacity, int minBubbleSize, int maxBubbleSize, bool showBidBubbles, bool showAskBubbles)
		{
			return indicator.VolumeBubbles(input, minimumVolume, bubbleOpacity, minBubbleSize, maxBubbleSize, showBidBubbles, showAskBubbles);
		}
	}
}

#endregion
