using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CentralLib.Connections
{
    [ComVisible(true)]
    [Guid("90E7D3F9-8876-4842-B055-66476ED74224")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IConnectFactory
    {
        
        byte ByteReserv { get; set; }
        byte ByteResult { get; set; }
        byte ByteStatus { get; set; }
        int ConsecutiveNumber { get; set; }
        string errorInfo { get; set; }
        byte[] glbytesForSend { get; set; }
        byte[] glbytesPrepare { get; set; }
        bool statusOperation { get; set; }

        byte[] dataExchange(byte[] input, bool repeatError = false);
        byte[] dataExchange(byte[] input, bool useCRC16 = false, bool repeatError = false);
        byte[] dataExchange(byte[] input);

        bool IsOpen { get; }
        void Open();
        void Close();

    }
}
