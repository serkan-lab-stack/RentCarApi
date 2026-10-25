# MSE C# - Rent Car Management API

Araç kiralama, kiralama geçmişi, bakım ve hasar süreçlerini yöneten bir Web API projesidir.
ABP Framework **Layered Monolith (Katmanlı Monolit)** başlangıç şablonu ile geliştirilmiştir.

## Teknolojiler

| Konu | Kullanılan |
|---|---|
| Platform | .NET 10, ASP.NET Core Web API |
| Mimari | ABP Framework 10 - Layered (DDD) şablonu (Domain.Shared, Domain, Application.Contracts, Application, EntityFrameworkCore, HttpApi, HttpApi.Host, DbMigrator) |
| Veritabanı | SQL Server (varsayılan: LocalDB), Entity Framework Core **Code-First + Migrations** |
| Kimlik doğrulama | **JWT** access token (OpenIddict), `Authorization: Bearer <token>` |
| Yetkilendirme | Rol ve permission tabanlı (`admin` rolü, `RentCar.Vehicles.*`, `RentCar.Rentals.Manage`) |
| Loglama | **Serilog** - `src/RentCar.HttpApi.Host/Logs/logs.txt` |
| Nesne eşleme | Mapperly |
| Dokümantasyon | Swagger UI |

## Çalıştırma

Gereksinimler: .NET 10 SDK, SQL Server (LocalDB, Express ya da tam sürüm), Node.js (yalnızca bir kez `abp install-libs` için).

```powershell
# 1) Araçlar (bir kez)
dotnet tool install -g Volo.Abp.Studio.Cli
dotnet tool install -g dotnet-ef
dotnet dev-certs https --trust

# 2) Tarayıcı kütüphaneleri (giriş sayfası için, bir kez)
abp install-libs

# 3) Veritabanını oluştur ve başlangıç verilerini (admin kullanıcısı, yetkiler) ekle
cd src\RentCar.DbMigrator
dotnet run

# 4) API'yi başlat
cd ..\RentCar.HttpApi.Host
dotnet run
```

Swagger: `https://localhost:44354/swagger` (port `launchSettings.json` / `appsettings.json` içindeki `SelfUrl` değeridir).

**Bağlantı dizesi:** `src/RentCar.DbMigrator/appsettings.json` ve `src/RentCar.HttpApi.Host/appsettings.json` içindeki `ConnectionStrings:Default`.
Varsayılan: `Server=(LocalDb)\MSSQLLocalDB;Database=RentCar;Trusted_Connection=True;TrustServerCertificate=true`.
Tam SQL Server için örnek: `Server=localhost;Database=RentCar;Trusted_Connection=True;TrustServerCertificate=true`.

**Hazır hesap:** kullanıcı adı `admin`, parola `1q2w3E*` (DbMigrator oluşturur).

## Kimlik doğrulama (JWT)

1. **Kayıt:** `POST /api/account/register`
   ```json
   { "userName": "john", "emailAddress": "john@example.com", "password": "Test123*", "appName": "RentCar" }
   ```
