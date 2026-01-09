// Create PayPal Subscription for SDK v5
const fetch = require('node-fetch');

exports.handler = async (event, context) => {
  // Only allow POST
  if (event.httpMethod !== 'POST') {
    return {
      statusCode: 405,
      body: JSON.stringify({ error: 'Method not allowed' })
    };
  }

  const clientId = process.env.PAYPAL_CLIENT_ID;
  const clientSecret = process.env.PAYPAL_CLIENT_SECRET;
  const environment = process.env.PAYPAL_ENV;
  const planId = process.env.PAYPAL_PLAN_ID; // Your subscription plan ID
  
  const baseURL = environment === 'sandbox'
    ? 'https://api-m.sandbox.paypal.com'
    : 'https://api-m.paypal.com';

  if (!clientId || !clientSecret) {
    return {
      statusCode: 500,
      body: JSON.stringify({ error: 'PayPal credentials not configured' })
    };
  }

  if (!planId) {
    return {
      statusCode: 500,
      body: JSON.stringify({ error: 'PayPal Plan ID not configured' })
    };
  }

  try {
    // Get access token
    const auth = Buffer.from(`${clientId}:${clientSecret}`).toString('base64');
    const tokenResponse = await fetch(`${baseURL}/v1/oauth2/token`, {
      method: 'POST',
      headers: {
        'Authorization': `Basic ${auth}`,
        'Content-Type': 'application/x-www-form-urlencoded'
      },
      body: 'grant_type=client_credentials'
    });

    const tokenData = await tokenResponse.json();

    if (!tokenResponse.ok) {
      console.error('Token error:', tokenData);
      return {
        statusCode: 500,
        body: JSON.stringify({ error: 'Failed to get PayPal token', details: tokenData })
      };
    }

    // Create subscription
    const subscriptionResponse = await fetch(`${baseURL}/v1/billing/subscriptions`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${tokenData.access_token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        plan_id: planId,
        application_context: {
          brand_name: 'Volume Bubble Indicator',
          locale: 'en-US',
          shipping_preference: 'NO_SHIPPING',
          user_action: 'SUBSCRIBE_NOW',
          return_url: `${event.headers.origin || 'https://your-domain.netlify.app'}/#purchase`,
          cancel_url: `${event.headers.origin || 'https://your-domain.netlify.app'}/#purchase`
        }
      })
    });

    const subscriptionData = await subscriptionResponse.json();

    if (!subscriptionResponse.ok) {
      console.error('Subscription creation error:', subscriptionData);
      return {
        statusCode: 500,
        body: JSON.stringify({ error: 'Failed to create subscription', details: subscriptionData })
      };
    }

    return {
      statusCode: 200,
      body: JSON.stringify({
        subscriptionId: subscriptionData.id,
        status: subscriptionData.status,
        links: subscriptionData.links
      })
    };

  } catch (error) {
    console.error('Error creating subscription:', error);
    return {
      statusCode: 500,
      body: JSON.stringify({ error: 'Internal server error', details: error.message })
    };
  }
};
