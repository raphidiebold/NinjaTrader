const nodemailer = require('nodemailer');

exports.handler = async (event, context) => {
  // Only allow POST requests
  if (event.httpMethod !== 'POST') {
    return { statusCode: 405, body: 'Method Not Allowed' };
  }

  try {
    const data = JSON.parse(event.body);
    const { orderId, subscriptionId, email, name, type } = data;
    
    const transactionId = orderId || subscriptionId;

    // Create email transporter (configure with your email service)
    const transporter = nodemailer.createTransport({
      host: process.env.SMTP_HOST,
      port: 587,
      secure: false,
      auth: {
        user: process.env.SMTP_USER,
        pass: process.env.SMTP_PASS
      }
    });

    // Download link - ZIP file with compiled DLL from GitHub Release
    const downloadLink = process.env.DOWNLOAD_URL || 'https://github.com/raphidiebold/NinjaTrader/releases/download/Indikator/LargeTradesDetectorRD.zip';

    // Email content
    const mailOptions = {
      from: process.env.SMTP_USER,
      to: email,
      subject: 'Volume Bubble Indicator - Download & Installation',
      html: `
        <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
          <h2 style="color: #2563eb;">Thank you for your purchase!</h2>
          <p>Hi ${name || 'there'},</p>
          <p>Thank you for purchasing the <strong>Volume Bubble Indicator</strong> for NinjaTrader 8.</p>
          ${type === 'subscription' 
            ? '<p style="color: #10b981;">✓ Your monthly subscription is now active.</p>' 
            : '<p style="color: #10b981;">✓ Your lifetime license is now activated.</p>'
          }
          
          <div style="background: #f3f4f6; padding: 20px; border-radius: 8px; margin: 20px 0;">
            <h3 style="margin-top: 0;">📦 Download Your Indicator</h3>
            <p><a href="${downloadLink}" style="background-color: #2563eb; color: white; padding: 14px 28px; text-decoration: none; border-radius: 8px; display: inline-block; font-weight: bold;">Download LargeTradesDetectorRD.zip</a></p>
            <p style="font-size: 14px; color: #6b7280; margin-top: 10px;">The ZIP file contains the compiled DLL ready for installation.</p>
          </div>
          
          <h3>📝 Installation Instructions</h3>
          <ol style="line-height: 1.8;">
            <li><strong>Download</strong> the ZIP file using the button above</li>
            <li><strong>Extract</strong> the ZIP file - you will get the <code>.cs</code> and <code>.dll</code> files</li>
            <li><strong>Close</strong> NinjaTrader 8 completely (if open)</li>
            <li>Open <strong>File Explorer</strong></li>
            <li>Navigate to: <strong>NinjaTrader 8 folder</strong> → <strong>bin</strong> → <strong>Custom</strong></li>
            <li><strong>Copy</strong> <strong>both files</strong> (.cs and .dll) into the <code>Custom</code> folder</li>
            <li><strong>Restart</strong> NinjaTrader 8</li>
            <li>The indicator is now available and can be added to your chart</li>
          </ol>
          
          <div style="background: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; margin: 20px 0;">
            <p style="margin: 0;"><strong>⚠️ Important:</strong> Make sure NinjaTrader 8 is completely closed before copying the files.</p>
          </div>
          
          <h3>📁 Path Example</h3>
          <p style="background: #f3f4f6; padding: 10px; border-radius: 4px; font-family: monospace; font-size: 14px;">
            C:\\Users\\YourUsername\\Documents\\NinjaTrader 8\\bin\\Custom\\<br>
            → LargeTradesDetectorRD.cs<br>
            → LargeTradesDetectorRD.dll
          </p>
          
          <div style="border-top: 1px solid #e5e7eb; margin-top: 30px; padding-top: 20px;">
            <p style="font-size: 14px; color: #6b7280;">
              ${type === 'subscription' ? 'Subscription' : 'Order'} ID: <strong>${transactionId}</strong><br>
              ${type === 'subscription' ? 'Your subscription will renew automatically each month.' : 'This is a one-time payment with lifetime access.'}
            </p>
          </div>
          
          <p>If you have any questions or need support, please reply to this email.</p>
          
          <p style="margin-top: 30px;">Best regards,<br><strong>Volume Bubble Indicator Team</strong></p>
        </div>
      `
    };

    // Send email
    await transporter.sendMail(mailOptions);

    return {
      statusCode: 200,
      body: JSON.stringify({ success: true, message: 'Email sent successfully' })
    };

  } catch (error) {
    console.error('Error:', error);
    return {
      statusCode: 500,
      body: JSON.stringify({ error: 'Failed to send email' })
    };
  }
};
