using System;
using System.Text.Json;
using Cosmos.Debug.Kernel;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug (Target = typeof(JsonSerializer))]
    public static class JsonPlug
    {
        internal static Debugger mDebugger = new Debugger("System", "Json Plugs");

        public static T Deserialize<T>(string json)
        {
            T result = default(T);
            return result;
        }
    }
}
