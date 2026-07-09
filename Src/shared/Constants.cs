using System.Reflection;

namespace com.github.lhervier.ksp.shared
{
    public class Constants
    {
        public static string ModName
        {
            get
            {
                // The shared sources are compiled into each mod's own DLL, so the
                // executing assembly IS the mod assembly; its simple name is the
                // csproj <AssemblyName> - the single source of truth for the mod name.
                return Assembly.GetExecutingAssembly().GetName().Name;
            }
        }
    }
}