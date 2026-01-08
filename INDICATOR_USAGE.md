# Volume Bubbles Indicator - Benutzerhandbuch / User Guide

## Beschreibung / Description

Der **Volume Bubbles Indicator** ist ein NinjaTrader-Indikator, der gehandelte Volumina in Echtzeit und historisch als skalierbare Blasen anzeigt. Der Indikator unterscheidet zwischen Bid- und Ask-Volumina und stellt diese in verschiedenen Farben dar.

The **Volume Bubbles Indicator** is a NinjaTrader indicator that displays traded volumes in real-time and historically as scalable bubbles. The indicator differentiates between Bid and Ask volumes and displays them in different colors.

## Features / Funktionen

- ✅ **Echtzeit- und historische Volumenanzeige** / Real-time and historical volume display
- ✅ **Bid/Ask-Unterscheidung** / Bid/Ask differentiation
  - Bid: Rot (Red)
  - Ask: Grün (Green)
- ✅ **Skalierbare Blasen** / Scalable bubbles based on volume size
- ✅ **Volumen-Tooltip** / Volume display on hover (via labels)
- ✅ **Konfigurierbare Parameter** / Configurable parameters
  - Mindestvolumen / Minimum volume threshold
  - Blasenfarben / Bubble colors
  - Blasengröße / Bubble size (min/max)
  - Transparenz / Opacity

## Installation

1. Kopieren Sie die Datei `VolumeBubblesIndicator.cs` in Ihren NinjaTrader Indicators-Ordner:
   ```
   Documents\NinjaTrader 8\bin\Custom\Indicators\
   ```

2. Kompilieren Sie den Indikator in NinjaTrader:
   - Öffnen Sie NinjaTrader
   - Gehen Sie zu Tools → Edit NinjaScript → Indicator
   - Wählen Sie "Compile" (oder drücken Sie F5)

3. Der Indikator ist nun unter dem Namen "Volume Bubbles Indicator" verfügbar

## Verwendung / Usage

### Indikator hinzufügen / Adding the Indicator

1. Öffnen Sie ein Chart in NinjaTrader
2. Klicken Sie mit der rechten Maustaste auf das Chart
3. Wählen Sie "Indicators..."
4. Suchen Sie nach "Volume Bubbles Indicator"
5. Fügen Sie den Indikator hinzu

### Parameter / Parameters

#### Minimum Volume (Mindestvolumen)
- **Standard / Default:** 100
- **Beschreibung:** Nur Trades mit diesem oder höherem Volumen werden als Blasen angezeigt
- **Description:** Only trades with this volume or higher will be displayed as bubbles

#### Max Bubble Size (Maximale Blasengröße)
- **Standard / Default:** 20 Pixel
- **Beschreibung:** Maximale Größe der Blasen in Pixeln
- **Description:** Maximum size of bubbles in pixels

#### Min Bubble Size (Minimale Blasengröße)
- **Standard / Default:** 5 Pixel
- **Beschreibung:** Minimale Größe der Blasen in Pixeln
- **Description:** Minimum size of bubbles in pixels

#### Bid Color (Bid-Farbe)
- **Standard / Default:** Rot / Red
- **Beschreibung:** Farbe für Verkaufs-Trades (Bid)
- **Description:** Color for sell trades (Bid)

#### Ask Color (Ask-Farbe)
- **Standard / Default:** Grün / Green
- **Beschreibung:** Farbe für Kauf-Trades (Ask)
- **Description:** Color for buy trades (Ask)

#### Bubble Opacity (Blasen-Transparenz)
- **Standard / Default:** 0.6 (60%)
- **Beschreibung:** Transparenz der Blasen (0.1 = sehr transparent, 1.0 = völlig undurchsichtig)
- **Description:** Transparency of bubbles (0.1 = very transparent, 1.0 = completely opaque)

#### Show Volume Label (Volumen-Beschriftung anzeigen)
- **Standard / Default:** True (Ja / Yes)
- **Beschreibung:** Zeigt das Volumen als Text auf größeren Blasen an
- **Description:** Displays the volume as text on larger bubbles

## Beispielkonfigurationen / Example Configurations

### Für große Trades / For Large Trades
- Minimum Volume: 500
- Max Bubble Size: 30
- Min Bubble Size: 10

### Für alle Trades / For All Trades
- Minimum Volume: 1
- Max Bubble Size: 20
- Min Bubble Size: 3

### Für Futures Trading
- Minimum Volume: 100
- Max Bubble Size: 25
- Min Bubble Size: 5

## Technische Details / Technical Details

### Wie funktioniert der Indikator? / How does the indicator work?

1. **Datenerfassung / Data Collection:** 
   - Der Indikator überwacht Market Data Events (`OnMarketData`)
   - Erfasst Volumen, Preis und Zeitstempel jedes Trades
   - Unterscheidet zwischen Bid und Ask basierend auf dem Preis

2. **Visualisierung / Visualization:**
   - Verwendet `OnRender` für benutzerdefinierte Darstellung
   - Zeichnet Kreise (Ellipsen) mit SharpDX
   - Skaliert die Blasengröße logarithmisch basierend auf dem Volumen

3. **Performance:**
   - Speichert nur Trades über dem Mindestvolumen
   - Rendert nur sichtbare Blasen (im aktuellen Chart-Bereich)

## Ähnliche Indikatoren / Similar Indicators

Dieser Indikator ist inspiriert vom **LargeTradeDetector** aus der OrderFlow+ Software von NinjaTrader und bietet ähnliche Funktionalität mit zusätzlicher Anpassbarkeit.

This indicator is inspired by the **LargeTradeDetector** from NinjaTrader's OrderFlow+ software and offers similar functionality with additional customizability.

## Fehlerbehebung / Troubleshooting

### Keine Blasen werden angezeigt / No bubbles are displayed
- Überprüfen Sie, ob das Mindestvolumen nicht zu hoch eingestellt ist
- Stellen Sie sicher, dass Sie Live-Daten oder Tick-Daten verwenden
- Der Indikator benötigt Market Data Events

### Blasen sind zu klein/groß / Bubbles are too small/large
- Passen Sie die Parameter "Min Bubble Size" und "Max Bubble Size" an
- Reduzieren Sie das Mindestvolumen für mehr Blasen

### Indicator kompiliert nicht / Indicator doesn't compile
- Stellen Sie sicher, dass Sie NinjaTrader 8 verwenden
- Überprüfen Sie, ob alle using-Statements korrekt sind
- Prüfen Sie die Fehlerausgabe im NinjaScript Editor

## Support

Für weitere Fragen oder Probleme, öffnen Sie bitte ein Issue im GitHub Repository.

For questions or issues, please open an issue in the GitHub repository.

## Lizenz / License

Dieser Indikator ist Open Source und kann frei verwendet und angepasst werden.

This indicator is open source and can be freely used and modified.
