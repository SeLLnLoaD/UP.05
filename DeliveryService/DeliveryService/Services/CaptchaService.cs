using System.Drawing.Drawing2D;

namespace DeliveryService.Services;

public static class CaptchaService
{
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private static string _current = "";
    public static int Failures { get; private set; } = 0;

    public static Bitmap Generate(int width = 180, int height = 50)
    {
        var rnd = new Random();
        _current = new string(Enumerable.Range(0, 5)
            .Select(_ => Chars[rnd.Next(Chars.Length)]).ToArray());

        var bmp = new Bitmap(width, height);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.WhiteSmoke);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var font = new Font("Segoe UI", 22, FontStyle.Bold | FontStyle.Italic);
        var brush = new SolidBrush(Color.FromArgb(40, 40, 120));

        for (int i = 0; i < _current.Length; i++)
        {
            var state = g.Save();
            var angle = rnd.Next(-20, 20);
            g.TranslateTransform(20 + i * 28, height / 2);
            g.RotateTransform(angle);
            g.DrawString(_current[i].ToString(), font, brush, -10, -18);
            g.Restore(state);
        }

        for (int i = 0; i < 6; i++)
        {
            var pen = new Pen(Color.FromArgb(120, rnd.Next(255), rnd.Next(255), rnd.Next(255)), 1.5f);
            g.DrawLine(pen, rnd.Next(width), rnd.Next(height), rnd.Next(width), rnd.Next(height));
        }
        return bmp;
    }

    public static bool Verify(string input) =>
        string.Equals(input?.Trim(), _current, StringComparison.OrdinalIgnoreCase);

    public static bool CaptchaRequired => Failures >= 2;
    public static void RegisterFailure() => Failures++;
    public static void Reset() => Failures = 0;
}