2. **Giriş:** `POST /api/auth/login`
   ```json
   { "userName": "john", "password": "Test123*" }
   ```
   Yanıt: `accessToken` (JWT), `tokenType` (Bearer), `expiresIn` (saniye). Hatalı bilgide `401` döner.
   (Standart OpenIddict token endpoint'i de kullanılabilir: `POST /connect/token`.)
3. **Kullanım:** Korumalı endpoint'lere `Authorization: Bearer <accessToken>` başlığı ile istek atılır.
4. **Swagger:** **Authorize** butonu ile (scope: `RentCar`) ya da giriş endpoint'inden alınan token ile denenebilir.
   Daha önce hatalı denemeler yapılmış bir tarayıcıda Authorize 401 verirse gizli pencere kullanın.

## Roller ve yetkiler

| İşlem | Kim yapabilir |
|---|---|
| Araç ekleme, güncelleme, silme, bakıma alma / bakımı tamamlama | Yalnızca **Admin** |
| Araçları listeleme / görüntüleme | Giriş yapmış her kullanıcı |
| Ehliyet/profil bilgisini güncelleme, kiralama, uzatma, iade, hasar bildirimi ve ödeme | Giriş yapmış kullanıcı (**kendi** kayıtları) |
| Tüm kiralamaları / hasar bildirimlerini listeleme, başkası adına iade ve hasar bildirimi | Yalnızca **Admin** (`RentCar.Rentals.Manage`) |

## Endpoint'ler

**Kimlik doğrulama ve kullanıcı**

| Metot | Yol | Açıklama |
|---|---|---|
| POST | `/api/account/register` | Kayıt |
| POST | `/api/auth/login` | Giriş, JWT döner |
| GET / PUT | `/api/account/my-profile` | Ad, soyad, telefon (ABP hazır endpoint'i) |
| GET / PUT | `/api/app/customer-profile/my-profile` | Ehliyet bilgileri, doğum tarihi, adres |

**Araç yönetimi** (`/api/app/vehicle`)

| Metot | Yol | Açıklama |
|---|---|---|
| GET | `/api/app/vehicle` | Liste (filtre: `Filter`, `Status`; sayfalama) |
| GET | `/api/app/vehicle/{id}` | Detay |
| POST | `/api/app/vehicle` | Ekle (Admin) |
| PUT | `/api/app/vehicle/{id}` | Güncelle (Admin) |
| DELETE | `/api/app/vehicle/{id}` | Sil (Admin, kirada olan araç silinemez) |
| POST | `/api/app/vehicle/{id}/send-to-maintenance` | Bakıma al (Admin) |
| POST | `/api/app/vehicle/{id}/complete-maintenance` | Bakımı tamamla (Admin) |

**Kiralama yönetimi** (`/api/app/rental`)

| Metot | Yol | Açıklama |
|---|---|---|
| POST | `/api/app/rental/rent` | Araç kirala (`vehicleId`, `rentalDays`) |
| POST | `/api/app/rental/{id}/extend` | Süreyi uzat (`additionalDays`) |
| POST | `/api/app/rental/{id}/return` | İade et (`endMileage`); ücret hesaplanır |
| GET | `/api/app/rental/my-rentals` | Kendi kiralama geçmişim |
| GET | `/api/app/rental/{id}` | Detay (sahibi ya da Admin) |
| GET | `/api/app/rental` | Tüm kiralamalar (Admin) |

**Hasar yönetimi** (`/api/app/damage-report`)

| Metot | Yol | Açıklama |
|---|---|---|
| POST | `/api/app/damage-report` | Hasar bildir (`rentalId`, `description`, `severity`: 1 Minor, 2 Moderate, 3 Major) |
| POST | `/api/app/damage-report/{id}/pay` | Hasar bedelini öde (`amount`) |
| GET | `/api/app/damage-report/my-reports` | Kendi hasar bildirimlerim |
| GET | `/api/app/damage-report/{id}` | Detay |
| GET | `/api/app/damage-report` | Tümü (Admin) |

## İş kuralları

- **Araç durumu:** `Available` (müsait), `Rented` (kirada), `Maintenance` (bakımda). Müsait olmayan araç kiralanamaz.
- **Kiralama şartı:** Kullanıcının ehliyet bilgisi kayıtlı olmalı ve ehliyet, kiralama bitiş tarihinde hâlâ geçerli olmalı.
- **Temel ücret:** gün sayısı x günlük fiyat. Fiyatlar kiralama anında kopyalanır, sonradan değişen araç fiyatı geçmiş kiralamayı etkilemez.
- **Ekstra km ücreti:** gün sayısı x günlük km limiti kadar km ücretsizdir. Fazlası `ekstra km ücreti` ile çarpılır.
- **Geç iade ücreti:** Planlanan bitişten sonra başlayan her gün için günlük fiyat x 1,5.
- **Süre uzatma:** Yalnızca aktif ve süresi dolmamış kiralamada yapılır (geç ücretten kaçınmak için süresi dolan kiralama uzatılamaz).
- **Otomatik bakım:** İadede araç kilometresi güncellenir. Son bakımdan beri `maintenanceIntervalKm` kadar km yapıldıysa araç otomatik `Maintenance` durumuna geçer.
- **Hasar bedeli:** kiralamadaki günlük fiyat x şiddet çarpanı (Minor x1, Moderate x3, Major x10). Ödeme tutarı hasar bedeline tam eşit olmalıdır. **Major** hasar, araç o an müsaitse aracı otomatik bakıma alır.
- **Admin için not:** Admin iade sırasında `returnDate` göndererek iade tarihini belirleyebilir (geriye dönük düzeltme / test amaçlı).

## Hata yönetimi ve loglama

İş kuralı ihlalleri `RentCar:000xx` kodlu `BusinessException` olarak fırlatılır, yerelleştirilmiş mesajla JSON döner (`403`).
Doğrulama hataları `400`, giriş hataları `401` döner. Tüm hatalar ve başarısız giriş denemeleri Serilog ile
`src/RentCar.HttpApi.Host/Logs/logs.txt` dosyasına yazılır.

## Kod yapısı (bu projede eklenenler)

```
RentCar.Domain.Shared   Vehicles/, Rentals/, DamageReports/, Customers/   (enum, sabitler, hata kodları)
RentCar.Domain          Vehicle, Rental, DamageReport, CustomerProfile    (entity + iş kuralları)
RentCar.Application.Contracts   DTO'lar, IxxxAppService, permission tanımları
RentCar.Application     VehicleAppService, RentalAppService, DamageReportAppService, CustomerProfileAppService
RentCar.EntityFrameworkCore     RentCarDbContext, Migrations
RentCar.HttpApi         Controllers/AuthController (giriş)
```
