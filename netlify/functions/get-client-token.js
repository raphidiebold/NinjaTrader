// Get PayPal Client Token for SDK v6

const fetch = require('node-fetch');

exports.handler = async (event, context) => {
  // Always return JSON with proper headers
  const headers = {
    'Content-Type': 'application/json',
    'Access-Control-Allow-Origin': '*',
  };

  if (event.httpMethod !== 'POST' && event.httpMethod !== 'GET') {
    return { 
      statusCode: 405, 
      headers,
      body: JSON.stringify({ error: 'Method Not Allowed' })
    };
  }

  const clientId = process.env.PAYPAL_CLIENT_ID;
  const clientSecret = process.env.PAYPAL_CLIENT_SECRET;
  const environment = process.env.PAYPAL_ENV;
  
  // Debug: Log what we have
  console.log('Environment check:', {
    hasClientId: !!clientId,
    hasClientSecret: !!clientSecret,
    hasEnvironment: !!environment,
    clientIdLength: clientId?.length,
    environment: environment
  });
  
  // Check if environment variables are set
  if (!clientId || !clientSecret || !environment) {
    return {
      statusCode: 500,
      headers,
      body: JSON.stringify({ 
        error: 'Missing PayPal credentials',
        details: {
          hasClientId: !!clientId,
          hasClientSecret: !!clientSecret,
          hasEnvironment: !!environment,
          allEnvVars: Object.keys(process.env).filter(k => k.includes('PAYPAL') || k.includes('SMTP'))
        }
      })
    };
  }
  
  const baseURL = environment === 'sandbox' 
    ? 'https://api-m.sandbox.paypal.com'
    : 'https://api-m.paypal.com';

  try {
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

    // Get client token
    const clientTokenResponse = await fetch(`${baseURL}/v1/identity/generate-token`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${accessToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({})
    });

    const clientTokenData = await clientTokenResponse.json();

    return {
      statusCode: 200,
      headers,
      body: JSON.stringify({
        clientToken: clientTokenData.client_token
      })
    };

  } catch (error) {
    console.error('Error getting client token:', error);
    return {
      statusCode: 500,
      headers,
      body: JSON.stringify({ 
        error: 'Failed to get client token',
        message: error.message 
      })
    };
  }
};
