// PayPal Integration - One-Time Payment & Subscription

// Wait for PayPal SDK to load
if (typeof paypal !== 'undefined') {
  initPayPal();
} else {
  window.addEventListener('load', initPayPal);
}

function initPayPal() {
  if (typeof paypal === 'undefined') {
    showPayPalError('PayPal SDK not loaded');
    return;
  }

  // Render PayPal button for one-time payment
  paypal.Buttons({
    createOrder: async function(data, actions) {
      try {
        const response = await fetch('/.netlify/functions/create-paypal-order', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            amount: '150.00',
            currency: 'USD'
          })
        });
        
        const orderData = await response.json();
        return orderData.orderId;
      } catch (error) {
        console.error('Error creating order:', error);
        showPayPalError('Failed to create order');
      }
    },
    
    onApprove: async function(data, actions) {
      try {
        const response = await fetch('/.netlify/functions/capture-paypal-order', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            orderId: data.orderID
          })
        });
        
        const captureData = await response.json();
        
        if (captureData.status === 'COMPLETED') {
          // Send download link via email
          await fetch('/.netlify/functions/send-download', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
              email: captureData.payer.email_address,
              orderId: data.orderID,
              type: 'onetime'
            })
          });
          
          alert('Payment successful! Check your email for the download link.');
          window.location.href = '#installation';
        } else {
          showPayPalError('Payment verification failed');
        }
      } catch (error) {
        console.error('Error capturing order:', error);
        showPayPalError('Payment processing failed');
      }
    },
    
    onError: function(err) {
      console.error('PayPal error:', err);
      showPayPalError('Payment system error');
    },
    
    style: {
      layout: 'vertical',
      color: 'gold',
      shape: 'rect',
      label: 'paypal'
    }
  }).render('#paypal-button-onetime');
  
  // Render PayPal button for monthly subscription
  paypal.Buttons({
    createSubscription: async function(data, actions) {
      try {
        const response = await fetch('/.netlify/functions/create-paypal-subscription', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' }
        });
        
        const subscriptionData = await response.json();
        
        if (subscriptionData.error) {
          console.error('Subscription error:', subscriptionData);
          showPayPalError('Failed to create subscription', 'subscription');
          throw new Error(subscriptionData.error);
        }
        
        return subscriptionData.subscriptionId;
      } catch (error) {
        console.error('Error creating subscription:', error);
        showPayPalError('Failed to create subscription', 'subscription');
        throw error;
      }
    },
    
    onApprove: async function(data, actions) {
      try {
        // Get subscription details
        const response = await fetch(`/.netlify/functions/get-subscription-details?subscriptionId=${data.subscriptionID}`);
        const subscriptionDetails = await response.json();
        
        if (subscriptionDetails.status === 'ACTIVE') {
          // Send download link via email for subscription
          await fetch('/.netlify/functions/send-download', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
              email: subscriptionDetails.subscriber.email_address,
              subscriptionId: data.subscriptionID,
              type: 'subscription'
            })
          });
          
          alert('Subscription activated! Check your email for the download link.');
          window.location.href = '#installation';
        } else {
          showPayPalError('Subscription verification failed', 'subscription');
        }
      } catch (error) {
        console.error('Error approving subscription:', error);
        showPayPalError('Subscription processing failed', 'subscription');
      }
    },
    
    onError: function(err) {
      console.error('PayPal subscription error:', err);
      showPayPalError('Subscription system error', 'subscription');
    },
    
    style: {
      layout: 'vertical',
      color: 'gold',
      shape: 'rect',
      label: 'subscribe'
    }
  }).render('#paypal-button-subscription');
  
  // Hide error messages when PayPal loads successfully
  hidePayPalError();
}

function showPayPalError(message, type = 'onetime') {
  const containerId = type === 'subscription' 
    ? 'paypal-button-subscription' 
    : 'paypal-button-onetime';
  const container = document.getElementById(containerId);
  if (container) {
    container.innerHTML = `
      <div style="padding: 20px; background: #fff3cd; border: 1px solid #ffc107; border-radius: 8px; color: #856404;">
        ⚠️ ${message}
      </div>
    `;
  }
}

function hidePayPalError() {
  // Errors are automatically replaced by PayPal buttons
}

// Lightbox functionality for screenshots
document.addEventListener('DOMContentLoaded', function() {
  const lightbox = document.getElementById('lightbox');
  const lightboxImg = document.getElementById('lightbox-img');
  const lightboxCaption = document.getElementById('lightbox-caption');
  const screenshots = document.querySelectorAll('.screenshot');

  screenshots.forEach(screenshot => {
    screenshot.addEventListener('click', function() {
      lightbox.classList.add('active');
      lightboxImg.src = this.src;
      lightboxCaption.textContent = this.alt;
    });
  });

  lightbox.addEventListener('click', function() {
    this.classList.remove('active');
  });

  // Close lightbox with Escape key
  document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape') {
      lightbox.classList.remove('active');
    }
  });
});
