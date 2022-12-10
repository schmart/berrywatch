using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace berrywatch
{
    public interface ICommand
    {
        Task<int> Run();
    }
}
