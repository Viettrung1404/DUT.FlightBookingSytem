document.addEventListener('DOMContentLoaded', function () {
    const paypalButton = document.getElementById('paypal-button');

    if (paypalButton) {
        paypalButton.addEventListener('click', async function (e) {
            e.preventDefault();

            const flightId = document.getElementById('flightId').value;
            const amount = document.getElementById('totalAmount').value;
            const currency = document.getElementById('currency').value;
            const description = `Flight Booking - ${flightId}`;

            try {
                const response = await fetch('/booking/create-paypal-order', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify({
                        amount: amount,
                        currency: currency,
                        description: description
                    })
                });

                if (!response.ok) {
                    throw new Error('Failed to create order');
                }

                const data = await response.json();
                window.location.href = data.approvalUrl;

            } catch (error) {
                console.error('Error:', error);
                alert('Failed to initiate PayPal payment');
            }
        });
    }
});