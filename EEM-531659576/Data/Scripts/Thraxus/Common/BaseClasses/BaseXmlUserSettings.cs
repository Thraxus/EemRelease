
using Eem.Thraxus.Common.Utilities.FileHandlers;

namespace Eem.Thraxus.Common.BaseClasses
{
    public abstract class BaseXmlUserSettings
    {
        private readonly string _settingsFileName;
        private const string Extension = ".cfg";

        protected BaseXmlUserSettings(string modName)
        {
            _settingsFileName = modName + "_Settings" + Extension;
        }

        protected abstract void SettingsMapper();

        protected T Get<T>()
        {
            return Load.ReadXmlFileInWorldStorage<T>(_settingsFileName);
        }

        protected void Set<T>(T settings)
        {
            if (settings == null) return;
            Save.WriteXmlFileToWorldStorage(_settingsFileName, settings);
        }
    }
}