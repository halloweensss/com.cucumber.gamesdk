using System.Collections.Generic;
using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.RemoteConfigs
{
    public interface IRemoteConfigsApp : IServiceProvider
    {
        IReadOnlyDictionary<string, RemoteConfigValue> RemoteValues { get; }
        Task Initialize();
        Task InitializeWithUserParameters(params KeyValuePair<string, string>[] parameters);
    }
}