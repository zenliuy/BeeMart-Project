# BeeMart - Aplikasi Kasir (Point of Sale)

Aplikasi kasir web berbasis **C# .NET 8** dengan arsitektur terpisah (Separation of Concerns):

- **BeeMart.BackendApi**: ASP.NET Core Web API + Entity Framework Core + SQL Server. Semua logika bisnis, validasi, otorisasi, dan akses database ada di sini.
- **WebClient**: ASP.NET Core MVC (tanpa Bootstrap). Hanya mengonsumsi data lewat HTTP ke Backend API, **tidak mengakses database**.
- **Database**: SQL Server (script ada di folder `Database`).

## Fitur

- Login dan Register dengan role **Admin** dan **Kasir** (nilai di database: `Admin` dan `Cashier`)
- Password disimpan sebagai hash **BCrypt**
- New Transaction: jam real-time, pencarian barang (maksimal 10 hasil), dialog Pilih, keranjang, metode pembayaran dari API, Invoice Number otomatis
- History Transaction: filter tanggal (date picker), pagination maksimal 20 data per halaman, urut tanggal-jam ascending
- Kasir hanya melihat transaksinya sendiri, Admin melihat semua kasir (dibatasi di backend lewat JWT)
- Endpoint versioning (`/api/v1`), format JSON, otentikasi JWT Bearer

## Format Invoice Number

`[5 digit IdUser][ddmmyy][3 digit running number]`, contoh `93837221026001`.

Running number berurutan per hari (zona WIB) untuk semua kasir. Dibuat oleh backend dan tidak bisa diubah user.

## Cara Run

1. Siapkan database

Jalankan script di SSMS **berurutan** (folder `Database\Scripts\Setup`):

1. `01-create-table-and-data.sql` : membuat 5 tabel dan data awal (3 metode pembayaran, 370 produk)
2. `03-seed-users.sql` : membuat akun awal `admin` dan `kasir1`

Pastikan database bernama **BeeMart** sudah dibuat dan dipilih sebelum menjalankan script (`CREATE DATABASE BeeMart;`).

2. Atur connection string

Buka `BeeMart.BackendApi\appsettings.json` dan sesuaikan bila perlu:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=BeeMart;Trusted_Connection=True;TrustServerCertificate=True"
}
```

Jika memakai SQL Server Express: `Server=.\\SQLEXPRESS;...`
 
3. Jalankan Backend API

```
cd BeeMart.BackendApi
dotnet run
```

API berjalan di `http://localhost:5274`. Dokumentasi Swagger: `http://localhost:5274/swagger`.

4. Jalankan WebClient (terminal baru)

```
cd WebClient
dotnet run
```

Buka `http://localhost:5105`. Alamat API diatur di `WebClient\appsettings.json` (`Api:BaseUrl`). Jika port API berbeda, ubah nilainya.

> API harus berjalan lebih dulu sebelum WebClient dipakai.

## Akun Uji

| Username | Password | Role |
|---|---|---|
| admin | Admin123! | Admin |
| kasir1 | Kasir123! | Kasir (Cashier) |

Akun baru bisa dibuat lewat halaman Register.

## Endpoint API (v1)

| Method | Endpoint | Akses |
|---|---|---|
| POST | `/api/v1/auth/register` | Publik |
| POST | `/api/v1/auth/login` | Publik |
| GET | `/api/v1/products?search=` | Login |
| GET | `/api/v1/payment-methods` | Login |
| GET | `/api/v1/transactions/next-invoice` | Login |
| POST | `/api/v1/transactions` | Admin, Kasir |
| GET | `/api/v1/transactions?date=yyyy-MM-dd&page=1` | Admin, Kasir |

## Aturan Bisnis

- Harga, diskon, subtotal, dan total **dihitung ulang di server**; angka dari client tidak dipercaya.
- Subtotal = Qty x Harga x (1 - Diskon/100).
- Stok dicek dan dikurangi di backend dalam satu database transaction.
- Waktu disimpan UTC, ditampilkan dan difilter dalam WIB (Asia/Jakarta).
- Pesan error: `Invalid login`, `username has already exists`, `Jumlah barang yang dimasukkan melebihi sisa stok.`

## Struktur Folder


BeeMart
- BeeMart.BackendApi   (Controllers, Data, Dtos, Models, Services)
- WebClient            (Controllers, Models, Services, Views, wwwroot)
- Database\Scripts     (SQL)
- BeeMart.slnx
 README.md
