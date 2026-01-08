// Simple test function to check environment variables
exports.handler = async (event, context) => {
  return {
    statusCode: 200,
    headers: {
      'Content-Type': 'application/json',
      'Access-Control-Allow-Origin': '*',
    },
    body: JSON.stringify({
      test: 'success',
      env: {
        PAYPAL_CLIENT_ID: process.env.PAYPAL_CLIENT_ID ? 'SET' : 'MISSING',
        PAYPAL_CLIENT_SECRET: process.env.PAYPAL_CLIENT_SECRET ? 'SET' : 'MISSING',
        PAYPAL_ENV: process.env.PAYPAL_ENV || 'MISSING',
        clientIdLength: process.env.PAYPAL_CLIENT_ID?.length
      }
    })
  };
};
