// Small UI helpers: show bootstrap toast if present
document.addEventListener('DOMContentLoaded', function () {
    try {
        var toastEl = document.getElementById('liveToast');
        if (toastEl) {
            var toast = new bootstrap.Toast(toastEl, { delay: 4000 });
            toast.show();
        }
    } catch (e) {
        // ignore
    }
});
