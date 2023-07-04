using p4g64.alternatingOpenings.Configuration;
using p4g64.alternatingOpenings.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using System.Diagnostics;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace p4g64.alternatingOpenings;
/// <summary>
/// Your mod logic goes here.
/// </summary>
public unsafe class Mod : ModBase // <= Do not Remove.
{
    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    /// Provides access to the Reloaded.Hooks API.
    /// </summary>
    /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
    private readonly IReloadedHooks? _hooks;

    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Entry point into the mod, instance that created this class.
    /// </summary>
    private readonly IMod _owner;

    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;

    private bool* _introAPlayed;
    private IAsmHook _openingHook;

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

        Utils.Initialise(_logger, _configuration);

        Memory memory = Memory.Instance;
        _introAPlayed = (bool*)memory.Allocate(1).Address;

        Utils.LogDebug($"Allocated introAPlayed to 0x{(nuint)_introAPlayed:X}");

        if (_modLoader.GetActiveMods().Any(x => x.Generic.ModId == "p4gpc.p4openingmovie"))
        {
            *_introAPlayed = true;
            Utils.Log("P4 Opening Movie mod detected, forcing P4 Opening first.");
        }
        else if (_configuration.RandomiseInitial)
            *_introAPlayed = new Random().Next(0, 2) == 1;
        else
            *_introAPlayed = _configuration.P4OpeningFirst;

        _modLoader.ModLoading += OnModLoading;

        var startupScannerController = _modLoader.GetController<IStartupScanner>();
        if (startupScannerController == null || !startupScannerController.TryGetTarget(out var startupScanner))
        {
            Utils.LogError($"Unable to get controller for Reloaded SigScan Library, stuff won't work :(");
            return;
        }

        startupScanner.AddMainModuleScan("E8 ?? ?? ?? ?? F6 15 ?? ?? ?? ?? 48 89 43 ??", result =>
        {
            if (!result.Found)
            {
                Utils.LogError($"Unable to find PlayOpening, stuff won't work :(");
                return;
            }
            Utils.LogDebug($"Found PlayOpening at 0x{result.Offset + Utils.BaseAddress:X}");

            string[] function =
            {
                "use64",
                "mov edx, 43", // Set default to P4G (In case of P4 Opening Mod which changes this)
                $"cmp byte [qword {(nuint)_introAPlayed}], 0",
                "je endHook",
                "mov edx, 21", // Play P4 opening
                "label endHook",
                $"xor byte [qword {(nuint)_introAPlayed}], 1" // Flip introAPlayed
            };
            _openingHook = _hooks.CreateAsmHook(function, result.Offset + Utils.BaseAddress, AsmHookBehaviour.ExecuteFirst).Activate();
        });

    }

    private void OnModLoading(IModV1 mod, IModConfigV1 modConfig)
    {
        if (modConfig.ModId == "p4gpc.p4openingmovie")
        {
            *_introAPlayed = true;
            Utils.Log("P4 Opening Movie mod detected, forcing P4 Opening first.");
        }
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}