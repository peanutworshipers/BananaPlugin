namespace BananaPlugin.Features;

using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using BananaPlugin.Features.Configs;
using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The main feature responsible for cleaning the facility / server.
/// </summary>
public sealed class Cleanup : PluginFeatureConfig<CfgCleanup>
{
    static Cleanup()
    {
        PrefabHelper.RunWhenReady(AddComponents);
    }

    private Cleanup()
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the Cleanup instance.
    /// </summary>
    public static Cleanup? Instance { get; private set; }

    /// <inheritdoc/>
    public override string Name => "Cleanup";

    /// <inheritdoc/>
    public override string Prefix => "clean";

    /// <inheritdoc/>
    protected override void Enable()
    {
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
    }

    /// <inheritdoc/>
    protected override CfgCleanup RetrieveLocalConfig(Config config) => config.Cleanup;

    private static void AddComponents()
    {
        foreach (KeyValuePair<uint, GameObject> prefab in NetworkClient.prefabs)
        {
            if (!prefab.Value.GetComponent<BasicRagdoll>())
            {
                continue;
            }

            prefab.Value.AddComponent<RagdollCleanup>();
        }

        BPLogger.Debug("Cleanups initialized.");
    }

    /// <summary>
    /// The main component responsible for cleanup of ragdolls.
    /// </summary>
    public sealed class RagdollCleanup : MonoBehaviour
    {
        static RagdollCleanup()
        {
            SceneManager.sceneUnloaded += ResetQueue;
        }

        /// <summary>
        /// Gets the current ragdoll queue.
        /// </summary>
        public static Queue<RagdollCleanup> RagdollQueue { get; } = new();

        private static void ResetQueue(Scene scene)
        {
            RagdollQueue.Clear();
        }

        private static void EnsureCapacity(RagdollCleanup ragdoll)
        {
            if (ragdoll == null || !Instance)
            {
                return;
            }

            RagdollQueue.Enqueue(ragdoll);

            if (!Instance.Enabled)
            {
                return;
            }

            while (RagdollQueue.Count > Instance.LocalConfig.RagdollCleanup)
            {
                if (!RagdollQueue.TryDequeue(out RagdollCleanup queuedRagdoll))
                {
                    break;
                }

                if (queuedRagdoll == null)
                {
                    continue;
                }

                NetworkServer.Destroy(queuedRagdoll.gameObject);
            }
        }

        private void Awake()
        {
            EnsureCapacity(this);
        }
    }
}
