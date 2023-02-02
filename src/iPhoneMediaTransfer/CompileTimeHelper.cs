internal static partial class CompileTimeHelper
{
    public static long CompileTime
    {
        get
        {
            var type = typeof(CompileTimeHelper);
            var field = type.GetField("_compileTime", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            return (long)field.GetValue(null);
        }
    }
}
