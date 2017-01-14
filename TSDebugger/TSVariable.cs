using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSDebugger
{
    class TSVariableOld
    {
        public TSVariableOld(String name, float min, float max, float cur)
        {
            Name = name;
            Minimum = min;
            Maximum = max;
            Current = cur;
        }
        
        private String _name;
        private float _minimum;
        private float _maximum;
        private float _current;

        public String Name { get { return _name; } set { _name = value; } }
        public float Minimum { get { return _minimum; } set { _minimum = value; } }
        public float Maximum { get { return _maximum; } set { _maximum = value; } }
        public float Current { get { return _current; } set { _current = value; } }
    }
}
