# Volume Bubbles Indikator - Technische Dokumentation

## Übersicht

Der **Volume Bubbles** Indikator ist ein vollständig funktionsfähiger NinjaTrader 8 Indikator, der gehandelte Volumina als visuelle Blasen darstellt. Der Indikator unterscheidet zwischen Bid- (Verkaufs-) und Ask- (Kauf-) Volumen und bietet umfangreiche Konfigurationsmöglichkeiten.

## Hauptmerkmale

### 1. Volumen-Tracking
- **Echtzeit-Daten**: Verwendet `OnMarketData()` für präzise Volumenerfassung bei jedem Trade
- **Historische Daten**: Approximiert Bid/Ask-Verteilung basierend auf der Position des Schlusskurses
- **Intelligente Zuordnung**: Trades werden basierend auf ihrer Preisposition relativ zu Bid/Ask zugeordnet

### 2. Visuelle Darstellung
- **Skalierbare Blasen**: Größe proportional zum Volumen
- **Farbcodierung**: 
  - Grün für Ask (Kaufvolumen)
  - Rot für Bid (Verkaufsvolumen)
- **Transparenz**: Konfigurierbare Opacity für bessere Übersicht
- **Schwarzer Rand**: Verbesserte Sichtbarkeit der Blasen

### 3. Interaktivität
- **Tooltip-System**: Zeigt exaktes Volumen beim Überfahren
- **Echtzeit-Updates**: Blasen wachsen mit zunehmendem Volumen
- **Smooth Rendering**: Verwendet SharpDX für hardwarebeschleunigte Grafiken

### 4. Performance-Optimierungen
- **Memory Management**: Automatische Bereinigung alter Daten
- **Sichtbarkeits-Filterung**: Zeichnet nur sichtbare Blasen
- **Effiziente Datenstrukturen**: Dictionary für schnellen Zugriff

## Technische Implementierung

### Klassen und Strukturen

#### VolumeData
```csharp
private class VolumeData
{
    public double BidVolume { get; set; }
    public double AskVolume { get; set; }
    public double Price { get; set; }
    public int BarIndex { get; set; }
}
```
Speichert das akkumulierte Volumen für jede Bar.

#### BubbleDrawing
```csharp
private class BubbleDrawing
{
    public int BarIndex { get; set; }
    public double Price { get; set; }
    public double Volume { get; set; }
    public string Type { get; set; }
    public SharpDX.Vector2 Center { get; set; }
    public float Radius { get; set; }
}
```
Verwaltet gezeichnete Blasen für Tooltip-Funktionalität.

### Haupt-Methoden

#### OnStateChange()
- **SetDefaults**: Initialisiert Standard-Parameter
- **DataLoaded**: Bereinigt Datenstrukturen
- **Historical**: Registriert MouseMove-Event-Handler
- **Terminated**: Deregistriert Event-Handler

#### OnBarUpdate()
- Verarbeitet historische Daten
- Approximiert Bid/Ask-Verteilung basierend auf OHLC

#### OnMarketData()
- Verarbeitet Echtzeit-Trades
- Ordnet Volumen zu Bid oder Ask zu
- Validiert Daten auf NaN-Werte

#### OnRender()
- Zeichnet Blasen für sichtbare Bars
- Verwaltet Bubble-Liste für Tooltips
- Ruft Memory-Cleanup auf

#### DrawVolumeBubble()
- Berechnet Blasengröße basierend auf Volumen
- Rendert Bubble mit SharpDX
- Speichert Bubble-Informationen für Tooltips

#### OnMouseMove()
- Erkennt Mausposition über Blasen
- Zeigt/versteckt Tooltip entsprechend
- Berechnet Distanz zur Blasenmitte

#### CleanupOldData()
- Entfernt nicht mehr sichtbare Daten
- Verhindert Memory-Leaks
- Bewahrt Buffer für Scroll-Performance

## Konfigurierbare Parameter

| Parameter | Typ | Standard | Beschreibung |
|-----------|-----|----------|--------------|
| MinimumVolume | int | 100 | Mindestvolumen für Anzeige |
| AskBubbleColor | Brush | Green | Farbe für Ask-Blasen |
| BidBubbleColor | Brush | Red | Farbe für Bid-Blasen |
| BubbleOpacity | double | 0.6 | Transparenz (0.1-1.0) |
| MinBubbleSize | int | 5 | Minimaler Radius in Pixeln |
| MaxBubbleSize | int | 30 | Maximaler Radius in Pixeln |
| ShowBidBubbles | bool | true | Bid-Blasen anzeigen |
| ShowAskBubbles | bool | true | Ask-Blasen anzeigen |

