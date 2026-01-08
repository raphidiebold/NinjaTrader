# NinjaTrader

## Volume Bubbles Indicator

Ein fortschrittlicher NinjaTrader 8 Indikator, der das gehandelte Volumen als Blasen (Bubbles) historisch und in Echtzeit anzeigt.

### Funktionen

- **Bid und Ask Volumen**: Zeigt separat das Volumen für Bid (Verkaufsdruck) und Ask (Kaufdruck) an
- **Farbcodierung**: 
  - Grün für Ask-Volumen (Käufer)
  - Rot für Bid-Volumen (Verkäufer)
- **Skalierbare Blasen**: Die Größe der Blasen skaliert entsprechend dem gehandelten Volumen
- **Tooltip-Anzeige**: Beim Überfahren mit der Maus über eine Blase wird das genaue Volumen angezeigt
- **Konfigurierbare Parameter**:
  - Minimales Volumen für die Anzeige
  - Farben für Ask und Bid Blasen
  - Transparenz der Blasen
  - Minimale und maximale Blasengröße
  - Einzelne Aktivierung/Deaktivierung von Bid- und Ask-Blasen

### Installation

1. Kopieren Sie die Datei `VolumeBubbles.cs` in Ihren NinjaTrader-Indikator-Ordner:
   ```
   Dokumente\NinjaTrader 8\bin\Custom\Indicators\
   ```

2. Öffnen Sie NinjaTrader 8

3. Gehen Sie zu Tools > Edit NinjaScript > Indicator

4. Öffnen Sie die Datei VolumeBubbles.cs (falls nicht automatisch importiert)

5. Kompilieren Sie das Skript (F5 oder Build-Button)

### Verwendung

1. Öffnen Sie ein Chart in NinjaTrader 8

2. Klicken Sie mit der rechten Maustaste auf das Chart und wählen Sie "Indicators"

3. Wählen Sie "Volume Bubbles" aus der Liste

4. Konfigurieren Sie die Parameter nach Ihren Wünschen:
   - **Minimum Volume**: Minimales Volumen für die Anzeige von Blasen
   - **Ask Bubble Color**: Farbe für Ask-Volumen Blasen (Standard: Grün)
   - **Bid Bubble Color**: Farbe für Bid-Volumen Blasen (Standard: Rot)
   - **Bubble Opacity**: Transparenz der Blasen (0.1 - 1.0)
   - **Minimum Bubble Size**: Minimaler Radius der Blasen in Pixeln
   - **Maximum Bubble Size**: Maximaler Radius der Blasen in Pixeln
   - **Show Bid Bubbles**: Bid-Blasen anzeigen (Ja/Nein)
   - **Show Ask Bubbles**: Ask-Blasen anzeigen (Ja/Nein)

5. Klicken Sie auf "OK" um den Indikator anzuwenden

### Funktionsweise

Der Indikator analysiert jeden Trade und ordnet ihn entweder dem Bid oder Ask zu:
- Trades am oder unter dem Bid-Preis werden als Bid-Volumen gezählt (Verkaufsdruck)
- Trades am oder über dem Ask-Preis werden als Ask-Volumen gezählt (Kaufdruck)
- Trades zwischen Bid und Ask werden basierend auf ihrer Position relativ zum Mittelpunkt zugeordnet

Die Blasen werden in Echtzeit aktualisiert und zeigen das akkumulierte Volumen für jede Kerze/Bar an.

### Systemanforderungen

- NinjaTrader 8
- .NET Framework 4.8 oder höher
- Marktdaten mit Bid/Ask-Informationen

### Hinweise

- Der Indikator funktioniert am besten mit Tick- oder Volumen-basierten Charts
- Für historische Daten werden die Blasen basierend auf verfügbaren Bid/Ask-Daten angezeigt
- In Echtzeit werden die Blasen kontinuierlich aktualisiert, wenn neue Trades stattfinden

### Support

Bei Fragen oder Problemen erstellen Sie bitte ein Issue im GitHub-Repository.
