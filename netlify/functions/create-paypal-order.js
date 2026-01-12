// Create PayPal Order for SDK v6

const fetch = require('node-fetch');

exports.handler = async (event, context) => {
  if (event.httpMethod !== 'POST') {
    return { statusCode: 405, body: 'Method Not Allowed' };
  }

  const clientId = process.env.PAYPAL_CLIENT_ID;
  const clientSecret = process.env.PAYPAL_CLIENT_SECRET;
  const environment = process.env.PAYPAL_ENV;
  
  const baseURL = environment === 'sandbox' 
    ? 'https://api-m.sandbox.paypal.com'
    : 'https://api-m.paypal.com';

  try {
    const { product, amount, currency } = JSON.parse(event.body);

    // Get access token
    const auth = Buffer.from(`${clientId}:${clientSecret}`).toString('base64');
    
    const tokenResponse = await fetch(`${baseURL}/v1/oauth2/token`, {
      method: 'POST',
      headers: {
        'Authorization': `Basic ${auth}`,
        'Content-Type': 'application/x-www-form-urlencoded',
      },
      body: 'grant_type=client_credentials'
    });

    const tokenData = await tokenResponse.json();
    const accessToken = tokenData.access_token;

    // Create order with return URLs
    const siteUrl = event.headers.origin || event.headers.referer?.split('/').slice(0, 3).join('/') || 'https://rdindicators.netlify.app';
    
    const orderResponse = await fetch(`${baseURL}/v2/checkout/orders`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${accessToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        intent: 'CAPTURE',
        purchase_units: [{
          description: product,
          amount: {
            currency_code: currency || 'USD',
            value: amount
          }
        }],
        application_context: {
          brand_name: 'Volume Bubble Indicator',
          locale: 'en-US',
          landing_page: 'BILLING',
          shipping_preference: 'NO_SHIPPING',
          user_action: 'PAY_NOW',
          return_url: `${siteUrl}/payment-success.html`,
          cancel_url: `${siteUrl}/index.html#purchase`
        }
      })
    });

    const orderData = await orderResponse.json();
    
    // Find approval URL
    const approvalUrl = orderData.links?.find(link => link.rel === 'approve')?.href;

    return {
      statusCode: 200,
      headers: {
        'Content-Type': 'application/json',
        'Access-Control-Allow-Origin': '*',
      },
      body: JSON.stringify({
        orderId: orderData.id,
        approvalUrl: approvalUrl
      })
    };

  } catch (error) {
    console.error('Error creating order:', error);
    return {
      statusCode: 500,
      body: JSON.stringify({ error: 'Failed to create order' })
    };
  }
};
