using CentralLib.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralLib.Connections;
using CentralLib;

namespace CentralLib.Protocols
{
    public class Protocol_MZ11 : Protocol_EP11, IProtocols
    {
        public Protocol_MZ11(DefaultPortCom dComPort, int inFPNumber) : base(dComPort, inFPNumber)
        {
            
        }

        public Protocol_MZ11(int serialPort, int inFPNumber) : base(serialPort, inFPNumber)
        {
        }

        public Protocol_MZ11(string IpAdress, int port, int inFPNumber) : base(IpAdress, port, inFPNumber)
        {
        }


        public override ReturnedStruct SetBarCode(string inBarcode)
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            byte[] forsending = new byte[] { 47 };//Comment            
            byte length;
            byte[] stringBytes = byteHelper.CodingBytes(inBarcode, 16, out length);            
            forsending = byteHelper.Combine(forsending, new byte[] { length });
            forsending = byteHelper.Combine(forsending, stringBytes);
            return ExchangeWithFP(forsending);
        }
    }
}
