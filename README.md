# Volume Bubble Indicator für NinjaTrader 8

Ein professioneller Volume-Analyse-Indikator mit Absorption-Detection für NinjaTrader 8.

## 🚀 Live Demo

- **Webseite**: [Deine Netlify URL]
- **PayPal Integration**: ✅ Sandbox-Modus aktiv

## 📦 Projekt-Struktur

```
.
├── docs/                          # Webseite (öffentlich)
│   ├── index.html                # Hauptseite mit PayPal Buttons
│   ├── script.js                 # PayPal Integration & Logik
│   ├── styles.css                # Design
│   └── VolumeBubbleIndicator.cs  # Download-Datei
│
├── netlify/
│   └── functions/
│       └── send-download.js      # Email-Versand nach Kauf
│
├── netlify.toml                  # Netlify Konfiguration
├── package.json                  # Node Dependencies
├── PAYPAL_SETUP.md              # PayPal Setup Anleitung
└── EMAIL_SETUP.md               # Email Konfiguration

```

## 🎯 Features

- ✅ PayPal Einmal-Zahlung ($150)
- ✅ PayPal Abo ($5/Monat)
- ✅ Automatischer Email-Versand mit Download-Link
- ✅ Netlify Functions für Backend
- ✅ Responsive Design
- ✅ Screenshot Lightbox

## 📋 Setup

### 1. Repository klonen & Dependencies installieren

```bash
npm install
```

### 2. PayPal konfigurieren

Siehe [PAYPAL_SETUP.md](PAYPAL_SETUP.md) für detaillierte Anleitung.

**Schnellstart:**
- Client ID ist bereits integriert (Sandbox)
- Für Live: Client ID in `docs/index.html` Zeile 230 ersetzen

### 3. Email-Versand einrichten

**Umgebungsvariablen in Netlify setzen:**
```
SMTP_HOST=your-smtp-host
SMTP_USER=your-email@example.com
SMTP_PASS=your-app-password
DOWNLOAD_URL=https://your-site.netlify.app/VolumeBubbleIndicator.cs
```

### 4. Deployment zu Netlify

```bash
# Mit Netlify CLI
npm install -g netlify-cli
netlify login
netlify init
netlify deploy --prod
```

Oder verbinde das GitHub Repository direkt in der Netlify UI.

## 🧪 Testen

### Sandbox Test

1. PayPal Sandbox Account erstellen: https://developer.paypal.com/dashboard/accounts
2. Webseite öffnen und auf PayPal Button klicken
3. Mit Sandbox Account einloggen
4. Zahlung durchführen

### Live-Modus aktivieren

Siehe [PAYPAL_SETUP.md](PAYPAL_SETUP.md) - Abschnitt "Go Live"

## 📧 Email Template

Nach erfolgreicher Zahlung erhalten Kunden eine Email mit:
- Download-Link zum Indicator
- Installations-Anleitung
- Order ID für Support

Template anpassen: `netlify/functions/send-download.js`

## 🔧 Anpassungen

### Preise ändern

**Einmal-Zahlung:** `docs/script.js` Zeile 9
```javascript
amount: {
    value: '150.00'  // Hier ändern
}
```

**Abo:** Neuen Plan in PayPal Dashboard erstellen und Plan ID in `docs/script.js` Zeile 50 eintragen

### Design anpassen

- **Farben**: `docs/styles.css` - CSS Variables oben
- **Text**: `docs/index.html`
- **Buttons**: PayPal Button Style in `docs/script.js`

## 🐛 Troubleshooting

### PayPal Button erscheint nicht
- Browser Console öffnen (F12)
- Client ID überprüfen
- Internet-Verbindung prüfen

### Email kommt nicht an
- Netlify Functions Log checken
- SMTP Credentials überprüfen  
- Spam-Ordner prüfen

### "Subscription creation error"
- Plan ID überprüfen
- Plan muss ACTIVE sein
- Im gleichen PayPal Account

## 📚 Dokumentation

- [PayPal JavaScript SDK](https://developer.paypal.com/docs/checkout/)
- [Netlify Functions](https://docs.netlify.com/functions/overview/)
- [Nodemailer](https://nodemailer.com/)

## 🔗 Links

- PayPal Dashboard: https://developer.paypal.com/dashboard
- Netlify Dashboard: https://app.netlify.com
- NinjaTrader: https://ninjatrader.com

## 📝 Lizenz

Proprietär - Alle Rechte vorbehalten

