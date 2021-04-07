using ExtendedCompiler.Extensions;

namespace ExtendedCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            MethodOf.CecilFixup(args[0]);
        }
    }
}
