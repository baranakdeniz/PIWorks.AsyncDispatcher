# Distributed Async Command Dispatcher with RabbitMQ

## Proje Hakkında
Bu proje, .NET Core ortamında RabbitMQ altyapısı kullanılarak geliştirilmiş, CQRS tabanlı asenkron bir görev dağıtım (dispatch) ve yönetim sistemidir. Dağıtık mimarilerde uzun süren işlemlerin (ör. rapor üretimi) kuyruğa alınması, iş yükünün sunucular (worker nodes) arasında dengelenmesi, durum (state) senkronizasyonunun sağlanması ve çapraz-sunucu (cross-node) iptal (cancellation) işlemlerinin güvenli bir şekilde yönetilmesini amaçlar.

## Temel Özellikler

* **Asenkron Görev İşleme (Competing Consumers):** Ağır iş yükleri RabbitMQ üzerinden adil dağıtım (Fair Dispatch / PrefetchCount: 1) prensibiyle boşta olan worker'lara atanır.
* **Dağıtık İptal Mekanizması (Distributed Cancellation):** Bir işlemin hangi sunucuda (WorkerId) çalıştığı takip edilir. İptal isteği geldiğinde, işlem farklı bir makinede çalışıyor olsa dahi ağ üzerinden iptal sinyali gönderilerek hedef makinedeki `CancellationToken` tetiklenir.
* **Ağ İçi Durum Senkronizasyonu (State Broadcasting):** Komutların durumları (Running, Finished, Cancelled), `Topic Exchange` üzerinden ağdaki tüm sunuculara anında yayınlanır ve In-Memory Tracker'lar güncel tutulur.
* **Performanslı Dinamik Çözümleme (Type Caching):** Reflection maliyetlerini önlemek amacıyla `ConcurrentDictionary` tabanlı Type Cache mekanizması kurgulanmıştır.
* **İzole Bağımlılık Yönetimi (Scoped DI):** Singleton olarak çalışan `HostedService` içerisinde bellek sızıntısını (memory leak) önlemek için her mesaja özel izole bir Service Scope açılır ve `CommandEnvelope` üzerinden Double Dispatch kurgusu ile işletilir.

## Mimari Kararlar ve Tasarım Desenleri

### 1. RabbitMQ Mesajlaşma Modelleri
Sistem, işin doğasına göre iki farklı kuyruk/mesajlaşma stratejisi kullanır:
* **Mode 0 (Competing Consumers):** Ana komutların (örneğin `CommandIntegrationEvent`) iletildiği ortaktır. Sadece tek bir worker işlemi devralır.
* **Mode 1 (Broadcast / Fanout):** Durum bildirimleri (`CommandStatusChangedIntegrationEvent`) ve iptal talepleri (`CancelCommandRequestedIntegrationEvent`) içindir. Mesaj, ağdaki tüm pod'lara klonlanarak ulaştırılır.

### 2. Double Dispatch ve Command Envelope
Kuyruktan gelen ham JSON mesajları, generic bir zarf olan `CommandEnvelope` içerisine alınır. `ExecuteAsync` metodu üzerinden dinamik olarak ayağa kaldırılan hedefe (Handler), güncel Dependency Injection `IServiceProvider`'ı parametre geçilerek yönlendirilir (Double Dispatch).

### 3. Subscription Registry (Rehber Mekanizması)
RabbitMQ üzerinden gelen bir mesajın hangi Handler sınıfı tarafından çözüleceği uygulamanın başlangıcında (`Program.cs`) hafızaya alınır. Arka plan servisi, gelen mesajın `Type` header bilgisine bakarak ilgili Handler'ı DI Container üzerinden dinamik olarak oluşturur.

## Gereksinimler
* .NET 8.0 SDK (veya üzeri)
* RabbitMQ (Docker üzerinden veya lokal kurulum)
* Visual Studio veya Visual Studio Code

## Kurulum ve Çalıştırma

### 1. RabbitMQ Ayarları
`appsettings.json` dosyasındaki RabbitMQ bağlantı bilgilerinizi ortamınıza göre güncelleyin.

### 2. Çoklu Düğüm (Multi-Node) Olarak Çalıştırma
Dağıtık yapıyı ve yük dengelemesini (load balancing) test etmek için uygulamayı iki farklı terminalde, iki farklı port üzerinden ayağa kaldırabilirsiniz:

**Terminal 1 (Node A):**
```bash
dotnet run --urls "http://localhost:5050"