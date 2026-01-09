// Get PayPal Subscription Details
const fetch = require('node-fetch');

exports.handler = async (event, context) => {
  // Only allow GET
  if (event.httpMethod !== 'GET') {
    return {
      statusCode: 405,
      body: JSON.stringify({ error: 'Method not allowed' })
    };
  }

  const clientId = process.env.PAYPAL_CLIENT_ID;
  const clientSecret = process.env.PAYPAL_CLIENT_SECRET;
  const environment = process.env.PAYPAL_ENV;
  
  const baseURL = environment === 'sandbox'
    ? 'https://api-m.sandbox.paypal.com'
    : 'https://api-m.paypal.com';

  const subscriptionId = event.queryStringParameters?.subscriptionId;

  if (!subscriptionId) {
    return {
      statusCode: 400,
      body: JSON.stringify({ error: 'Subscription ID is required' })
    };
  }

  if (!clientId || !clientSecret) {
    return {
      statusCode: 500,
      body: JSON.stringify({ error: 'PayPal credentials not configured' })
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

    // Get subscription details
    const subscriptionResponse = await fetch(`${baseURL}/v1/billing/subscriptions/${subscriptionId}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${tokenData.access_token}`,
        'Content-Type': 'application/json'
      }
    });

    const subscriptionData = await subscriptionResponse.json();

    if (!subscriptionResponse.ok) {
      console.error('Subscription fetch error:', subscriptionData);
      return {
        statusCode: 500,
        body: JSON.stringify({ error: 'Failed to fetch subscription', details: subscriptionData })
      };
    }

    return {
      statusCode: 200,
      body: JSON.stringify(subscriptionData)
    };

  } catch (error) {
    console.error('Error fetching subscription:', error);
    return {
      statusCode: 500,
      body: JSON.stringify({ error: 'Internal server error', details: error.message })
    };
  }
};
