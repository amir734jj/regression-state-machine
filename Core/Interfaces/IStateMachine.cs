using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IStateMachine<in T>
    {
        Task Run(Dictionary<string, object> dict, T instance);
    }
}