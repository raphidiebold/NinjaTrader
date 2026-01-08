# Installation und Konfiguration - Volume Bubbles Indikator

## Schritt-für-Schritt Installation

### 1. Datei kopieren

Kopieren Sie die Datei `VolumeBubbles.cs` in den NinjaTrader Indicators-Ordner:

**Windows Standard-Pfad:**
```
C:\Users\[IhrBenutzername]\Documents\NinjaTrader 8\bin\Custom\Indicators\
```

### 2. NinjaScript Editor öffnen

1. Öffnen Sie NinjaTrader 8
2. Gehen Sie zu **Tools** → **Edit NinjaScript** → **Indicator**
3. Die Datei sollte in der Liste erscheinen als "VolumeBubbles"

### 3. Kompilieren

1. Drücken Sie **F5** oder klicken Sie auf den **Compile** Button
2. Überprüfen Sie das Output-Fenster auf Fehler
3. Bei erfolgreicher Kompilierung schließen Sie den Editor

### 4. Indikator hinzufügen

1. Öffnen oder erstellen Sie ein Chart
2. Rechtsklick auf das Chart
3. Wählen Sie **Indicators...**
4. Suchen Sie nach "Volume Bubbles"
5. Doppelklicken oder auf **Add** klicken

## Empfohlene Einstellungen

### Für Day Trading (kleine Zeiteinheiten)

```
Minimum Volume: 50-100
Ask Bubble Color: Lime (hell grün)
Bid Bubble Color: Red
Bubble Opacity: 0.5-0.7
Minimum Bubble Size: 3-5
Maximum Bubble Size: 20-25
```

### Für Swing Trading (größere Zeiteinheiten)

```
Minimum Volume: 500-1000
Ask Bubble Color: Green
Bid Bubble Color: DarkRed
Bubble Opacity: 0.6-0.8
Minimum Bubble Size: 5-8
Maximum Bubble Size: 30-40
```

### Für Volumenanalyse

```
Minimum Volume: 100
Show Bid Bubbles: Ja
Show Ask Bubbles: Ja
Bubble Opacity: 0.4 (für Überlagerungen sichtbar)
```

## Fehlerbehebung

### Indikator erscheint nicht in der Liste

- Stellen Sie sicher, dass die Datei im richtigen Ordner liegt
- Kompilieren Sie erneut (F5)
- Überprüfen Sie Kompilierungsfehler im Output-Fenster
- Starten Sie NinjaTrader neu

### Keine Blasen werden angezeigt

- Überprüfen Sie, ob **Show Bid Bubbles** oder **Show Ask Bubbles** aktiviert ist
- Senken Sie den **Minimum Volume** Wert
- Stellen Sie sicher, dass Sie einen Chart mit Marktdaten verwenden
- Überprüfen Sie, ob der Datenfeed Bid/Ask-Informationen bereitstellt

### Blasen sind zu klein oder zu groß

- Passen Sie **Minimum Bubble Size** und **Maximum Bubble Size** an
- Ändern Sie den **Minimum Volume** Wert um die Skalierung anzupassen

### Tooltip wird nicht angezeigt

- Bewegen Sie die Maus langsam über die Blase
- Der Tooltip erscheint nur, wenn die Maus direkt über einer Blase ist
- Stellen Sie sicher, dass die Blasengröße nicht zu klein ist

## Performance-Tipps

1. **Minimum Volume erhöhen**: Reduziert die Anzahl der gezeichneten Blasen und verbessert die Performance

2. **Nur benötigte Blasen anzeigen**: Deaktivieren Sie entweder Bid oder Ask, wenn Sie nur eines davon analysieren möchten

3. **Opacity anpassen**: Niedrigere Opacity-Werte können die Rendering-Performance verbessern

4. **Blasengröße begrenzen**: Kleinere maximale Blasengrößen erfordern weniger Rechenleistung

## Kompatibilität

Der Indikator wurde getestet mit:
- NinjaTrader 8.0 und höher
- Standard Datenfeeds (Kinetick, CQG, Rithmic, etc.)
- Tick-, Minuten-, und Volumen-basierten Charts

## Technische Anforderungen

- **NinjaTrader Version**: 8.0 oder höher
- **.NET Framework**: 4.8 oder höher
- **Arbeitsspeicher**: Mindestens 4 GB RAM empfohlen
- **Datenfeed**: Muss Bid/Ask-Daten bereitstellen

## Weitere Informationen

Für weitere Fragen oder technischen Support, besuchen Sie bitte das GitHub-Repository oder erstellen Sie ein Issue.
