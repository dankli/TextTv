﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextTv.AppContext.Infrastructure.Contracts
{
    public interface ILocalSettingsProvider
    {
        object GetValue(string name);
        void SetValue(string name, object value);
    }
}
