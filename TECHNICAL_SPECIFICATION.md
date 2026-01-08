# Volume Bubbles Indicator - Technische Spezifikation

## Architektur / Architecture

### Hauptkomponenten / Main Components

1. **VolumeBubble Klasse** (Datenstruktur)
   - Speichert Informationen über jedes erkannte Volumen-Event
   - Eigenschaften: BarIndex, Price, Volume, IsAsk, Time

2. **Datenspeicherung**
   - `volumeBubbles`: Liste aller erkannten Volumina
   - `bidVolumeByBar`: Aggregierte Bid-Volumina pro Bar und Preis
   - `askVolumeByBar`: Aggregierte Ask-Volumina pro Bar und Preis

3. **Event-Handler**
   - `OnMarketData`: Erfasst Echtzeit-Tick-Daten
   - `OnRender`: Zeichnet Blasen auf dem Chart
   - `OnBarUpdate`: Verwaltet Bar-basierte Datenstrukturen

### Datenfluss / Data Flow

```
Market Data Event
    ↓
OnMarketData()
    ↓
Volumen ≥ Minimum? → Nein → Verwerfen
    ↓ Ja
VolumeBubble erstellen
    ↓
Bid/Ask bestimmen
    ↓
In volumeBubbles Liste speichern
    ↓
OnRender() - bei jedem Chart-Update
    ↓
Bubble zeichnen (SharpDX)
```

## Bubble-Skalierung / Bubble Scaling

### Logarithmische Skalierung

Die Blasengröße wird logarithmisch skaliert, um eine bessere visuelle Darstellung zu erreichen.
Es enthält Schutz vor Division durch Null und ungültigen Logarithmus-Operationen:

```csharp
if (MinimumVolume <= 1 || volume < MinimumVolume)
    return MinBubbleSize;

double scaleFactor = Math.Log10(volume) / Math.Log10(MinimumVolume);
double size = MinBubbleSize + (scaleFactor * (MaxBubbleSize - MinBubbleSize));
```

**Wichtig:** MinimumVolume sollte mindestens 2 sein, um korrekte Skalierung zu gewährleisten.

### Beispiele / Examples

Bei MinimumVolume = 100, MinBubbleSize = 5, MaxBubbleSize = 20:

| Volumen | Skalierungsfaktor | Blasengröße |
|---------|-------------------|-------------|
| 100     | 1.0               | 5 px        |
| 200     | 1.15              | 7.25 px     |
| 500     | 1.35              | 10.25 px    |
| 1000    | 1.5               | 12.5 px     |
| 5000    | 1.85              | 17.75 px    |
| 10000   | 2.0               | 20 px       |

## Bid/Ask Erkennung / Bid/Ask Detection

Die Unterscheidung zwischen Bid und Ask erfolgt in mehreren Schritten:

**Primär:** Vergleich mit dem Midpoint zwischen Bid und Ask
```csharp
if (marketDataUpdate.Ask > 0 && marketDataUpdate.Bid > 0)
{
    double midPrice = (marketDataUpdate.Ask + marketDataUpdate.Bid) / 2;
    isAsk = marketDataUpdate.Price >= midPrice;
}
```

**Fallback:** Vergleich mit dem vorherigen Close-Preis, wenn Bid/Ask nicht verfügbar
```csharp
else if (CurrentBar > 0)
{
    isAsk = marketDataUpdate.Price >= Close[0];
}
```

- **Ask (Kauf)**: Preis >= Midpoint oder >= Previous Close → Grüne Blase
- **Bid (Verkauf)**: Preis < Midpoint oder < Previous Close → Rote Blase

## Rendering-Pipeline

### 1. Bubble-Filterung
- Nur Blasen im sichtbaren Chart-Bereich werden gerendert
- Performance-Optimierung durch Bereichsprüfung

### 2. Koordinaten-Berechnung
```csharp
int x = chartControl.GetXByBarIndex(ChartBars, bubble.BarIndex);
int y = chartScale.GetYByValue(bubble.Price);
```

