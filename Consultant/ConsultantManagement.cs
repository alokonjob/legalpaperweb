using System;
using System.Collections.Generic;
using System.Text;
using User;

namespace Consultant
{
    public class ConsultantManagement
    {
        private readonly IClienteleStaffServices clienteleStaffServices;

        public ConsultantManagement(IClienteleStaffServices clienteleStaffServices)
        {
            this.clienteleStaffServices = clienteleStaffServices;
        }


    }
}
