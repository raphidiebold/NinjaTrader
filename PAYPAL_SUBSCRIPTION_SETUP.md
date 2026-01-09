# PayPal Subscription Setup - Konfigurationsanleitung

## Umgebungsvariablen

Du musst folgende Umgebungsvariable in deinen Netlify-Einstellungen hinzufügen:

### PAYPAL_PLAN_ID

Diese Variable enthält die Plan-ID deines monatlichen Subscription-Plans von PayPal Sandbox.

#### So findest du die Plan-ID:

1. Gehe zu https://developer.paypal.com/
2. Melde dich an und wähle "Dashboard"
3. Navigiere zu "Apps & Credentials"
4. Wähle "Sandbox"
5. Gehe zu "Subscriptions" → "Plans"
6. Wähle deinen erstellten Plan aus
7. Kopiere die "Plan ID" (beginnt normalerweise mit `P-`)

#### Hinzufügen in Netlify:

1. Gehe zu deinem Netlify Dashboard
2. Wähle deine Site aus
3. Gehe zu "Site settings" → "Environment variables"
4. Klicke "Add a variable"
5. Name: `PAYPAL_PLAN_ID`
6. Value: `P-0M377129CN458145HNFQEDLA`
7. Speichern und neu deployen

## Bereits konfigurierte Variablen

Diese sollten bereits gesetzt sein:
- `PAYPAL_CLIENT_ID` - Deine PayPal Client ID
- `PAYPAL_CLIENT_SECRET` - Dein PayPal Client Secret
- `PAYPAL_ENV` - Sollte `sandbox` sein für Tests, später `production`

## Testing

Nach dem Deployment kannst du die Subscription testen:

1. Öffne deine Webseite
2. Scrolle zur "Monthly Subscription" Preisbox
3. Der PayPal Subscribe Button sollte erscheinen
4. Klicke auf den Button und folge dem PayPal Checkout
5. Verwende PayPal Sandbox Test-Accounts zum Testen

## PayPal Sandbox Test Accounts

Erstelle Test-Accounts unter:
https://developer.paypal.com/dashboard/accounts

Du brauchst:
- Einen "Personal" Account (Käufer)
- Einen "Business" Account (Verkäufer - bereits erstellt)

## Wichtige Hinweise

- Die Subscription nutzt PayPal SDK v5 mit `vault=true` und `intent=subscription`
- Der Plan muss in PayPal aktiv sein
- Stelle sicher, dass der Plan $5/Monat kostet (oder passe den Preis in index.html an)
- Nach erfolgreicher Subscription erhält der Kunde automatisch den Download-Link per Email

## Dateien die erstellt/geändert wurden

1. **netlify/functions/create-paypal-subscription.js** - Erstellt neue Subscription
2. **netlify/functions/get-subscription-details.js** - Holt Subscription-Details
3. **docs/script.js** - Hinzugefügt: Subscription Button Integration
4. **docs/index.html** - Ersetzt "Coming Soon" mit PayPal Button

## Nächste Schritte

1. ✅ Plan in PayPal Sandbox erstellt
2. ⏳ `PAYPAL_PLAN_ID` zu Netlify Umgebungsvariablen hinzufügen
3. ⏳ Code committen und pushen
4. ⏳ Auf Netlify deployen
5. ⏳ Subscription mit Sandbox Account testen
6. ⏳ Bei Erfolg: auf Production umstellen
