// PayPal Subscription Integration

// Wait for PayPal SDK to load
if (typeof paypal !== 'undefined') {
    initPayPalButtons();
} else {
    window.addEventListener('load', function() {
        if (typeof paypal !== 'undefined') {
            initPayPalButtons();
        } else {
            console.error('PayPal SDK not loaded');
        }
    });
}

function initPayPalButtons() {
    // Monthly subscription button
    paypal.Buttons({
        style: {
            shape: 'rect',
            color: 'blue',
            layout: 'vertical',
            label: 'subscribe'
        },
        createSubscription: function(data, actions) {
            return actions.subscription.create({
                'plan_id': 'P-0UY85111HW8408537NFP7UAQ'
            });
        },
        onApprove: function(data, actions) {
            showSuccessMessage({
                id: data.subscriptionID,
                subscription: true
            }, 'subscription');
            
            sendOrderToServer({
                id: data.subscriptionID,
                orderID: data.orderID,
                subscription: true
            }, 'subscription');
        },
        onError: function(err) {
            console.error('PayPal subscription error:', err);
            alert('Error with subscription. Please try again or contact support.');
        }
    }).render('#paypal-button-subscription');
}

function showSuccessMessage(orderData, type) {
    const purchaseSection = document.querySelector('#purchase .container');
    
    const message = `
        <h3>✓ Subscription Activated!</h3>
        <p>Thank you for subscribing!</p>
        <p>Subscription ID: ${orderData.id}</p>
        <p>📧 You will receive an email with the download link and subscription details within 5 minutes.</p>
        <p>Please check your spam folder if you don't see it.</p>
        <br>
        <a href="#installation" class="btn btn-primary">View Installation Guide</a>
    `;
    
    purchaseSection.innerHTML = '<h2>Thank You!</h2><div class="success-message show">' + message + '</div>';
}

function sendOrderToServer(orderData, type) {
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
    document.body.style.overflow = 'hidden';
}

function closeLightbox() {
    const lightbox = document.getElementById('lightbox');
    lightbox.classList.remove('active');
    document.body.style.overflow = 'auto';
}

document.addEventListener('keydown', function(event) {
    if (event.key === 'Escape') {
        closeLightbox();
    }
});

document.getElementById('lightbox-img')?.addEventListener('click', function(event) {
    event.stopPropagation();
});
