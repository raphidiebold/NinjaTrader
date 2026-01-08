# PayPal Integration - Setup Anleitung

## 🎯 Aktueller Status

✅ **Client ID integriert**: `Adr6O6q0QWdYPcLqGOPSQ...`  
✅ **Subscription Plan**: `P-0UY85111HW8408537NFP7UAQ`  
⚠️ **Modus**: Sandbox (Test-Modus)

---

## 📋 Nächste Schritte

### 1. Test-Zahlung durchführen

1. **Öffne deine Webseite**: https://your-site.netlify.app
2. **Klicke auf einen PayPal Button**
3. **Nutze einen PayPal Sandbox Account**:
   - Gehe zu: https://developer.paypal.com/dashboard/accounts
   - Erstelle einen Test-Buyer Account
   - Oder nutze existierende Sandbox-Credentials

### 2. Sandbox Test Accounts erstellen

```
PayPal Developer Dashboard → Sandbox → Accounts → Create Account

Account Type: Personal (Buyer)
Email: test-buyer@yourdomain.com
Password: [selbst wählen]
Balance: $1000 (Testgeld)
```

### 3. Live-Modus aktivieren (wenn Tests erfolgreich)

1. **PayPal Dashboard öffnen**: https://developer.paypal.com
2. **Oben rechts**: "Sandbox" → "Live" umschalten
3. **Live Client ID kopieren** (gleiche App: "NinjaTrader 8 Indikator")
4. **In `docs/index.html` ersetzen**:
   ```javascript
   // Zeile 230: Alte Client ID raus, Live Client ID rein
   client-id=DEINE_LIVE_CLIENT_ID
   ```
5. **`&disable-funding` entfernen** (um Kreditkarten zu erlauben)

---

## 🔧 PayPal Button Konfiguration

### Einmalige Zahlung ($150)
- **Typ**: Order
- **Betrag**: $150.00 USD
- **Container**: `#paypal-button-container-onetime`

### Abo ($5/Monat)
- **Typ**: Subscription
- **Plan ID**: `P-0UY85111HW8408537NFP7UAQ`
- **Container**: `#paypal-button-container-subscription`

---

## 📧 Email-Versand einrichten

### Option 1: Gmail (Einfach, für Tests)

1. **Gmail App-Passwort erstellen**:
   - https://myaccount.google.com/apppasswords
   - "Andere App" auswählen → Name: "NinjaTrader"
   - Passwort generieren und kopieren

2. **Netlify Umgebungsvariablen**:
   ```
   SMTP_HOST=smtp.gmail.com
   SMTP_USER=deine-email@gmail.com
   SMTP_PASS=[generiertes App-Passwort]
   DOWNLOAD_URL=https://your-site.netlify.app/VolumeBubbleIndicator.cs
   ```

### Option 2: SendGrid (Empfohlen für Produktion)

1. **SendGrid Account**: https://signup.sendgrid.com
2. **API Key erstellen**: Settings → API Keys → Create API Key
3. **Netlify Umgebungsvariablen**:
   ```
   SENDGRID_API_KEY=[dein API Key]
   SENDGRID_FROM_EMAIL=noreply@yourdomain.com
   DOWNLOAD_URL=https://your-site.netlify.app/VolumeBubbleIndicator.cs
   ```

---

## 🧪 Test-Checkliste

- [ ] Sandbox Test-Buyer Account erstellt
- [ ] Einmalige Zahlung getestet ($150)
- [ ] Abo-Zahlung getestet ($5/Monat)
- [ ] Email-Versand funktioniert
- [ ] Download-Link in Email funktioniert
- [ ] Indicator-Datei ist erreichbar

---

## 🚀 Go Live Checkliste

- [ ] Alle Sandbox-Tests erfolgreich
- [ ] Live Client ID von PayPal geholt
- [ ] Live Client ID in `index.html` eingefügt
- [ ] `&disable-funding` entfernt (optional)
- [ ] Echte Email-Adresse in `send-download.js` konfiguriert
- [ ] Netlify Environment Variables gesetzt
- [ ] Produktion-Test mit kleinem Betrag
- [ ] PayPal Webhook URLs konfiguriert (optional, für Auto-Cancellations)

---

## 🔍 Debugging

### PayPal Button wird nicht angezeigt
- Browser Console öffnen (F12)
- Nach Fehlern suchen
- Client ID überprüfen
- Internet-Verbindung checken

### "Subscription creation error"
- Plan ID überprüfen: `P-0UY85111HW8408537NFP7UAQ`
- Plan muss im gleichen PayPal Account existieren
- Plan muss ACTIVE Status haben

### Email kommt nicht an
- Netlify Functions Log checken
- SMTP Credentials überprüfen
- Spam-Ordner checken
- SendGrid verwenden (zuverlässiger)

---

## 📞 Support

Bei Fragen:
1. Browser Console Log checken (F12)
2. Netlify Functions Log checken
3. PayPal Developer Dashboard → Sandbox → API Calls

---

## 🔗 Wichtige Links

- PayPal Developer Dashboard: https://developer.paypal.com/dashboard
- Sandbox Test Accounts: https://developer.paypal.com/dashboard/accounts
- Subscription Plans: https://developer.paypal.com/dashboard/applications/sandbox
- Netlify Dashboard: https://app.netlify.com
