using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Wing.HospitalCompare.Model
{
    class ImportCommand
    {
        public bool ExecuteAgainstTarget { get; set; }

        public string Description { get; set; }

        public string Command { get; set; }
    }
}
