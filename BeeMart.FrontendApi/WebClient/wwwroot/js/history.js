(() => {
    const { rupiah, wib, toast, api } = window.BeeMart;
    const $ = id => document.getElementById(id);

    // Isi tanggal dengan hari ini (WIB)
    $('filterDate').value = new Intl.DateTimeFormat('sv-SE', { timeZone: 'Asia/Jakarta' }).format(new Date());

    function cell(text) {
        const td = document.createElement('td');
        td.textContent = text;
        return td;
    }

    async function load(page) {
        const date = $('filterDate').value;
        if (!date) { toast('Tanggal wajib dipilih.'); return; }

        const r = await api('/ui/history?date=' + encodeURIComponent(date) + '&page=' + page);
        if (!r) return;
        if (!r.ok) { toast(r.data?.message || 'Gagal memuat riwayat.'); return; }

        render(r.data);
    }

    function render(data) {
        const tbody = $('historyTable').querySelector('tbody');
        tbody.innerHTML = '';

        if (data.items.length === 0) {
            const tr = document.createElement('tr');
            const td = cell('Tidak ada transaksi pada tanggal ini.');
            td.colSpan = 6;
            tr.appendChild(td);
            tbody.appendChild(tr);
        }

        data.items.forEach(t => {
            const tr = document.createElement('tr');
            tr.appendChild(cell(t.invoiceNo));
            tr.appendChild(cell(wib(t.transactionDate)));
            tr.appendChild(cell(t.cashierName));
            tr.appendChild(cell(t.totalItems));
            tr.appendChild(cell(t.paymentMethod));
            tr.appendChild(cell(rupiah(t.totalPrice)));
            tbody.appendChild(tr);
        });

        const pg = $('pagination');
        pg.innerHTML = '';
        for (let p = 1; p <= data.totalPages; p++) {
            const b = document.createElement('button');
            b.type = 'button';
            b.className = 'page-btn' + (p === data.page ? ' active' : '');
            b.textContent = p;
            b.addEventListener('click', () => load(p));
            pg.appendChild(b);
        }
    }

    $('btnSearch').addEventListener('click', () => load(1));
})();