// Example: How to use Volume Bubbles Indicator in a NinjaTrader Strategy
// Beispiel: Verwendung des Volume Bubbles Indicators in einer NinjaTrader-Strategie

#region Using declarations
using System;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Strategies;
#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
    public class VolumeBubblesExample : Strategy
    {
        private VolumeBubblesIndicator volumeBubbles;
        
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Beispiel-Strategie mit Volume Bubbles Indicator";
                Name = "Volume Bubbles Example Strategy";
                Calculate = Calculate.OnEachTick;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 20;
            }
            else if (State == State.Configure)
            {
            }
            else if (State == State.DataLoaded)
            {
                // Initialize Volume Bubbles Indicator
                // Method 1: Default settings
                volumeBubbles = VolumeBubblesIndicator(100, 20, 5, 0.6, true);
                
                // Method 2: Custom settings (commented out)
                // volumeBubbles = VolumeBubblesIndicator(500, 25, 8, 0.7, true);
                
                AddChartIndicator(volumeBubbles);
            }
        }

        protected override void OnBarUpdate()
        {
            // This is just an example - actual trading logic depends on your strategy
            
            // Example 1: Simple volume-based entry
            // Note: The indicator itself doesn't provide direct volume values,
            // but you can use it for visual confirmation
            
            if (CurrentBar < BarsRequiredToTrade)
                return;
                
            // Your trading logic here
            // The Volume Bubbles Indicator is mainly for visualization
            // You would typically combine it with other indicators for entries
            
            // Example placeholder logic:
            // if (Close[0] > Close[1] && Volume[0] > Volume[1] * 2)
            // {
            //     EnterLong("Long Entry");
            // }
        }
    }
}

