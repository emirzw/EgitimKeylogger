# Eğitim Amaçlı C# Keylogger Projesi

![Dil](https://img.shields.io/badge/Dil-C%23-blueviolet)
![Framework](https://img.shields.io/badge/Framework-.NET%20Framework-blue)
![IDE](https://img.shields.io/badge/IDE-Visual%20Studio-orange)

Bu proje, yazılım mühendisliği dersi kapsamında geliştirilmiş eğitim amaçlı bir keylogger uygulamasıdır. Uygulamanın temel amacı, Windows işletim sisteminin alt seviye API'lerini (P/Invoke) kullanarak global klavye olaylarını yakalamak ve C# dilinin yeteneklerini sergilemektir.

## Özellikler

*   **Global Klavye Dinleme:** Uygulama odakta olmasa bile sistem genelindeki tüm klavye girişlerini yakalar.
*   **E-posta ile Raporlama:** Belirlenen karakter sayısına ulaşıldığında yakalanan girdileri belirtilen e-posta adresine gönderir.
*   **Türkçe Karakter Desteği:** Windows API'nin `ToUnicode` fonksiyonu sayesinde Türkçe klavye düzeni de dahil olmak üzere farklı dil düzenlerindeki karakterleri doğru bir şekilde yakalar.
*   **İki Farklı Çalışma Modu:**
    *   **Görünür Mod (Debug):** Hata ayıklama için kullanılan, yakalanan karakterleri, sayacı ve anlık durumu gösteren bir arayüze sahiptir.
    *   **Gizli Mod (Stealth):** Arayüzü olmadan, görev çubuğunda görünmeden tamamen arka planda çalışır.
*   **Hata Yönetimi:** E-posta gönderimi sırasında oluşabilecek hataları yakalar ve bir `error_log.txt` dosyasına kaydeder.

## Kullanılan Teknolojiler

*   **Dil:** C#
*   **Platform:** .NET Framework
*   **IDE:** Visual Studio
*   **Temel Kavramlar:** Windows API (P/Invoke), Global Hooking (`SetWindowsHookEx`), SMTP

## Kurulum ve Yapılandırma

1.  Projeyi Visual Studio ile açın.
2.  `Form1.cs` dosyasını açın.
3.  `SendEmail()` fonksiyonu içerisindeki aşağıdaki değişkenleri kendi bilgilerinizle güncelleyin:

    ```csharp
    string fromMail = "gonderici-mail@gmail.com";
    string fromPassword = "google-uygulama-sifreniz"; // Gmail normal şifreniz değil!
    string toMail = "alici-mail@gmail.com";
    ```
4.  **Önemli:** `fromPassword` için Google Hesabınızdan "Uygulama Şifresi" oluşturmanız gerekmektedir. Bu, 2 Adımlı Doğrulama'nın açık olmasını gerektirir.

## Çalıştırma Modları

Kodun en başında bulunan `const bool GIZLI_MOD` değişkeni ile programın çalışma modu belirlenir.

*   `GIZLI_MOD = false;` **(Görünür Mod):** Program arayüz ile birlikte normal bir şekilde çalışır. Hata ayıklama için idealdir.
*   `GIZLI_MOD = true;` **(Gizli Mod):** Program hiçbir arayüz göstermeden arka planda çalışır.

## ⚠️ Yasal ve Etik Uyarı

Bu proje **sadece eğitim amaçlı** geliştirilmiştir. Keylogger yazılımlarının başkalarının bilgisayarlarına onların izni olmadan yüklenmesi ve kullanılması yasa dışıdır ve ciddi bir gizlilik ihlalidir.

Geliştirici, yazılımın kötüye kullanılmasından doğacak hiçbir yasal sorumluluğu kabul etmez. Bu kodu kullanarak tüm sorumluluğu kabul etmiş sayılırsınız.
