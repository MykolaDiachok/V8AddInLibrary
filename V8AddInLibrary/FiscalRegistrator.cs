
using CentralLib.Helper;
using CentralLib.Protocols;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using V8.AddIn;


[ComVisible(true)]
[Guid("AD41135B-CA81-497D-8EB7-F56FCBB35E25")]
//[InterfaceType(ClassInterfaceType.AutoDispatch)]
public class FiscalRegistrator
{
    public FiscalRegistrator()
    {
        InstanceName = "FiscalRegistrator";
    }

    private BaseProtocol pr;


    public int ByteReserv { get; private set; }
    public int ByteResult { get; private set; }
    public int ByteStatus { get; private set; }
    public bool statusOperation { get; private set; }

    [Alias("Включена")]
    public bool IsEnabled { get; private set; }

    [Alias("ИмяЭкземпляра")]
    public string InstanceName { get; private set; }

    [Alias("Выполнить")]
    public object Make(string How, int Count)
    {

        if (IsEnabled)
        {
            InstanceName = String.Empty;
            for (int i = 0; i < Count; ++i)
                V8Context.CreateV8Context().V8Message(MessageTypes.Info, How);
        }
        return null;
    }

    private void sendMessage(string message)
    {
        V8Context.CreateV8Context().V8Message(MessageTypes.Info, message);
    }

    [Alias("ПодключитьсяКФР")]
    public bool ConnectToFR(int ComPort, string TypeProtocol)
    {
        try
        {
            if (TypeProtocol == "MG-08")
            {
                pr = new Protocol_MG08(ComPort, 0);
            }
            else if (TypeProtocol == "ЕП-11")
            {
                pr = new Protocol_EP11(ComPort, 0);
            }
            else if (TypeProtocol == "ЕП-06")
            {
                pr = new Protocol_EP06(ComPort, 0);
            }
            else // если не знаем что то пытаемся определить
            {
                pr = SingletonProtocol.Instance(ComPort, 0).GetProtocols();
            }
            ByteReserv = pr.ByteReserv;
            ByteResult = pr.ByteResult;
            ByteStatus = pr.ByteStatus;
            statusOperation = pr.statusOperation;
            IsEnabled = true;
            return pr.statusOperation;
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            IsEnabled = false;
            return false;
        }
    }

    [Alias("ПодключитьсяКФРПоСети")]
    public bool ConnectToFRNetWork(string IpAdress,int IpPort, string TypeProtocol)
    {
        try
        {
            if (TypeProtocol == "MG-08")
            {
                pr = new Protocol_MG08(IpAdress, IpPort, 0);
            }
            else if (TypeProtocol == "ЕП-11")
            {
                pr = new Protocol_EP11(IpAdress, IpPort,0);
            }
            else if (TypeProtocol == "ЕП-06")
            {
                pr = new Protocol_EP06(IpAdress, IpPort, 0);
            }
            else if (TypeProtocol == "MZ-11")
            {
                pr = new Protocol_MZ11(IpAdress, IpPort, 0);
            }
            else // если не знаем, то пытаемся определить
            {
                pr = SingletonProtocol.Instance(IpAdress, IpPort, 0).GetProtocols();
            }
            ByteReserv = pr.ByteReserv;
            ByteResult = pr.ByteResult;
            ByteStatus = pr.ByteStatus;
            statusOperation = pr.statusOperation;
            IsEnabled = true;
            return pr.statusOperation;
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            IsEnabled = false;
            return false;
        }
    }

    [Alias("ОтключитьсяОтФР")]
    public bool DisconnectToFP()
    {
        pr.Dispose();
        return true;
    }

