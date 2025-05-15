document.addEventListener('DOMContentLoaded', function () {
    if (window.paypal) {
        paypal.Buttons({
            createOrder: function (data, actions) {
                // Gọi API backend để tạo order, nhận về orderId
                return fetch('/booking/create-paypal-order', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    }
                })
                .then(res => res.json())
                .then(data => data.id); // Trả về orderId cho PayPal SDK
            },
            onApprove: function (data, actions) {
                // Gọi API backend để capture order
                return fetch(`/booking/capture-paypal-order?orderId=${data.orderID}`, {
                    method: 'POST'
                })
                .then(res => res.json())
                .then(details => {
                    alert('Thanh toán thành công!');
                    window.location.href = '/Booking/Success'; // hoặc trang xác nhận
                });
            },
            onError: function (err) {
                alert('Có lỗi khi thanh toán với PayPal');
                console.error(err);
            }
        }).render('#paypal-button-container');
    }
});
