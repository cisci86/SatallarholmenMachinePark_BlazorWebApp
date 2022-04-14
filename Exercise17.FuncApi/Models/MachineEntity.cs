using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise17.FuncApi.Models
{
    public class MachineEntity : TableEntity
    {
        public string  Id { get; set; }
        public string Name { get; set; }
        public bool Online { get; set; }
        public int Data { get; set; }
        public DateTime UpdatingTime { get; set; }
    }
}
