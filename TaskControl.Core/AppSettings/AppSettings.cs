using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.Core.AppSettings
{
    //TODO: Сделать класс настроек приложения
    public class AppSettings
    {
        public const string SectionName = "AppSettings";
        public bool EnableDetailedLogging { get; set; } = false;
    }
}
