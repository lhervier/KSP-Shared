using System.Reflection;

namespace com.github.lhervier.ksp.shared
{
    public class Constants
    {
        public static string ModName
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;
            }
        }
    }
}