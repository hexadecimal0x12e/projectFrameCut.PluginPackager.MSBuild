using projectFrameCut.ApplicationAPIBase.DynamicPreviewProvider;
using projectFrameCut.ApplicationAPIBase.Effect;
using projectFrameCut.ApplicationAPIBase.Plugins;

namespace TestReferenceMixedPlugin
{
    // All the code in this file is included in all platforms.
    public partial class TestReferenceMixedPlugin : nobody.MyExamplePlugin, IApplicationPluginBase
    {
        public int AppLevelPluginAPIVersion => 3;

        public Dictionary<string, IClipDynamicPreviewProvider> ClipDynamicPreviewProvider => new();

        public Dictionary<string, IEffectDynamicPreviewProvider> EffectDynamicPreviewProvider => new();

        Dictionary<string, Func<IEffectBundle>> IApplicationPluginBase.EffectBundleProvider => new();

        View? IApplicationPluginBase.SettingPageProvider(ref IApplicationPluginBase instance)
        {
            return null;
        }
    }
}
