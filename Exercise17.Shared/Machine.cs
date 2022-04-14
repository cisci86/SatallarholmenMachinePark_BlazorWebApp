using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise17.Shared
{
    public class Machine
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Online { get; set; }
        public int Data { get; set; }
        public DateTime UpdatingTime { get; set; }

        public Machine()
        {
            Id = Guid.NewGuid().ToString();
            Data = 0;
        }
        
    }
}
