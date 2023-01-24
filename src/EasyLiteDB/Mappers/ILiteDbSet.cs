using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLiteDB.Mappers
{
    public interface ILiteDbSet
    {
        string ConnectionStrings { get; internal set; }
    }
}
