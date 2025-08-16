using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;


namespace ClassLibrary1
{
    [ServiceContract]
    public interface ServerInterface
    {
        [OperationContract]
        bool CheckUsername(string username);
    }
}
