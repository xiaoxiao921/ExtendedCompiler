using ExtendedCompiler.Extensions;

namespace ExtendedCompiler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MethodOf.CecilFixup(args[0]);
        }
    }
}
