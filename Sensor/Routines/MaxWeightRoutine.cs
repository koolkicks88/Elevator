using ElevatorModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sensor.Routines
{
    internal class MaxWeightRoutine
    {
        public MaxWeightRoutine()
        {
            MaxWeight = false;
        }
        public bool MaxWeight { get; set; }

        internal void CompleteInteralRequests()
        {
            MaxWeight = true;
        }

        internal List<string> FilteredList(List<string> input)
        {
            var list = new List<string>();
            if (input.Any(value => value.Contains('Q')))
            {
                list = input.OrderBy(level => level[0]).Distinct().ToList();
            }

            return list;
        }
    }
}