    [Alias("ПолучитьСтатус")]
    public string ReturnStatus()
    {
        try
        {
            return JsonConvert.SerializeObject(pr.status);
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("XОтчет")]
    public string printXReport()
    {
        try
        {
            return JsonConvert.SerializeObject(pr.FPDayReport(0));
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("ZОтчет")]
    public string printZReport()
    {
        try
        {
            return JsonConvert.SerializeObject(pr.FPDayClrReport(0));
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("ПереодическийОтчет")]
    public string FPPeriodicReport(string strJSON)
    {
        try
        {
            Dictionary<string, dynamic> input = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(strJSON);
            return JsonConvert.SerializeObject(pr.FPPeriodicReport(0
                                                                    , (DateTime)input["ДатаНачала"]
                                                                    , (DateTime)input["ДатаОкончания"]));
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("ПереодическийОтчетКороткий")]
    public string FPPeriodicReportShort(string strJSON)
    {
        try
        {
            Dictionary<string, dynamic> input = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(strJSON);
            return JsonConvert.SerializeObject(pr.FPPeriodicReportShort(0
                                                                    , (DateTime)input["ДатаНачала"]
                                                                    , (DateTime)input["ДатаОкончания"]));
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("ПереодическийОтчет2")]
    public string FPPeriodicReport2(string strJSON)
    {
        try
        {
            Dictionary<string, dynamic> input = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(strJSON);
            return JsonConvert.SerializeObject(pr.FPPeriodicReport2(0
                                                                    , (ushort)input["НачальныйНомер"]
                                                                    , (ushort)input["ПоследнийНомер"]));
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("Сумма")]
    public string FPCash(int Summa)
    {
        try
        {
            if (Summa > 0)
        {
            return JsonConvert.SerializeObject(pr.FPCashIn((uint)Summa));
        }
        else
        {
            return JsonConvert.SerializeObject(pr.FPCashOut((uint)-Summa));
        }
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("СуммаСлужебногоВнесения")]
    public string FPCashIn(int Summa)
    {
        try
        {
            return JsonConvert.SerializeObject(pr.FPCashIn((uint)Summa));
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("СуммаСлужебнойВыдачи")]
    public string FPCashOut(int Summa)
    {
        try
        {
            return JsonConvert.SerializeObject(pr.FPCashOut((uint)Summa));
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("ОткрытьЯщик")]
    public string FPOpenBox(int impulse=200)
    {
        try
        {
            return JsonConvert.SerializeObject(pr.FPOpenBox((byte)impulse));
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("ПродвижениеБумаги")]
    public void FPLineFeed()
    {
        try
        {
            pr.FPLineFeed();
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            //return null;
        }
    }

    [Alias("СтатусБумаги")]
    public string FPGetPapStat()
    {
        try
        {
            return JsonConvert.SerializeObject(pr.papStat);
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("ПолучитьТекущийСтатус")]
    public string getDayReport()
    {
        try
        {
            return JsonConvert.SerializeObject(pr.dayReport);
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }


    [Alias("СуммаНаличныхВДенежномЯщике")]
    public uint? GetMoneyInBox()
    {
        try
        {
            return pr.GetMoneyInBox();
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("АннулироватьЧек")]
    public string FPResetOrder()
    {
        try
        {
            return JsonConvert.SerializeObject(pr.FPResetOrder());
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("УстановитьКассира")]
    public string FPRegisterCashier(string NameCachier)
    {
        try
        {
            return JsonConvert.SerializeObject(pr.FPRegisterCashier(0, NameCachier, 0));
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("УстановитьВверхнююНадписьНаТабло")]
    public string showTopString(string Info)
    {
        try
        {
            return JsonConvert.SerializeObject(pr.showTopString(Info));
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("УстановитьНижнююНадписьНаТабло")]
    public string showBottomString(string Info)
    {
        try
        {
            return JsonConvert.SerializeObject(pr.showBottomString(Info));
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("ПереключитьOnline")]
    public string FPCplOnline()
    {
        try
        {
            return JsonConvert.SerializeObject(pr.FPCplOnline());
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("ПолучитьСуммуПоЧеку")]
    public string FPGetCheckSums()
    {
        try
        {
            return JsonConvert.SerializeObject(pr.FPGetCheckSums());
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("ПечатьНулевойЧек")]
    public void FPNullCheck()
    {
        try
        {
            pr.FPNullCheck();
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);           
        }
    }

    [Alias("ПечататьСтрокуJSON")]
    public string FPSaleExJSON(string strJSON)
    {
        try
        {
            Dictionary<string, dynamic> input = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(strJSON);
        return
        JsonConvert.SerializeObject(pr.FPSaleEx(
                                            (ushort)input["Количество"]
                                            , (byte)input["ЧислоДесятичныхРазрядов"]
                                            , (bool)input["НеПечататьКоличество"]
                                            , (int)input["Цена"]
                                            , (ushort)input["НалоговаяГруппа"]
                                            , false
                                            , input["НазваниеТовара"]
                                            , (ulong)input["КодТовара"]
                                            , (bool)input["ПечатьШтрихКода"]));
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("ПечататьСтрокуВозвратаJSON")]
    public string FPPayMoneyExJSON(string strJSON)
    {
        try
        {
            Dictionary<string, dynamic> input = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(strJSON);
            return
            JsonConvert.SerializeObject(pr.FPPayMoneyEx(
                                                (ushort)input["Количество"]
                                                , (byte)input["ЧислоДесятичныхРазрядов"]
                                                , (bool)input["НеПечататьКоличество"]
                                                , (int)input["Цена"]
                                                , (ushort)input["НалоговаяГруппа"]
                                                , false
                                                , input["НазваниеТовара"]
                                                , (ulong)input["КодТовара"]
                                                , (bool)input["ПечатьШтрихКода"]));
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("ОплатитьJSON")]
    public string FPPaymentJSON(string strJSON)
    //byte Payment_Status, uint Payment, string CheckClose, string FiscStatus)
    {
        try
        {
            Dictionary<string, dynamic> input = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(strJSON);

        return JsonConvert.SerializeObject(pr.FPPayment((byte)input["ТипОплаты"]
                                                , (uint)input["Оплата"]
                                                , (bool)input["АвтоматическоеЗакрытие"]
                                                , (bool)input["ФискальныйЧек"]));
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("УстановитьШтрихкод")]
    public string SetBarCode(string strBarcode)
    {
        try
        {

            return JsonConvert.SerializeObject(pr.SetBarCode(strBarcode));
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }


    [Alias("ПечатьКомментарий")]
    public void FPCommentLine(string strJSON)
    {
        try
        {
            Dictionary<string, dynamic> input = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(strJSON);

        pr.FPCommentLine(input["СтрокаКомментария"], input?["ОткрытиеЧекаВыплаты"] ? input["ОткрытиеЧекаВыплаты"] : false);
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            //return null;
        }
    }

    [Alias("ПечатьСкидки")]
    public string Discount(string strJSON)
    {
        try
        {
            
            Dictionary<string, dynamic> input = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(strJSON);

            //(YourEnum)Enum.ToObject(typeof(YourEnum) , yourInt)

            return JsonConvert.SerializeObject(pr.Discount(
                                                (FPDiscount)Enum.ToObject(typeof(FPDiscount),input["ТипДисконта"])
                                                ,(Int16)input["ЗначениеДисконта"]
                                                , input["Комментарий"]
                                                ));
        }
        catch (Exception ex)
        {
            sendMessage(ex.Message);
            return null;
        }
    }

    [Alias("КопияЧека")]
    public void FPPrintCopy()
    {
        try
        {
            pr.FPPrintCopy();
        }catch(Exception ex)
        {
            sendMessage(ex.Message);
        }
    }


}
