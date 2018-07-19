using System.Threading.Tasks;

namespace SF.Core.Infrastructure.Modules.Builder
{
    public interface IModuleConfigBuilder
    {     
        Task<ModuleConfig> BuildConfig(string filePath);
    }
}
