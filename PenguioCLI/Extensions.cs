using System.Linq;

namespace PenguioCLI
{
    public static class Extensions
    {
        public static string RemoveLastSplit(this string s, char sp)
        {
            return s.Replace(sp + s.Split(sp).Last(), "");
        }

        public static string ExeDirectory()
        {
            return System.Windows.Forms.Application.ExecutablePath.RemoveLastSplit('\\');
        }
    }
}