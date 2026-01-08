# PayPal & Email Setup

## PayPal Integration

### Client IDs
**Sandbox (Test):**
- Client ID: `Adr6O6q0QWdYPcLqGOPSQqEGY2gKEwRVex_VDYkCV82zI36wwQhR1LBQM0JDxOzGKFB9SqE3icKlvQxL`
- App Name: NinjaTrader 8 Indikator
- Created: 08/01/26

**Live (Production):**
- Switch in PayPal Dashboard from Sandbox to Live
- Get Live Client ID from the same app
- Update in `docs/index.html` Line 230

### Subscription Plan ID
- Monthly $5: `P-0UY85111HW8408537NFP7UAQ`
- Update in `docs/script.js` Line 50

---

## Required Environment Variables

Set these in Netlify Dashboard → Site Settings → Environment Variables:

### Email Configuration (Gmail Example)
```
SMTP_HOST=smtp.gmail.com
SMTP_USER=your-email@gmail.com
SMTP_PASS=your-app-password
```

### Download URL
```
DOWNLOAD_URL=https://your-site.netlify.app/VolumeBubbleIndicator.cs
```

## Gmail Setup
1. Enable 2-Factor Authentication
2. Generate App Password: https://myaccount.google.com/apppasswords
3. Use the App Password as SMTP_PASS

## Alternative Email Services
- **SendGrid**: More reliable for transactional emails
- **Mailgun**: Good for high volume
- **AWS SES**: Very cheap for production

## File Hosting
Put the indicator file in the `docs` folder:
- `/docs/VolumeBubbleIndicator.cs`
- URL will be: `https://your-site.netlify.app/VolumeBubbleIndicator.cs`
