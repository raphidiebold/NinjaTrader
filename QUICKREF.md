# Volume Bubbles - Schnellreferenz

## Grundlegende Verwendung

### Installation (3 Schritte)
1. Datei `VolumeBubbles.cs` nach `Dokumente\NinjaTrader 8\bin\Custom\Indicators\` kopieren
2. NinjaScript Editor öffnen: Tools → Edit NinjaScript → Indicator
3. F5 drücken zum Kompilieren

### Zum Chart hinzufügen
Rechtsklick auf Chart → Indicators → Volume Bubbles auswählen

## Wichtigste Parameter

| Parameter | Empfehlung | Zweck |
|-----------|------------|-------|
| **Minimum Volume** | 50-200 | Filtert kleine Trades |
| **Bubble Opacity** | 0.5-0.7 | Transparenz einstellen |
| **Min/Max Size** | 5/30 | Blasengröße begrenzen |

## Farbschema

- 🟢 **Grün** = Ask (Käufer, Kaufdruck)
- 🔴 **Rot** = Bid (Verkäufer, Verkaufsdruck)

## Schnell-Einstellungen

### Day Trading (schnell)
```
Minimum Volume: 50
Opacity: 0.5
Size: 3-20
```

### Swing Trading (langsam)
```
Minimum Volume: 500
Opacity: 0.7
Size: 8-35
```

### Volumen-Spitzen finden
```
Minimum Volume: 1000+
Nur große Trades anzeigen
```

## Interpretation

### Bullish Signale
- Große grüne Blasen (Ask)
- Steigende Ask-Volumina
- Ask > Bid bei steigendem Preis

### Bearish Signale
- Große rote Blasen (Bid)
- Steigende Bid-Volumina
- Bid > Ask bei fallendem Preis

### Umkehr-Signale
- Volumendivergenz zu Preis
- Plötzlicher Wechsel Bid ↔ Ask
- Volumen-Spitzen an Extremen

## Tipps

✓ **Tooltip verwenden**: Maus über Blase = genaues Volumen  
✓ **Minimum Volume anpassen**: Zu viele Blasen = höherer Wert  
✓ **Mit MA kombinieren**: Bessere Trend-Bestätigung  
✓ **Support/Resistance**: Große Blasen = wichtige Levels  

## Häufige Probleme

❌ **Keine Blasen?** → Minimum Volume senken  
❌ **Zu viele Blasen?** → Minimum Volume erhöhen  
❌ **Nicht sichtbar?** → Farbe ändern oder Opacity erhöhen  
❌ **Performance?** → Minimum Volume erhöhen, Max Size verkleinern  

## Tastenkombinationen

- **F5** - NinjaScript kompilieren
- **F6** - Indikator-Dialog öffnen
- **Strg + I** - Indikator hinzufügen

## Chart-Typen

✓ Tick Charts (beste Präzision)  
✓ Minute Charts (gut)  
✓ Volume Charts (gut)  
✓ Range Charts (okay)  

## Trading-Strategien (Kurzversion)

### 1. Ausbruch mit Volumen
Preis bricht aus → Große grüne Blase = Long  
Preis bricht aus → Große rote Blase = Short

### 2. Volumen-Bestätigung
Trend + passende Blasenfarbe = Fortsetzung wahrscheinlich  
Trend + gegenteilige Farbe = Umkehr möglich

### 3. Support/Resistance
Viele große Blasen = wichtiger Preisbereich  
Nutzen für Ein-/Ausstiege

## Weitere Dokumentation

- **README.md** - Vollständige Übersicht
- **INSTALLATION.md** - Detaillierte Installation
- **EXAMPLES.md** - Ausführliche Beispiele
- **TECHNICAL.md** - Technische Details

## Support

Probleme oder Fragen?  
→ GitHub Issue erstellen  
→ Dokumentation durchlesen  
→ Parameter anpassen und testen

---

**Quick Tip**: Starten Sie mit Standard-Einstellungen und passen Sie dann schrittweise an Ihren Trading-Stil an!
