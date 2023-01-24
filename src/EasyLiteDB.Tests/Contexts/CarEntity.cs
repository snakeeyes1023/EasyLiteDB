using EasyLiteDB.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLiteDB.Tests.Contexts
{
    [Table("CarEntities")]
    public class CarEntity : LiteDBEntity
    {
        public string Name { get; set; }

        public string Model { get; set; }

        public string Color { get; set; }
    }
}
