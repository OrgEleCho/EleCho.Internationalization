using System;
using System.Globalization;
using System.Windows;

namespace EleCho.Internationalization.Wpf
{
    public class I18nStringResources : ResourceDictionary
    {
        private II18nStrings? _i18nStrings;
        private string? _keyPrefix;

        public string? KeyPrefix
        {
            get => _keyPrefix;
            set
            {
                _keyPrefix = value;
                PopulateResources();
            }
        }

        public II18nStrings? I18nStrings
        {
            get => _i18nStrings;
            set
            {
                if (_i18nStrings is not null)
                {
                    _i18nStrings.CurrentCultureChanged -= I18nStrings_CurrentCultureChanged;
                }

                _i18nStrings = value;
                PopulateResources();

                if (value is not null)
                {
                    value.CurrentCultureChanged += I18nStrings_CurrentCultureChanged;
                }
            }
        }

        public I18nStringResources()
        {
            _keyPrefix = "I18nString";
        }

        public I18nStringResources(II18nStrings i18nStrings) : this()
        {
            I18nStrings = i18nStrings;
        }

        ~I18nStringResources()
        {
            if (_i18nStrings is not null)
            {
                _i18nStrings.CurrentCultureChanged -= I18nStrings_CurrentCultureChanged;
            }
        }

        private void I18nStrings_CurrentCultureChanged(object sender, EventArgs e)
        {
            PopulateResources();
        }

        private void PopulateResources()
        {
            Clear();

            if (I18nStrings is not null)
            {
                foreach (var name in I18nStrings.AllStringNames)
                {
                    var key = name;
                    if (_keyPrefix is not null)
                    {
                        key = $"{_keyPrefix}{name}";
                    }

                    this[key] = I18nStrings.GetString(name);
                }
            }
        }
    }
}
