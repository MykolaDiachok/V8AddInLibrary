using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralLib.Protocols;

namespace TestDll
{
    class Program
    {
        static void Main(string[] args)
        {
            BaseProtocol pr = new Protocol_MZ11("172.17.20.42", 4001,0);
            var status = pr.status;
            //var daystatus = pr.dayReport;
            //pr.FPDayReport(0);
            //pr.FPDayClrReport(0);
            var stSAles = pr.FPSaleEx(10, 0, false, 11900, 1, false, "Батон горчичный нарезаный КХЗ #3", 100000000);
            var stSAles2 = pr.FPSaleEx(10, 0, false, 11900, 1, false, "Батон горчичный нарезаный КХЗ #4", 100000001);
            var info = pr.SetBarCode("D3F48623-16B3-4D52-B5F1-2516F368F896");
            //var stSAles1 = pr.FPSaleEx(2, 0, false, 11900, 1, false, "Батон горчичный нарезаный КХЗ #2", 2);
            var stPay = pr.FPPayment(3, 500000, false, true);
            //Батон горчичный нарезаный КХЗ №1
            //var checkinfo = pr.FPGetCheckSums();
            //var papstat = pr.papStat;
            //pr.FPResetOrder();
            pr.Dispose();
        }
    }
}