### 3. Bubble-Zeichnung
- Hauptkreis mit konfigurierbarer Farbe und Transparenz
- Umrandung in dunklerer Farbe (DarkGreen/DarkRed)
- Optional: Volumen-Text auf größeren Blasen

### 4. Text-Formatierung
- Volumen > 1M: "X.XM"
- Volumen > 1K: "X.XK"
- Volumen < 1K: Ganzzahl

## Performance-Überlegungen / Performance Considerations

### Optimierungen / Optimizations

1. **Selektive Speicherung**
   - Nur Volumina über Schwellenwert werden gespeichert
   - Reduziert Speicherverbrauch

2. **Sichtbarkeitsfilterung**
   - Nur sichtbare Blasen werden gerendert
   - Verbessert Rendering-Performance

3. **Resource Management**
   - SharpDX-Ressourcen werden nach Gebrauch entsorgt
   - Verhindert Memory Leaks

### Speicherbedarf / Memory Usage

Pro VolumeBubble: ~40 Bytes
- int (4) + double (8) + long (8) + bool (1) + DateTime (8) + Overhead (~11)

Bei 1000 Trades über Schwellenwert: ~40 KB

## Konfigurationsbeispiele / Configuration Examples

### Konservativ (nur große Trades)
```
MinimumVolume: 1000
MaxBubbleSize: 30
MinBubbleSize: 10
BubbleOpacity: 0.7
```

### Aggressiv (alle Trades)
```
MinimumVolume: 1
MaxBubbleSize: 15
MinBubbleSize: 3
BubbleOpacity: 0.4
```

### Balanced (empfohlen)
```
MinimumVolume: 100
MaxBubbleSize: 20
MinBubbleSize: 5
BubbleOpacity: 0.6
```

## Integration mit NinjaTrader

### Erforderliche NinjaTrader-Versionen
- NinjaTrader 8.0 oder höher
- .NET Framework 4.8 oder höher

### Abhängigkeiten
- SharpDX.Direct2D1
- SharpDX.DirectWrite
- NinjaTrader.Core
- NinjaTrader.Gui

### Installation
1. Indicator-Datei in Custom/Indicators kopieren
2. NinjaScript kompilieren (F5)
3. NinjaTrader neu starten (optional)

## Erweiterte Anwendungsfälle / Advanced Use Cases

### Kombination mit anderen Indikatoren
Der Volume Bubbles Indicator kann kombiniert werden mit:
- Volume Profile
- Market Delta
- Order Flow Indikatoren
- VWAP

### Handelsstrategien / Trading Strategies
1. **Breakout Erkennung**: Große grüne Blasen bei Widerständen
2. **Volumen-Cluster**: Mehrere Blasen auf ähnlichem Preisniveau
3. **Divergenzen**: Preis steigt, aber große rote Blasen erscheinen

## Fehlerbehebung / Troubleshooting

### Häufige Probleme / Common Issues

1. **Keine Blasen sichtbar**
   - Lösung: MinimumVolume reduzieren
   - Lösung: Sicherstellen, dass Tick-Daten verfügbar sind

2. **Performance-Probleme**
   - Lösung: MinimumVolume erhöhen
   - Lösung: BubbleOpacity reduzieren

3. **Falsche Farben**
   - Lösung: Bid/Ask-Erkennung überprüfen
   - Lösung: Instrument-Einstellungen prüfen

## Zukünftige Erweiterungen / Future Enhancements

Mögliche Verbesserungen:
- [ ] Historische Daten-Unterstützung über Tick-Replay
- [ ] Audio-Alarme bei großen Trades
- [ ] Export von Volumen-Daten
- [ ] Filter nach Zeitbereichen
- [ ] Aggregation von nahen Trades
- [ ] Verschiedene Bubble-Formen (Rechteck, Stern)
- [ ] Heatmap-Modus für Volumen-Dichte

## Lizenz und Haftungsausschluss

Dieser Indikator wird "wie besehen" bereitgestellt. Der Autor übernimmt keine Haftung für Handelsverluste oder technische Probleme.

This indicator is provided "as is". The author assumes no liability for trading losses or technical issues.
