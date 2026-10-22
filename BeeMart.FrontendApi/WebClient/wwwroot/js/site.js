window.BeeMart = (() => {
    const rupiah = n => new Intl.NumberFormat('id-ID', {
        style: 'currency', currency: 'IDR', maximumFractionDigits: 0
    }).format(n);

    // Waktu disimpan UTC di database, ditampilkan dalam WIB
    const wib = iso => new Intl.DateTimeFormat('sv-SE', {
        timeZone: 'Asia/Jakarta', dateStyle: 'short', timeStyle: 'short'
    }).format(new Date(iso));

    function toast(message, type = 'error') {
        const box = document.getElementById('toastContainer');
        if (!box) return;
        const t = document.createElement('div');
        t.className = 'toast toast-' + type;
        t.textContent = message;
        box.appendChild(t);
        setTimeout(() => t.remove(), 4000);
    }

    // Memanggil WebClient (/ui/...). Kalau 401, kembali ke Login.
    async function api(url, options) {
        let res;
        try {
            res = await fetch(url, options);
        } catch {
            toast('Tidak dapat terhubung ke server.');
            return null;
        }
        if (res.status === 401) {
            window.location.href = '/Account/Login';
            return null;
        }
        let data = null;
        try { data = await res.json(); } catch { }
        return { ok: res.ok, status: res.status, data };
    }

    return { rupiah, wib, toast, api };
})();