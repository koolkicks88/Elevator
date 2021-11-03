using System;
using System.Collections.Generic;
using System.Text;
using Logger;

namespace Sensor
{
    public class SensorBaseClass
    {
        public SensorBaseClass()
        {
            Logger = new EventLogger();
        }

        public ILogger Logger { get; set; }
    }
}
