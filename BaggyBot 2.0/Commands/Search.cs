using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.DataProcessors;

namespace BaggyBot.Commands
{
    class Search : ICommand
    {
        public PermissionLevel Permissions { get { return PermissionLevel.All; } }
        //private DataFunctionSet dataFunctionSet;

        public Search(DataFunctionSet df)
        {
            //dataFunctionSet = df;
        }

        public void Use(CommandArgs command)
        {
            
        }
    }
}