## Volumen-Zuordnungslogik

### Echtzeit-Trades
```
Wenn Preis <= Bid: Bid-Volumen
Wenn Preis >= Ask: Ask-Volumen
Wenn Bid < Preis < Ask:
    Wenn Preis > Mittelwert: Ask-Volumen
    Sonst: Bid-Volumen
```

### Historische Daten
```
Position = (Close - Low) / (High - Low)
Ask-Volumen = Total-Volumen × Position
Bid-Volumen = Total-Volumen × (1 - Position)
```

## Blasen-Skalierung

Die Größe wird wie folgt berechnet:
```
normalizedVolume = min(volume / (MinimumVolume × 10), 1.0)
bubbleSize = MinBubbleSize + (normalizedVolume × (MaxBubbleSize - MinBubbleSize))
```

Dies sorgt für:
- Progressive Skalierung mit zunehmendem Volumen
- Begrenzung auf MaxBubbleSize
- Sichtbare Unterschiede auch bei kleinen Volumenänderungen

## Performance-Charakteristiken

### Memory-Nutzung
- Dynamisch basierend auf sichtbarem Bereich
- Automatische Bereinigung alter Daten
- Buffer von 100 Bars für Scroll-Performance

### Rendering-Performance
- Nur sichtbare Bars werden gezeichnet
- Hardwarebeschleunigtes Rendering via SharpDX
- Effiziente Event-Handler-Registrierung

### Daten-Verarbeitung
- O(1) Dictionary-Zugriff für Bar-Daten
- O(n) Rendering wobei n = Anzahl sichtbarer Bars
- Minimale Overhead bei Market-Data-Events

## Kompatibilität

### Unterstützte Versionen
- NinjaTrader 8.0 und höher
- .NET Framework 4.8 oder höher

### Unterstützte Chart-Typen
- Tick Charts
- Minute Charts
- Volume Charts
- Range Charts
- Second Charts

### Unterstützte Datenfeeds
- Kinetick
- CQG
- Rithmic
- Interactive Brokers
- Alle anderen Feeds mit Bid/Ask-Daten

## Einschränkungen

1. **Historische Daten**: Nur Approximation möglich, nicht so präzise wie Echtzeit
2. **Replay-Modus**: Funktioniert, aber abhängig von verfügbaren Replay-Daten
3. **Sehr hohe Volumen**: Könnten UI bei sehr kleinen Minimum-Werten überlasten
4. **Tick-Daten**: Benötigt Tick-Daten für präzise Echtzeit-Zuordnung

## Best Practices

1. **Minimum-Volumen anpassen**: An Instrument und Zeitrahmen anpassen
2. **Farben kontrastreich wählen**: Für bessere Sichtbarkeit
3. **Bubble-Größe limitieren**: Zu große Blasen können Chart überdecken
4. **Mit anderen Indikatoren kombinieren**: Für bessere Trading-Entscheidungen
5. **Regelmäßig Einstellungen optimieren**: Marktbedingungen ändern sich

## Fehlerbehebung

### Keine Blasen sichtbar
1. Minimum-Volumen zu hoch gesetzt
2. Show-Flags deaktiviert
3. Keine ausreichenden Marktdaten
4. Farben identisch mit Hintergrund

### Performance-Probleme
1. Minimum-Volumen zu niedrig
2. Zu große Max-Bubble-Size
3. Sehr lange historische Periode geladen

### Tooltip erscheint nicht
1. Blasen zu klein
2. Maus-Bewegung zu schnell
3. Bubble-Radius zu klein für Hit-Test

## Zukünftige Erweiterungen

Mögliche Verbesserungen:
- [ ] Volume-Profile-Integration
- [ ] Cluster-Analyse
- [ ] Alert-System bei großen Volumina
- [ ] Export-Funktion für Volumen-Daten
- [ ] Delta-Berechnung (Ask - Bid)
- [ ] Kumulative Volumendarstellung
- [ ] Multi-Timeframe-Analyse

## Support und Beiträge

- **GitHub Repository**: Für Issues und Pull Requests
- **Dokumentation**: README.md, INSTALLATION.md, EXAMPLES.md
- **Code-Qualität**: CodeQL-geprüft, keine Sicherheitslücken
- **Best Practices**: Folgt NinjaTrader 8 Coding-Standards

## Lizenz

Siehe Repository-Lizenz für Details.

## Autoren

Entwickelt für NinjaTrader-Nutzer zur Verbesserung der Volumen-Analyse.

---

**Version**: 1.0.0  
**Letzte Aktualisierung**: Januar 2026  
**Kompatibilität**: NinjaTrader 8.0+
