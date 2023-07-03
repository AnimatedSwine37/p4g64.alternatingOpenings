using p4g64.alternatingOpenings.Template.Configuration;
using System.ComponentModel;

namespace p4g64.alternatingOpenings.Configuration;
public class Config : Configurable<Config>
{
    [DisplayName("Randomise Initial Opening")]
    [Description("If enabled, the first displayed opening will be randomly chosen between the P4G and P4 ones.")]
    [DefaultValue(false)]
    public bool RandomiseInitial { get; set; } = false;

    [DisplayName("P4 Initial Opening")]
    [Description("If enabled, the first displayed opening will always be the P4 Opening (Randomise Initial Opening takes priority over this).")]
    [DefaultValue(false)]
    public bool P4OpeningFirst { get; set; } = false;

    [DisplayName("Debug Mode")]
    [Description("Logs additional information to the console that is useful for debugging.")]
    [DefaultValue(false)]
    public bool DebugEnabled { get; set; } = false;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}
