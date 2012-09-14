using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Notifications_Server.Service
{
    [ServiceContract]
    public interface IRegistrationService
    {
        [OperationContract, WebGet]
        void Register(string uri);

        [OperationContract, WebGet]
        void Unregister(string uri);
    }
}
