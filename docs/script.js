// PayPal SDK v6 Integration
// One-time payment for Volume Bubble Indicator

async function onPayPalWebSdkLoaded() {
  try {
    console.log('PayPal SDK v6 loaded');
    
    // Get client token from server
    const clientToken = await getClientToken();
    
    if (!clientToken) {
      console.error('No client token available');
      showPayPalError('Payment system unavailable. Please try again later.');
      return;
    }
    
    // Create PayPal SDK instance
    const sdkInstance = await window.paypal.createInstance({
      clientToken,
      components: ["paypal-payments"],
      pageType: "checkout",
    });

    // Check eligibility for PayPal payments
    const paymentMethods = await sdkInstance.findEligibleMethods({
      currencyCode: "USD",
    });

    // Set up PayPal button if eligible
    if (paymentMethods.isEligible("paypal")) {
      console.log('PayPal payment method is eligible');
      setUpPayPalButton(sdkInstance);
    } else {
      console.error('PayPal not eligible');
      showPayPalError('PayPal payments not available in your region.');
    }
    
  } catch (error) {
    console.error("SDK initialization error:", error);
    showPayPalError('Payment system error. Please refresh and try again.');
  }
}

// Get client token
async function getClientToken() {
  try {
    const response = await fetch('/.netlify/functions/get-client-token');
    const data = await response.json();
    return data.clientToken;
  } catch (error) {
    console.error('Client token error:', error);
    return null;
  }
}

// Payment session options
const paymentSessionOptions = {
  async onApprove(data) {
    console.log("Payment approved:", data);
    
    try {
      const orderData = await captureOrder(data.orderId);
      console.log("Payment captured successfully:", orderData);
      showSuccessMessage(orderData);
      sendOrderToServer(orderData);
    } catch (error) {
      console.error("Payment capture failed:", error);
      alert('Payment processing failed. Please contact support with Order ID: ' + data.orderId);
    }
  },
  
  onCancel(data) {
    console.log("Payment cancelled:", data);
  },
  
  onError(error) {
    console.error("Payment error:", error);
    alert('Payment error occurred. Please try again or contact support.');
  },
};

// Set up PayPal button
async function setUpPayPalButton(sdkInstance) {
  const paypalPaymentSession = sdkInstance.createPayPalOneTimePaymentSession(
    paymentSessionOptions
  );

  const paypalButton = document.querySelector("paypal-button");
  paypalButton.removeAttribute("hidden");

  paypalButton.addEventListener("click", async () => {
    try {
      await paypalPaymentSession.start(
        { presentationMode: "auto" },
        createOrder()
      );
    } catch (error) {
      console.error("PayPal payment start error:", error);
      if (!error.isRecoverable) {
        alert('Unable to start payment. Please try again.');
      }
    }
  });
}

// Create order
async function createOrder() {
  try {
    const response = await fetch("/.netlify/functions/create-paypal-order", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        product: "Volume Bubble Indicator",
        amount: "150.00",
        currency: "USD"
      }),
    });
    
    const data = await response.json();
    if (!data.orderId) throw new Error('No order ID received');
    
    console.log('Order created:', data.orderId);
    return { orderId: data.orderId };
  } catch (error) {
    console.error('Create order error:', error);
    throw error;
  }
}

// Capture order
async function captureOrder(orderId) {
  try {
    const response = await fetch(`/.netlify/functions/capture-paypal-order`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ orderId }),
    });
    return await response.json();
  } catch (error) {
    console.error('Capture order error:', error);
    throw error;
  }
}

// Show success
function showSuccessMessage(orderData) {
  const purchaseSection = document.querySelector('#purchase .container');
  purchaseSection.innerHTML = `
    <h2>Thank You!</h2>
    <div class="success-message show">
      <h3>✓ Payment Successful!</h3>
      <p>Thank you for your purchase!</p>
      <p>Order ID: ${orderData.id}</p>
      <p>You will receive an email with the download link within 5 minutes.</p>
      <p>Please check your spam folder if you don't see it.</p>
    </div>
  `;
}

// Send order for email
function sendOrderToServer(orderData) {
  fetch('/.netlify/functions/send-download', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      orderId: orderData.id,
      email: orderData.payer?.email_address || 'customer@email.com',
      name: orderData.payer?.name?.given_name || 'Customer',
      type: 'lifetime'
    })
  })
  .then(() => console.log('Email sent'))
  .catch(err => console.error('Email error:', err));
}

// Show error
function showPayPalError(message) {
  const container = document.querySelector('#paypal-button-onetime');
  if (container) {
    container.innerHTML = `
      <div style="padding: 20px; background: #fff3cd; border: 1px solid #ffc107; border-radius: 8px;">
        <p style="margin: 0; color: #856404;">⚠️ ${message}</p>
      </div>
    `;
  }
}

// Smooth scroll
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
  anchor.addEventListener('click', function (e) {
    e.preventDefault();
    const target = document.querySelector(this.getAttribute('href'));
    if (target) target.scrollIntoView({ behavior: 'smooth' });
  });
});

// Lightbox
function openLightbox(imgElement) {
  const lightbox = document.getElementById('lightbox');
  const lightboxImg = document.getElementById('lightbox-img');
  const lightboxCaption = document.getElementById('lightbox-caption');
  lightbox.classList.add('active');
  lightboxImg.src = imgElement.src;
  lightboxCaption.textContent = imgElement.alt;
  document.body.style.overflow = 'hidden';
}

function closeLightbox() {
  document.getElementById('lightbox').classList.remove('active');
  document.body.style.overflow = 'auto';
}

document.addEventListener('keydown', e => e.key === 'Escape' && closeLightbox());
document.getElementById('lightbox-img')?.addEventListener('click', e => e.stopPropagation());
