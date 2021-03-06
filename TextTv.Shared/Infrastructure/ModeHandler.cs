﻿using System;
using TextTv.Shared.Infrastructure.Contracts;

namespace TextTv.Shared.Infrastructure
{
    public class ModeHandler
    {
        private readonly ILocalSettingsProvider _localSettings;
        private readonly string WEB_TEXT;
        private readonly string TV_TEXT;

        private readonly Action<string> _setTextAction;

        public ModeHandler(AppResources appResources, ILocalSettingsProvider localSettings, Action<string> setTextAction)
        {
            _localSettings = localSettings;
            WEB_TEXT = appResources.Get("ThemeWeb");
            TV_TEXT = appResources.Get("ThemeTv");
			if (setTextAction != null) {
				_setTextAction = setTextAction;
			} else {
				_setTextAction = s => {

				};
			}
            
        }

        public void Initialize()
        {
            if (_localSettings.GetValue("theme") == null)
            {
                this.CurrentTextTvMode = TextTvMode.Tv;
                this._setTextAction(WEB_TEXT);
                return;
            }

            string mode = _localSettings.GetValue("theme").ToString();
            if (string.IsNullOrWhiteSpace(mode) == false)
            {
                if (mode == TextTvMode.Tv.ToString())
                {
                    this.CurrentTextTvMode = TextTvMode.Tv;
                    this._setTextAction(WEB_TEXT);
                }
                else
                {
                    this.CurrentTextTvMode = TextTvMode.Web;
                    this._setTextAction(TV_TEXT);
                }
            }
        }

        public TextTvMode CurrentTextTvMode
        {
            get
            {
                if (_localSettings.GetValue("theme") == null)
                {
                    this._setTextAction(WEB_TEXT);
                    return TextTvMode.Tv;
                }

                string mode = _localSettings.GetValue("theme").ToString();
                if (string.IsNullOrWhiteSpace(mode) == false)
                {
                    if (mode == TextTvMode.Tv.ToString())
                    {
                        return TextTvMode.Tv;
                    }
                    else
                    {
                        return TextTvMode.Web;
                    }
                }

                return TextTvMode.Tv;
            }
            set
            {
                if (value == TextTvMode.Tv)
                {
                    this._setTextAction(WEB_TEXT);
                }
                else
                {
                    this._setTextAction(TV_TEXT);
                }

                _localSettings.SetValue("theme", value.ToString());
            }
        }

        public void Toggle()
        {
            if (this.CurrentTextTvMode == TextTvMode.Tv)
            {
                this.CurrentTextTvMode = TextTvMode.Web;
            }
            else
            {
                this.CurrentTextTvMode = TextTvMode.Tv;
            }
        }
    }
}
