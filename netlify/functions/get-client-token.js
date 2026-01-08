const fetch = require('node-fetch');

exports.handler = async (event, context) => {
  const headers = {
    'Content-Type': 'application/json',
    'Access-Control-Allow-Origin': '*',
  };

  try {
    const clientId = process.env.PAYPAL_CLIENT_ID;
    const clientSecret = process.env.PAYPAL_CLIENT_SECRET;
    const environment = process.env.PAYPAL_ENV || 'sandbox';
    
    if (!clientId || !clientSecret) {
      return {
        statusCode: 500,
        headers,
        body: JSON.stringify({ error: 'Missing credentials' })
      };
    }

    const baseURL = environment === 'sandbox' 
      ? 'https://api-m.sandbox.paypal.com'
      : 'https://api-m.paypal.com';

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

    if (!tokenResponse.ok) {
      const errorText = await tokenResponse.text();
      return {
        statusCode: 500,
        headers,
        body: JSON.stringify({ 
          error: 'Failed to get access token',
          status: tokenResponse.status
        })
      };
    }

    const tokenData = await tokenResponse.json();
    const accessToken = tokenData.access_token;

    // Get client token
    const clientTokenResponse = await fetch(`${baseURL}/v1/identity/generate-token`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${accessToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({})
    });

    if (!clientTokenResponse.ok) {
      const errorText = await clientTokenResponse.text();
      return {
        statusCode: 500,
        headers,
        body: JSON.stringify({ 
          error: 'Failed to get client token',
          status: clientTokenResponse.status
        })
      };
    }

    const clientTokenData = await clientTokenResponse.json();

    return {
      statusCode: 200,
      headers,
      body: JSON.stringify({
        clientToken: clientTokenData.client_token
      })
    };

  } catch (error) {
    return {
      statusCode: 500,
      headers,
      body: JSON.stringify({ 
        error: 'Server error',
        message: error.message
      })
    };
  }
};
