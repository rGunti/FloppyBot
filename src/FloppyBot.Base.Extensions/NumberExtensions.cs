namespace FloppyBot.Base.Extensions;

internal enum FileSizeUnit
{
    B = 0,
    Kilo = 1,
    Mega = 2,
    Giga = 3,
    Tera = 4,
}

public static class NumberExtensions
{
    public static double Bytes(this double bytes) => bytes.AsSiSize(FileSizeUnit.B);

    public static double KiloBytes(this double kiloBytes) => kiloBytes.AsSiSize(FileSizeUnit.Kilo);

    public static double MegaBytes(this double megaBytes) => megaBytes.AsSiSize(FileSizeUnit.Mega);

    public static double GigaBytes(this double gigaBytes) => gigaBytes.AsSiSize(FileSizeUnit.Giga);

    public static double TeraBytes(this double teraBytes) => teraBytes.AsSiSize(FileSizeUnit.Tera);

    public static double Bytes(this int bytes) => bytes;

    public static double KiloBytes(this int kiloBytes) => kiloBytes.AsSiSize(FileSizeUnit.Kilo);

    public static double MegaBytes(this int megaBytes) => megaBytes.AsSiSize(FileSizeUnit.Mega);

    public static double GigaBytes(this int gigaBytes) => gigaBytes.AsSiSize(FileSizeUnit.Giga);

    public static double TeraBytes(this int teraBytes) => teraBytes.AsSiSize(FileSizeUnit.Tera);

    public static double KibiBytes(this double kibiBytes) =>
        kibiBytes.AsBinarySize(FileSizeUnit.Kilo);

    public static double MebiBytes(this double mebiBytes) =>
        mebiBytes.AsBinarySize(FileSizeUnit.Mega);

    public static double GibiBytes(this double gibiBytes) =>
        gibiBytes.AsBinarySize(FileSizeUnit.Giga);

    public static double TebiBytes(this double tebiBytes) =>
        tebiBytes.AsBinarySize(FileSizeUnit.Tera);

    private static double AsSiSize(this double size, FileSizeUnit unit) =>
        size * Math.Pow(1000, (int)unit);

    private static double AsSiSize(this int size, FileSizeUnit unit) =>
        size * Math.Pow(1000, (int)unit);

    private static double AsBinarySize(this double size, FileSizeUnit unit) =>
        size * Math.Pow(1024, (int)unit);

    private static double AsBinarySize(this int size, FileSizeUnit unit) =>
        size * Math.Pow(1024, (int)unit);
}
