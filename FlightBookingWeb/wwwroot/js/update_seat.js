function renderUpgradePayPalButton(ticketId, targetSeatClass, containerSelector) {
    paypal.Buttons({
        createOrder: function (data, actions) {
            // Gọi API server để tạo order PayPal
            return fetch('/Account/CreateUpgradePayPalOrder', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() // Nếu có AntiForgery
                },
                body: JSON.stringify({
                    ticketId: ticketId,
                    targetSeatClass: targetSeatClass
                })
            })
                .then(function (res) {
                    return res.json();
                })
                .then(function (data) {
                    return data.id; // Trả về orderId cho PayPal SDK
                });
        },
        onApprove: function (data, actions) {
            // Gọi API server để xác nhận thanh toán
            return fetch('/Account/CaptureUpgradePayPalOrder?orderId=' + data.orderID, {
                method: 'POST'
            })
                .then(function (res) {
                    return res.json();
                })
                .then(function (details) {
                    alert('Thanh toán thành công! Vé của bạn đã được nâng hạng.');
                    window.location.href = '/Account/BookingHistory';
                });
        },
        onError: function (err) {
            alert('Có lỗi xảy ra khi thanh toán: ' + err);
        }
    }).render(containerSelector);
}
