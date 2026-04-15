# HakedisCheck

Excel tabanli hak edis dogrulama araci.

Bu uygulama, ayni aya ait:

- personel izin listesi
- mesai / puantaj dosyasi
- alt yuklenici hak edis dosyasi

arasindaki tutarliligi kontrol eder.

## Proje Yapisi

- `HakedisCheck.Core`: Excel okuma, kolon profilleri, eslestirme, agregasyon ve dogrulama mantigi
- `HakedisCheck.App`: WinForms arayuzu
- `HakedisCheck.Tests`: core davranis testleri

## Gereksinimler

Uygulamayi sorunsuz calistirmak icin Windows ortaminda su iki yoldan birini kullan:

### Secenek 1: Visual Studio ile

- Windows 10 veya Windows 11
- Visual Studio 2022
- Visual Studio Installer icinden `.NET desktop development` workload'u

### Secenek 2: Sadece .NET SDK ile

- Windows 10 veya Windows 11
- `.NET 8 SDK`

## Visual Studio ile Calistirma

1. Repoyu indir:

```bash
git clone https://github.com/murataslan1/hakedis-check.git
cd hakedis-check
```

2. `HakedisCheck.sln` dosyasini Visual Studio ile ac.
3. `Solution Explorer` icinde `HakedisCheck.App` projesine sag tikla.
4. `Set as Startup Project` sec.
5. `F5` ile calistir.

### Onemli Not

Eger `A project with an output type of Class Library cannot be started directly` hatasi gorursen:

- baslangic projesi yanlis secilmistir
- `HakedisCheck.App` projesini `Startup Project` yapman gerekir

## Uygulama Nasil Kullanilir

Uygulama acildiginda sirasiyla:

1. Izin dosyasini sec
2. Mesai / puantaj dosyasini sec
3. Hak edis dosyasini sec
4. Gerekirse `Kolon Esleme` ekranindan baslik satiri, veri satiri ve kolonlari duzelt
5. Hak edis icin ay sayfasini sec, ornegin `Mart2026`
6. `Kontrolu Calistir` butonuna bas
7. Sonuclari grid uzerinde incele
8. Istersen `Excel Raporu` ile renkli rapor al

Rapor durumlari:

- `OK`: eslesme var
- `HATA`: fark var
- `EKSIK`: personel veya veri diger dosyada yok

## Profil Kaydetme

- Her Excel tipi icin kolon eslemesi profil olarak saklanabilir
- Profiller su klasore yazilir:

```text
%AppData%\HakedisCheck\profiles
```

Bir sonraki calistirmada ayni profil tekrar yuklenebilir.

## Komut Satirindan Calistirma

Windows ortaminda:

```bash
dotnet run --project HakedisCheck.App/HakedisCheck.App.csproj
```

## EXE Olarak Almak

Tek dosya `.exe` olusturmak icin Windows ortaminda su komutu calistir:

```bash
dotnet publish HakedisCheck.App/HakedisCheck.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/win-x64
```

Olusan dosya:

```text
publish\win-x64\HakedisCheck.exe
```

Bu `.exe` dosyasi hedef Windows makinede ayrica .NET kurulumu olmadan calisacak sekilde uretilir.

## Visual Studio'dan EXE Publish

1. `HakedisCheck.App` projesine sag tikla
2. `Publish...`
3. `Folder` sec
4. Hedef klasor belirle
5. Ayarlarda:
   - `Configuration`: `Release`
   - `Target runtime`: `win-x64`
   - `Deployment mode`: `Self-contained`
   - `Produce single file`: aktif
6. `Publish` de

## Testler

Core testlerini calistirmak icin:

```bash
dotnet test HakedisCheck.Tests/HakedisCheck.Tests.csproj
```

## Notlar

- Ornek Excel dosyalari repoya dahil edilmedi.
- WinForms proje yapisi Windows icin hazirlandi.
- Mac ortaminda `WindowsDesktop SDK` olmadigi icin WinForms proje dogrudan derlenmez; uygulamayi Windows'ta build/publish etmelisin.
