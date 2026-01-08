# Environment Variables Setup

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
