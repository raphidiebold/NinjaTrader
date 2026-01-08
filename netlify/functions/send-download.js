const nodemailer = require('nodemailer');

exports.handler = async (event, context) => {
  // Only allow POST requests
  if (event.httpMethod !== 'POST') {
    return { statusCode: 405, body: 'Method Not Allowed' };
  }

  try {
    const data = JSON.parse(event.body);
    const { orderId, email, name, type } = data;

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

    // Download link (you'll need to host the file somewhere accessible)
    const downloadLink = process.env.DOWNLOAD_URL || 'https://your-domain.com/downloads/VolumeBubbleIndicator.cs';

    // Email content
    const mailOptions = {
      from: process.env.SMTP_USER,
      to: email,
      subject: 'Volume Bubble Indicator - Download Link',
      html: `
        <h2>Thank you for your purchase!</h2>
        <p>Hi ${name},</p>
        <p>Thank you for purchasing the Volume Bubble Indicator for NinjaTrader 8.</p>
        ${type === 'subscription' 
          ? '<p>Your monthly subscription is now active.</p>' 
          : '<p>Your lifetime license is now activated.</p>'
        }
        
        <h3>Download Your Indicator</h3>
        <p><a href="${downloadLink}" style="background-color: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block;">Download VolumeBubbleIndicator.cs</a></p>
        
        <h3>Installation Instructions</h3>
        <ol>
          <li>Download the file using the link above</li>
          <li>Open NinjaTrader 8</li>
          <li>Go to Tools → Import → NinjaScript Add-On</li>
          <li>Select the downloaded file</li>
          <li>Press F5 to compile</li>
          <li>Add the indicator to your chart</li>
        </ol>
        
        <p>Order ID: ${orderId}</p>
        
        <p>If you have any questions, please reply to this email.</p>
        
        <p>Best regards,<br>Your Team</p>
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
