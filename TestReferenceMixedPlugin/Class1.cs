using projectFrameCut.ApplicationAPIBase.Effect;
using projectFrameCut.ApplicationAPIBase.Plugins;

namespace TestReferenceMixedPlugin
{
    // All the code in this file is included in all platforms.
    public partial class TestReferenceMixedPlugin : nobody.MyExamplePlugin, IApplicationPluginBase
    {
        public int AppLevelPluginAPIVersion => 3;

        Dictionary<string, Func<IEffectBundle>> IApplicationPluginBase.EffectBundleProvider => new();

        View? IApplicationPluginBase.SettingPageProvider(ref IApplicationPluginBase instance)
        {
            return null;
        }
    }
}
