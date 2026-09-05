using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.Core;

namespace Holstin.Core
{
    /// <summary>
    /// Central entry point for Unity Gaming Services used by the runtime client.
    /// Keeps initialization and authentication out of gameplay systems and prevents
    /// duplicate service initialization when multiple systems need Cloud Code.
    /// </summary>
    public static class UnityCloudServices
    {
        private static readonly SemaphoreSlim InitializationGate = new(1, 1);

        public static bool IsReady =>
            UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn;

        /// <summary>
        /// Initializes Unity Gaming Services and signs in with an anonymous player account.
        /// Anonymous authentication is ideal for development and prototyping; replace or link
        /// it with a durable identity provider before relying on cross-device progression.
        /// </summary>
        public static async Task InitializeAsync()
        {
            if (IsReady)
            {
                return;
            }

            await InitializationGate.WaitAsync();
            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    await UnityServices.InitializeAsync();
                }

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
            }
            finally
            {
                InitializationGate.Release();
            }
        }

        /// <summary>
        /// Calls a deployed Cloud Code C# module endpoint after ensuring UGS is initialized.
        /// </summary>
        public static async Task<TResult> CallModuleAsync<TResult>(
            string moduleName,
            string functionName,
            Dictionary<string, object> arguments = null)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                throw new ArgumentException("Cloud Code module name is required.", nameof(moduleName));
            }

            if (string.IsNullOrWhiteSpace(functionName))
            {
                throw new ArgumentException("Cloud Code function name is required.", nameof(functionName));
            }

            await InitializeAsync();
            return await CloudCodeService.Instance.CallModuleEndpointAsync<TResult>(
                moduleName,
                functionName,
                arguments);
        }
    }
}
