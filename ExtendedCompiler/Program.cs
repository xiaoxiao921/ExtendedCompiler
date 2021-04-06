using ExtendedCompiler.Extensions;

namespace ExtendedCompiler
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            MethodOf.CecilFixup(args[0]);
        }
    }
}
