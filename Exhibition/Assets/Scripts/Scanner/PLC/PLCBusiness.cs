using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unattended
{
    public class PLCBusiness
    {
        public bool IsAuthorization()
        {
            var ret = false;
            try
            {
                //授权检查
                if (HslCommunication.Authorization.SetAuthorizationCode("a4908a2c-4fb6-4a14-a418-d183c5a6e448"))
                {
                    ret = true;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return ret;
        }
    }
}
