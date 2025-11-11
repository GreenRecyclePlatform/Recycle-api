using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces
{
    public interface IUnitOfWork
    {
        //add repository interfaces here


        Task SaveChangesAsync();
    }
}
