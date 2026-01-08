// PayPal Integration
// Replace 'YOUR_PAYPAL_CLIENT_ID' in index.html with your actual PayPal Client ID

// Wait for PayPal SDK to load
if (typeof paypal !== 'undefined') {
    initPayPalButtons();
} else {
    console.error('PayPal SDK not loaded');
}

function initPayPalButtons() {
    // One-time payment button
    paypal.Buttons({
    // Set up the transaction
    createOrder: function(data, actions) {
        return actions.order.create({
            purchase_units: [{
                description: 'Volume Bubble Indicator - NinjaTrader 8 (Lifetime License)',
                amount: {
                    currency_code: 'USD',
                    value: '150.00'
                }
            }]
        });
    },
    
    // Finalize the transaction
    onApprove: function(data, actions) {
        return actions.order.capture().then(function(orderData) {
            // Show success message
            showSuccessMessage(orderData, 'lifetime');
            
            // Send order details to your server
            sendOrderToServer(orderData, 'lifetime');
        });
    },
    
    // Handle errors
    onError: function(err) {
        console.error('PayPal error:', err);
        alert('An error occurred with the payment. Please try again or contact support.');
    }
    }).render('#paypal-button-container-onetime');

    // Monthly subscription button
    paypal.Buttons({
    style: {
        shape: 'rect',
        color: 'blue',
        layout: 'vertical',
        label: 'subscribe'
    },
    // Set up the subscription
    createSubscription: function(data, actions) {
        console.log('Creating subscription with Plan ID: P-0UY85111HW8408537NFP7UAQ');
        return actions.subscription.create({
            'plan_id': 'P-0UY85111HW8408537NFP7UAQ'
        }).catch(function(err) {
            console.error('Subscription creation error:', err);
            alert('Fehler beim Erstellen des Abonnements: ' + (err.message || 'Bitte versuchen Sie es erneut oder kontaktieren Sie den Support.'));
            throw err;
        });
    },
    
    // Handle subscription approval
    onApprove: function(data, actions) {
        showSuccessMessage({
            id: data.subscriptionID,
            subscription: true
        }, 'subscription');
        
        // Send subscription details to your server
        sendOrderToServer({
            id: data.subscriptionID,
            orderID: data.orderID,
            subscription: true
        }, 'subscription');
    },
    
    // Handle errors
    onError: function(err) {
        console.error('PayPal subscription error details:', err);
        console.error('Error name:', err.name);
        console.error('Error message:', err.message);
        console.error('Error details:', JSON.stringify(err, null, 2));
        alert('Fehler beim Abonnement: ' + (err.message || err.toString()) + '\n\nBitte überprüfen Sie die Browser-Konsole für Details oder kontaktieren Sie den Support.');
    }
    }).render('#paypal-button-container-subscription');
}

// End of PayPal initialization

// Show success message after purchase
function showSuccessMessage(orderData, type) {
    const purchaseSection = document.querySelector('#purchase .container');
    
    const message = type === 'subscription' 
        ? `
            <h3>✓ Subscription Activated!</h3>
            <p>Thank you for subscribing!</p>
            <p>Subscription ID: ${orderData.id}</p>
            <p>You will receive an email with the download link and subscription details within 5 minutes.</p>
        `
        : `
            <h3>✓ Payment Successful!</h3>
            <p>Thank you for your purchase!</p>
            <p>Order ID: ${orderData.id}</p>
            <p>You will receive an email with the download link within 5 minutes.</p>
        `;
    
    // Create success message
    const successHTML = `
        <div class="success-message show">
            ${message}
            <p>Please check your spam folder if you don't see it.</p>
        </div>
    `;
    
    purchaseSection.innerHTML = '<h2>Thank You!</h2>' + successHTML;
}

// Send order details to your server (backend required)
function sendOrderToServer(orderData, type) {
    // Send to Netlify Function
    fetch('/.netlify/functions/send-download', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            orderId: orderData.id,
            email: orderData.payer?.email_address || 'customer@email.com',
            name: orderData.payer?.name?.given_name || 'Customer',
            type: type,
            subscription: orderData.subscription || false
        })
    })
    .then(response => response.json())
    .then(data => {
        console.log('Email sent successfully:', data);
    })
    .catch(error => {
        console.error('Error sending email:', error);
    });
    // This is where you would send the order data to your backend server
    // to process the order, send the download link via email, etc.
    
    // Example implementation:
    /*
    fetch('https://your-backend-server.com/api/process-order', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            orderId: orderData.id,
            customerEmail: orderData.payer.email_address,
            customerName: orderData.payer.name.given_name + ' ' + orderData.payer.name.surname,
            amount: orderData.purchase_units[0].amount.value,
            product: 'Volume Bubble Indicator'
        })
    })
    .then(response => response.json())
    .then(data => {
        console.log('Order processed:', data);
    })
    .catch(error => {
        console.error('Error processing order:', error);
    });
    */
    
    console.log('Order Data:', orderData);
}

// Smooth scroll for navigation links
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

// Lightbox functionality
function openLightbox(imgElement) {
    const lightbox = document.getElementById('lightbox');
    const lightboxImg = document.getElementById('lightbox-img');
    const lightboxCaption = document.getElementById('lightbox-caption');
    
    lightbox.classList.add('active');
    lightboxImg.src = imgElement.src;
    lightboxCaption.textContent = imgElement.alt;
    document.body.style.overflow = 'hidden'; // Prevent background scrolling
}

function closeLightbox() {
    const lightbox = document.getElementById('lightbox');
    lightbox.classList.remove('active');
    document.body.style.overflow = 'auto'; // Re-enable scrolling
}

// Close lightbox with Escape key
document.addEventListener('keydown', function(event) {
    if (event.key === 'Escape') {
        closeLightbox();
    }
});

// Prevent lightbox close when clicking on the image
document.getElementById('lightbox-img')?.addEventListener('click', function(event) {
    event.stopPropagation();
});

// Mobile menu toggle (optional, for future enhancement)
// You can add a hamburger menu for mobile devices if needed