/* 
 * =============================================================================
 * USAGE EXAMPLES / VERWENDUNGSBEISPIELE
 * =============================================================================
 * 
 * 1. BASIC USAGE IN CHART / GRUNDLEGENDE VERWENDUNG IM CHART
 *    --------------------------------------------------------------
 *    - Right-click on chart / Rechtsklick auf Chart
 *    - Select "Indicators..." / "Indikatoren..." wählen
 *    - Find "Volume Bubbles Indicator"
 *    - Add with default settings / Mit Standardeinstellungen hinzufügen
 * 
 * 2. CUSTOM SETTINGS / BENUTZERDEFINIERTE EINSTELLUNGEN
 *    --------------------------------------------------------------
 *    In the indicator properties dialog:
 *    
 *    For Day Trading:
 *    - Minimum Volume: 50-200
 *    - Max Bubble Size: 20-25
 *    - Min Bubble Size: 5-8
 *    
 *    For Swing Trading:
 *    - Minimum Volume: 200-500
 *    - Max Bubble Size: 25-30
 *    - Min Bubble Size: 8-10
 *    
 *    For Scalping:
 *    - Minimum Volume: 10-50
 *    - Max Bubble Size: 15-20
 *    - Min Bubble Size: 3-5
 * 
 * 3. PROGRAMMATIC USAGE / PROGRAMMATISCHE VERWENDUNG
 *    --------------------------------------------------------------
 *    // In a strategy or indicator:
 *    private VolumeBubblesIndicator myVolumeBubbles;
 *    
 *    // In OnStateChange (State.DataLoaded):
 *    myVolumeBubbles = VolumeBubblesIndicator(
 *        100,     // MinimumVolume
 *        20,      // MaxBubbleSize
 *        5,       // MinBubbleSize
 *        0.6,     // BubbleOpacity
 *        true     // ShowVolumeLabel
 *    );
 *    AddChartIndicator(myVolumeBubbles);
 * 
 * 4. INTERPRETATION / INTERPRETATION
 *    --------------------------------------------------------------
 *    Green Bubbles (Ask/Buy):
 *    - Large green bubble = Strong buying pressure
 *    - Multiple green bubbles = Accumulation
 *    - Green bubbles at resistance = Potential breakout
 *    
 *    Red Bubbles (Bid/Sell):
 *    - Large red bubble = Strong selling pressure
 *    - Multiple red bubbles = Distribution
 *    - Red bubbles at support = Potential breakdown
 *    
 *    Grüne Blasen (Ask/Kauf):
 *    - Große grüne Blase = Starker Kaufdruck
 *    - Mehrere grüne Blasen = Akkumulation
 *    - Grüne Blasen am Widerstand = Möglicher Ausbruch
 *    
 *    Rote Blasen (Bid/Verkauf):
 *    - Große rote Blase = Starker Verkaufsdruck
 *    - Mehrere rote Blasen = Distribution
 *    - Rote Blasen an Unterstützung = Möglicher Durchbruch
 * 
 * 5. TRADING SCENARIOS / HANDELSSZENARIEN
 *    --------------------------------------------------------------
 *    
 *    Scenario 1: Breakout Confirmation
 *    - Price approaches resistance
 *    - Large green bubbles appear
 *    - Price breaks resistance → Entry signal
 *    
 *    Scenario 2: Reversal Warning
 *    - Price in uptrend
 *    - Large red bubbles appear at highs
 *    - Consider profit-taking or exit
 *    
 *    Scenario 3: Volume Cluster
 *    - Multiple bubbles at same price level
 *    - Indicates strong support/resistance
 *    - Use for stop-loss placement
 *    
 *    Szenario 1: Ausbruchsbestätigung
 *    - Preis nähert sich Widerstand
 *    - Große grüne Blasen erscheinen
 *    - Preis durchbricht Widerstand → Einstiegssignal
 *    
 *    Szenario 2: Umkehrwarnung
 *    - Preis im Aufwärtstrend
 *    - Große rote Blasen bei Hochs
 *    - Gewinnmitnahme oder Ausstieg erwägen
 *    
 *    Szenario 3: Volumen-Cluster
 *    - Mehrere Blasen auf gleichem Preisniveau
 *    - Zeigt starke Unterstützung/Widerstand
 *    - Verwenden für Stop-Loss-Platzierung
 * 
 * 6. COMBINATION WITH OTHER INDICATORS / KOMBINATION MIT ANDEREN INDIKATOREN
 *    --------------------------------------------------------------
 *    
 *    Effective combinations / Effektive Kombinationen:
 *    
 *    a) Volume Profile + Volume Bubbles
 *       - Volume Profile shows cumulative volume
 *       - Volume Bubbles show individual large trades
 *       
 *    b) VWAP + Volume Bubbles
 *       - VWAP as price reference
 *       - Volume Bubbles for entry timing
 *       
 *    c) Moving Averages + Volume Bubbles
 *       - MAs for trend direction
 *       - Volume Bubbles for entry confirmation
 *       
 *    d) Order Flow + Volume Bubbles
 *       - Order Flow for market structure
 *       - Volume Bubbles for large player activity
 * 
 * 7. BEST PRACTICES / BESTE PRAKTIKEN
 *    --------------------------------------------------------------
 *    
 *    ✓ Start with higher minimum volume (100-500)
 *    ✓ Adjust colors for your chart theme
 *    ✓ Use with tick data for best results
 *    ✓ Combine with price action analysis
 *    ✓ Don't trade on bubbles alone
 *    ✓ Consider market context
 *    
 *    ✗ Don't use too low minimum volume (visual clutter)
 *    ✗ Don't ignore price structure
 *    ✗ Don't trade every bubble
 *    ✗ Don't use on historical minute bars (needs tick data)
 * 
 * 8. MARKET-SPECIFIC SETTINGS / MARKTSPEZIFISCHE EINSTELLUNGEN
 *    --------------------------------------------------------------
 *    
 *    ES (E-mini S&P 500):
 *    - Minimum Volume: 100-300
 *    
 *    NQ (E-mini NASDAQ):
 *    - Minimum Volume: 50-150
 *    
 *    YM (E-mini Dow):
 *    - Minimum Volume: 50-100
 *    
 *    CL (Crude Oil):
 *    - Minimum Volume: 100-200
 *    
 *    Forex Futures:
 *    - Minimum Volume: 200-500
 * 
 * =============================================================================
 */
