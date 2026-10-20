(() => {
    const { rupiah, toast, api } = window.BeeMart;
    const $ = id => document.getElementById(id);

    let cart = [];
    let picked = null;

    // ---------- jam real-time (WIB) ----------
    function tick() {
        $('clock').textContent = new Date().toLocaleString('id-ID', { timeZone: 'Asia/Jakarta' });
    }
    tick();
    setInterval(tick, 1000);

    // ---------- helper ----------
    const weightText = w => (w >= 1000 ? (w / 1000) + ' Kg' : w + ' g');
    const subtotalOf = i => i.qty * i.price * (1 - i.discount / 100);

    function cell(text) {
        const td = document.createElement('td');
        td.textContent = text;
        return td;
    }

    // ---------- data awal ----------
    async function loadInvoice() {
        const r = await api('/ui/next-invoice');
        if (r && r.ok) $('invoiceNo').textContent = r.data.invoiceNo;
    }

    async function loadPaymentMethods() {
        const r = await api('/ui/payment-methods');
        if (!r || !r.ok) { toast('Gagal memuat metode pembayaran.'); return; }
        const sel = $('paymentMethod');
        sel.innerHTML = '';
        r.data.forEach(m => {
            const o = document.createElement('option');
            o.value = m.idPaymentMethod;
            o.textContent = m.methodName;
            sel.appendChild(o);
        });
    }

    // ---------- pencarian ----------
    async function search() {
        const keyword = $('searchInput').value.trim();
        const r = await api('/ui/products?search=' + encodeURIComponent(keyword));
        if (!r) return;
        if (!r.ok) { toast(r.data?.message || 'Gagal mencari barang.'); return; }

        const tbody = $('searchResults').querySelector('tbody');
        tbody.innerHTML = '';
        $('searchWrap').classList.remove('hidden');

        if (r.data.length === 0) {
            const tr = document.createElement('tr');
            const td = cell('Barang tidak ditemukan.');
            td.colSpan = 6;
            tr.appendChild(td);
            tbody.appendChild(tr);
            return;
        }

        r.data.forEach(p => {
            const tr = document.createElement('tr');
            tr.appendChild(cell(p.name));
            tr.appendChild(cell(weightText(p.weight)));
            tr.appendChild(cell(p.stock));
            tr.appendChild(cell(rupiah(p.price)));
            tr.appendChild(cell(p.discount + '%'));

            const td = document.createElement('td');
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'btn-pilih';
            btn.textContent = 'Pilih';
            btn.addEventListener('click', () => pick(p));
            td.appendChild(btn);
            tr.appendChild(td);
            tbody.appendChild(tr);
        });
    }

    $('searchInput').addEventListener('keydown', e => {
        if (e.key === 'Enter') { e.preventDefault(); search(); }
    });

    // ---------- dialog Pilih ----------
    function pick(p) {
        picked = p;
        $('pickName').textContent = p.name;
        $('pickPrice').textContent = rupiah(p.price);
        $('pickStock').textContent = p.stock;
        $('pickQty').value = 1;
        $('pickDialog').classList.remove('hidden');
        $('pickQty').focus();
    }

    $('btnAddToCart').addEventListener('click', () => {
        if (!picked) return;
        const qty = parseInt($('pickQty').value, 10);

        if (!Number.isInteger(qty) || qty <= 0) {
            toast('Qty harus lebih dari 0.');
            return;
        }

        const existing = cart.find(i => i.idProduct === picked.idProduct);
        const totalQty = qty + (existing ? existing.qty : 0);

        if (totalQty > picked.stock) {
            toast('Jumlah barang yang dimasukkan melebihi sisa stok.');
            return;
        }

        if (existing) {
            existing.qty = totalQty;
        } else {
            cart.push({
                idProduct: picked.idProduct, name: picked.name,
                price: picked.price, discount: picked.discount,
                stock: picked.stock, qty
            });
        }

        $('pickDialog').classList.add('hidden');
        picked = null;
        renderCart();
    });

    // ---------- keranjang ----------
    function renderCart() {
        const tbody = $('cartTable').querySelector('tbody');
        tbody.innerHTML = '';
        let total = 0;

        cart.forEach((item, idx) => {
            const sub = subtotalOf(item);
            total += sub;

            const tr = document.createElement('tr');
            tr.appendChild(cell(item.name));
            tr.appendChild(cell(rupiah(item.price)));
            tr.appendChild(cell(item.discount + '%'));
            tr.appendChild(cell(item.qty));
            tr.appendChild(cell(rupiah(sub)));

            const td = document.createElement('td');
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'btn-hapus';
            btn.textContent = 'Hapus';
            btn.addEventListener('click', () => { cart.splice(idx, 1); renderCart(); });
            td.appendChild(btn);
            tr.appendChild(td);
            tbody.appendChild(tr);
        });

        // Total hanya tampilan. Angka resmi dihitung ulang oleh server.
        $('totalText').textContent = 'Total: ' + rupiah(total);
    }

    // ---------- submit ----------
    $('btnSubmit').addEventListener('click', async () => {
        if (cart.length === 0) { toast('Keranjang masih kosong.'); return; }

        const btn = $('btnSubmit');
        btn.disabled = true;

        // Harga tidak dikirim. Server mengambilnya dari database.
        const body = {
            idPaymentMethod: parseInt($('paymentMethod').value, 10),
            items: cart.map(i => ({ idProduct: i.idProduct, qty: i.qty }))
        };

        const r = await api('/ui/submit', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });

        btn.disabled = false;
        if (!r) return;

        if (!r.ok) {
            toast(r.data?.message || 'Transaksi gagal.');
            return;
        }

        toast('Transaksi berhasil. Invoice: ' + r.data.invoiceNo, 'success');
        // Reset halaman ke kondisi awal + invoice baru
        cart = [];
        picked = null;
        renderCart();
        $('searchInput').value = '';
        $('searchWrap').classList.add('hidden');
        $('pickDialog').classList.add('hidden');
        loadInvoice();
    });

    loadInvoice();
    loadPaymentMethods();
    renderCart();
})();