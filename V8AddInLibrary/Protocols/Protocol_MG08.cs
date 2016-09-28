using CentralLib.Connections;
using CentralLib.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CentralLib.Protocols
{
    [Guid("2A81292C-329D-44CA-A715-5DD38DC82B23"), ComVisible(true)]
    public class Protocol_MG08 : BaseProtocol, IProtocols
    {

        private Status tStatus;
        public override Taxes currentTaxes
        {
            get; set;
        }

        public override Status status
        {
            get
            {
                if ((lastByteCommand != 0))
                    getStatus();
                return this.tStatus;
            }
        }


        /// <summary>
        /// Передаем класс для подключения. Рекомендую использовать сразу код порта
        /// </summary>
        /// <param name="connFP"></param>
        public Protocol_MG08(CentralLib.Connections.DefaultPortCom dComPort, int inFPNumber):base(dComPort, inFPNumber)
        {
            useCRC16 = true;
            connFP = new CentralLib.Connections.ConnectFP_EP11(dComPort);
            MaxStringLenght = 75;
            initial();
        }

        /// <summary>
        /// Класс инициализации приложения
        /// </summary>
        /// <param name="serialPort"></param>
        public Protocol_MG08(int serialPort, int inFPNumber):base(serialPort, inFPNumber)
        {
            useCRC16 = true;
            MaxStringLenght = 75;
            initial();

        }

        public Protocol_MG08(string IpAdress, int port, int inFPNumber) :base(IpAdress,port, inFPNumber)
        {
            useCRC16 = true;
            connFP = new ConnectNetFP_EP11(IpAdress, port, inFPNumber);
            MaxStringLenght = 75;
            initial();

        }

        private void initial()
        {
            getStatus();
            if (!(bool)structStatus.ExceedingOfWorkingShiftDuration)
                FPGetTaxRate();
        }

        public override ReturnedStruct getStatus()
        {
            byte[] forsending = new byte[] { 0 };
            ReturnedStruct forReturn = ExchangeWithFP(forsending);
            byte[] answer = forReturn.bytesReturn;

            if ((statusOperation) && (answer.Length > 21))
            {
                string hexday = answer[21].ToString("X");
                int _day = Math.Min(Math.Max((int)Convert.ToInt16(hexday), 1), 31);

                string hexmonth = answer[22].ToString("X");
                int _month = Math.Min(Math.Max((int)Convert.ToInt16(hexmonth), 1), 12);

                string hexyear = answer[23].ToString("X");
                int _year = Convert.ToInt16(hexyear);

                string hexhour = answer[24].ToString("X");
                int _hour = Math.Min(Math.Max((int)Convert.ToInt16(hexhour), 0), 23);

                string hexmin = answer[25].ToString("X");
                int _min = Math.Min(Math.Max((int)Convert.ToInt16(hexmin), 0), 59);

                int curCountByte = 26;
                string fiscalNumber = new ByteHelper().EncodingBytes(answer, curCountByte, 10);
                curCountByte = curCountByte + 10;


                byte tlen1 = answer[curCountByte];
                tlen1 = byteHelper.SetBit(tlen1, 6, false);
                tlen1 = byteHelper.SetBit(tlen1, 7, false);
                int len1 = tlen1;

                string str1 = "";
                if (len1 > 0)
                {
                    curCountByte++;
                    str1 = byteHelper.EncodingBytes(answer, curCountByte, len1);

                }
                curCountByte = curCountByte + len1;

                byte tlen2 = answer[curCountByte];
                tlen2 = byteHelper.SetBit(tlen2, 6, false);
                tlen2 = byteHelper.SetBit(tlen2, 7, false);
                int len2 = tlen2;
                string str2 = "";
                if (len2 > 0)
                {
                    curCountByte++;
                    str2 = byteHelper.EncodingBytes(answer, curCountByte, len2);

                }
                curCountByte = curCountByte + len2;

                byte tlen3 = answer[curCountByte];
                tlen3 = byteHelper.SetBit(tlen3, 6, false);
                tlen3 = byteHelper.SetBit(tlen3, 7, false);
                int len3 = tlen3;
                string str3 = "";
                if (len3 > 0)
                {
                    curCountByte++;
                    str3 = byteHelper.EncodingBytes(answer, curCountByte, len3);

                }
                curCountByte = curCountByte + len3;

                byte tlenTax = answer[curCountByte];
                tlenTax = byteHelper.SetBit(tlenTax, 6, false);
                tlenTax = byteHelper.SetBit(tlenTax, 7, false);
                int lenTax = tlenTax;
                string strTax = "";
                if (lenTax > 0)
                {
                    curCountByte++;
                    strTax = byteHelper.EncodingBytes(answer, curCountByte, lenTax);

                }
                curCountByte = curCountByte + len3;



                byte[] verBytes = new byte[5];
                System.Buffer.BlockCopy(answer, answer.Length - 5, verBytes, 0, 5);
                string ver = byteHelper.EncodingBytes(verBytes);


                this.tStatus = new Status(answer.Take(2).ToArray()
                    , byteHelper.EncodingBytes(answer.Skip(2).Take(19).ToArray())
                    , new DateTime(2000 + _year, _month, _day, _hour, _min, 0)
                    , fiscalNumber
                    , str1
                    , str2
                    , str3
                    , strTax
                    , ver
                    , connFP.ConsecutiveNumber
                    , forReturn
                    );
            }
            else
            {
                this.statusOperation = false;
            }
            return forReturn;
        }

        private PapStat tpapStat;

        public override PapStat papStat
        {
            get
            {
                if ((lastByteCommand != 48))
                    getGetPapStat();
                return tpapStat;
            }
        }

        /// <summary>
        /// Код: 48.GetPapStat прочитать состояние бумаги в принтере
        /// </summary>
        private void getGetPapStat()
        {
            byte[] forsending = new byte[] { 48 };
            var structreturn = ExchangeWithFP(forsending);
            byte[] answer = structreturn.bytesReturn;
            if ((connFP.statusOperation) && (answer.Length == 1))
            {
                this.tpapStat = new PapStat(answer[0], structreturn);
            }
            else
            {
                this.statusOperation = false;
            }
        }

        private DayReport tDayReport;
        public override DayReport dayReport
        {
            get
            {
                //if ((lastByteCommand != 42) && (dayReport != null))
                {
                    tDayReport = getDayReport();
                }
                return tDayReport;
            }
        }

        private DayReport getDayReport()
        {
            byte[] bytesReturn, bytesReturn0, bytesReturn1, bytesReturn2, bytesReturn3;

            {
                byte[] forsending = new byte[] { 42, 0 };
                var answer = ExchangeWithFP(forsending);
                if ((answer.statusOperation) && (answer.bytesReturn.Length > 0))
                {
                    bytesReturn0 = answer.bytesReturn;
                }
                else
                {
                    this.statusOperation = false;
                    return new DayReport();
                }
            }
            {
                byte[] forsending = new byte[] { 42, 1 };
                var answer = ExchangeWithFP(forsending);
                if ((answer.statusOperation) && (answer.bytesReturn.Length > 0))
                {
                    bytesReturn1 = answer.bytesReturn;
                }
                else
                {
                    this.statusOperation = false;
                    return new DayReport();
                }
            }
            {
                byte[] forsending = new byte[] { 42, 2 };
                var answer = ExchangeWithFP(forsending);
                if ((answer.statusOperation) && (answer.bytesReturn.Length > 0))
                {
                    bytesReturn2 = answer.bytesReturn;
                }
                else
                {
                    this.statusOperation = false;
                    return new DayReport();
                }
            }
            //{
            //    byte[] forsending = new byte[] { 42, 3 };
            //    var answer = ExchangeWithFP(forsending);
            //    if ((answer.statusOperation) && (answer.bytesReturn.Length > 0))
            //    {
            //        bytesReturn3 = answer.bytesReturn;
            //    }
            //    else
            //    {
            //        this.statusOperation = false;
            //        return new DayReport();
            //    }
            //}

            ReturnedStruct answer42;
            {
                byte[] forsending = new byte[] { 42 };
                answer42 = ExchangeWithFP(forsending);
                if ((answer42.statusOperation) && (answer42.bytesReturn.Length > 0))
                {
                    bytesReturn = answer42.bytesReturn;
                }
                else
                {
                    this.statusOperation = false;
                    return new DayReport();
                }
            }

            return new DayReport(this, answer42, bytesReturn, bytesReturn0, bytesReturn1, bytesReturn2, null);
        }

        /// <summary>
        /// Код: 32.       PrintVer печать налогового номера и версии программного обеспечения
        /// Налоговый номер и дата регистрации ЭККР печатаются только в фискальном режиме.
        /// </summary>
        public override ReturnedStruct FPPrintVer()
        {
            byte[] forsending = new byte[] { 32 };
            return ExchangeWithFP(forsending);
        }


        /// <summary>
        /// Код: 29. OpenBox открытие денежного ящика
        ///
        /// При отсутствии параметра на денежный ящик подается импульс 200мс.
        /// </summary>
        /// <param name="impulse"> длительность импульса открытия в 2мс </param>
        public override ReturnedStruct FPOpenBox(byte impulse = 0) //обнуление чека
        {
            byte[] forsending = new byte[] { 29 };
            if (impulse != 0)
                forsending = new byte[] { 29, impulse };
            return ExchangeWithFP(forsending);
        }

        /// <summary>
        ///  Код: 36. запрет/разрешение режима OnLine регистраций
        ///  В режиме OnLine регистрация продажи, выплаты, оплаты, комментариев, скидок\наценок
        ///  сопровождается печатью в чеке.Команда запрещена при открытом чеке. Вызов команды меняет
        ///  значение параметра на противоположный.
        /// </summary>
        public override ReturnedStruct FPCplOnline() // Код: 36. запрет/разрешение режима OnLine регистраций
        {
            byte[] forsending = new byte[] { 36 };
            return ExchangeWithFP(forsending);
        }

        #region Cutter
        /// <summary>
        /// Установка обрезчика, в начале запрашиваем данные о состоянии, после включаем
        /// </summary>
        /// <param name="Enable">Если true то включаем, если false то нет</param>
        public override bool setFPCplCutter(bool Enable)
        {
            if ((Enable) && (status.paperCuttingForbidden))
                FPCplCutter();
            if ((!Enable) && (!status.paperCuttingForbidden))
                FPCplCutter();
            return statusOperation;
        }

        #endregion

        #region установка кассира

        /// <summary>
        ///  Код: 5.SetCod                   установка пароля
        /// </summary>
        /// <param name="UserID">номер (0-7 – пароли кассиров, 8 – пароль режима программирования, 9 – пароль режима отчетов)</param>
        /// <param name="OldPassword"> старый пароль</param>
        /// <param name="NewPassword">новый пароль</param>
        public override ReturnedStruct FPSetPassword(byte UserID, ushort OldPassword, ushort NewPassword)
        {
            byte[] forsending = new byte[] { 5 };//SetCod
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(OldPassword));
            forsending = byteHelper.Combine(forsending, new byte[] { UserID });
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(NewPassword));
            return ExchangeWithFP(forsending);
        }


        #endregion


        #region Чеки


        public override SumTaxGroupsAndTypesOfPayments FPGetCheckSums()
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            byte[] forsending = new byte[] { 43 };//Comment            
            var retSt = ExchangeWithFP(forsending);
            byte[] answer = retSt.bytesReturn;

            return new SumTaxGroupsAndTypesOfPayments(answer,0,4);
        }

        /// <summary>
        /// Печать нулевого чека
        /// </summary>
        /// <returns></returns>
        public override uint FPPrintZeroReceipt()
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            byte[] forsending = new byte[] { 13 };//Comment            
            byte length;
            byte[] stringBytes = byteHelper.CodingBytes("Нульовий чек", 27, out length);
            length = byteHelper.SetBit(length, 7, false);
            forsending = byteHelper.Combine(forsending, new byte[] { length });
            forsending = byteHelper.Combine(forsending, stringBytes);
            var retSt = ExchangeWithFP(forsending);
            byte[] answer = retSt.bytesReturn;
            if (statusOperation)
            {
                forsending = new byte[] { 18, 0x03 };//Payment 
                forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(0 ^ (1 << 31)));
                answer = ExchangeWithFP(forsending).bytesReturn;
                if (answer.Length == 4)
                    return BitConverter.ToUInt32(answer, 0);
            }
            return 0;
            //return base.FPPrintZeroReceipt();
        }


        /// <summary>
        /// Код: 11. PayMoney                  регистрация выплаты 
        /// Команда  запрещена,  если  не  зарегистрированы  налоговые  ставки.  Рассчитанная  стоимость  не  
        /// должна превышать  999.999,99  грн.Сумма по  чеку не  должна превышать  21.474.836,47  грн.При
        /// отрицательной цене(для скидки, отказа от  предыдущей регистрации  и пр.)  стоимость не  должна
        /// превышать промежуточную сумму  по предыдущим  выплатам.После закрытия  чека в  параметрах
        /// артикулов соответствующих кодов   меняются значения   статусов на   больший(с увеличением
        /// разрядности меньшего),     увеличивается его    количество и   стоимость,      если артикулы
        /// запрограммированы,  или полностью  заносится описание  артикула,  если не  запрограммированы.
        /// ЭККР запрещает  изменение налоговой  группы,  название выплаты, а  в пределах  чека,  и цены.        
        /// Группа Е – непрограммируемая необлагаемая группа.
        /// </summary>
        /// <param name="Amount">количество или вес</param>
        /// <param name="Amount_Status">число десятичных разрядов в количестве</param>
        /// <param name="IsOneQuant">количество 1 не печатается в чеке)</param>
        /// <param name="Price">цена в коп (бит 31 = 1 – отрицательная цена)</param>
        /// <param name="NalogGroup">налоговая группа</param>
        /// <param name="MemoryGoodName">n=255 – название взять из памяти</param>
        /// <param name="GoodName">название товара или услуги (для n # 255)</param>
        /// <param name="StrCode">код товара</param>
        /// <param name="PrintingOfBarCodesOfGoods">=1 – печать штрих-кода товара (EAN13)</param>
        /// <returns></returns>
        public override ReceiptInfo FPPayMoneyEx(UInt16 Amount, byte Amount_Status, bool IsOneQuant, Int32 Price, ushort NalogGroup, bool MemoryGoodName, string GoodName, UInt64 StrCode, bool PrintingOfBarCodesOfGoods = false)
        {
            byte[] forsending = new byte[] { 11 };

            forsending = byteHelper.Combine(forsending, byteHelper.ConvertUint32ToArrayByte3(Amount));
            Amount_Status = byteHelper.SetBit(Amount_Status, 7, IsOneQuant);
            Amount_Status = byteHelper.SetBit(Amount_Status, 6, PrintingOfBarCodesOfGoods);
            forsending = byteHelper.Combine(forsending, new byte[] { Amount_Status });
            Int32 _price = Price;
            //BitArray b_price = new BitArray(BitConverter.GetBytes(_price));
            if (Price < 0)
            {
                _price = -_price;
                _price = _price ^ (1 << 31);
            }
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(_price));
            byte[] VAT = new byte[] { 0x80 }; ;
            if (NalogGroup == 1)
                VAT = new byte[] { 0x81 };
            else if (NalogGroup == 2)
                VAT = new byte[] { 0x82 };
            else if (NalogGroup == 3)
                VAT = new byte[] { 0x83 };
            else if (NalogGroup == 4)
                VAT = new byte[] { 0x84 };
            else if (NalogGroup == 5)
                VAT = new byte[] { 0x85 };
            forsending = byteHelper.Combine(forsending, VAT);

            if (MemoryGoodName)
                forsending = byteHelper.Combine(forsending, new byte[] { 255 });
            else
            {
                forsending = byteHelper.Combine(forsending, byteHelper.CodingStringToBytesWithLength(GoodName, MaxStringLenght));
            }
            forsending = byteHelper.Combine(forsending, byteHelper.ConvertUint64ToArrayByte6(StrCode));
            var tanswer = ExchangeWithFP(forsending);


            byte[] answer = tanswer.bytesReturn;
            if ((statusOperation) && (answer.Length == 8))
            {
                ReceiptInfo _checkinfo = new ReceiptInfo(tanswer, BitConverter.ToInt32(answer, 0), BitConverter.ToInt32(answer, 4));
                return _checkinfo;
            }
            return new ReceiptInfo();
        }


        /// <summary>
        /// Код: 20.Sale                      регистрация продажи товара или услуги
        /// Команда  запрещена,  если  не  зарегистрированы  налоговые  ставки.  Рассчитанная  стоимость  не  
        /// должна превышать  999.999,99  грн.Сумма по  чеку не  должна превышать  21.474.836,47  грн.При
        /// отрицательной цене(для скидки, отказа от  предыдущей регистрации  и пр.)  стоимость не  должна
        /// превышать промежуточную сумму  по предыдущим  продажам.После закрытия  чека в  параметрах
        /// артикулов соответствующих кодов   меняется статус  на больший(с увеличением  разрядности
        /// меньшего),  увеличивается его  количество и  стоимость,  если артикулы  запрограммированы,  или
        /// полностью заносится описание  артикула, если не запрограммированы.ЭККР запрещает изменение
        /// налоговой группы, имени  товара,  а в  пределах чека, и  цены.Группа Е  –  непрограммируемая
        /// необлагаемая группа.
        /// </summary>
        /// <param name="Amount">количество или вес </param>
        /// <param name="Amount_Status">число десятичных разрядов в количестве,</param>
        /// <param name="IsOneQuant">количество 1 не печатается в чеке)</param>
        /// <param name="Price">цена в коп (бит 31 = 1 – отрицательная цена)</param>
        /// <param name="NalogGroup">налоговая группа</param>
        /// <param name="MemoryGoodName"></param>
        /// <param name="GoodName">название товара или услуги (для n # 255) </param>
        /// <param name="StrCode">код товара</param>
        /// <param name="PrintingOfBarCodesOfGoods">печать штрих-кода товара (EAN13)</param>
        public override ReceiptInfo FPSaleEx(UInt16 Amount, byte Amount_Status, bool IsOneQuant, Int32 Price, ushort NalogGroup, bool MemoryGoodName, string GoodName, UInt64 StrCode, bool PrintingOfBarCodesOfGoods = false)
        {
            byte[] forsending = new byte[] { 20 };

            forsending = byteHelper.Combine(forsending, byteHelper.ConvertUint32ToArrayByte3(Amount));
            Amount_Status = byteHelper.SetBit(Amount_Status, 7, IsOneQuant);
            Amount_Status = byteHelper.SetBit(Amount_Status, 6, PrintingOfBarCodesOfGoods);
            forsending = byteHelper.Combine(forsending, new byte[] { Amount_Status });
            Int32 _price = Price;
            //BitArray b_price = new BitArray(BitConverter.GetBytes(_price));
            if (Price < 0)
            {
                _price = -_price;
                _price = _price ^ (1 << 31);
            }
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(_price));
            byte[] VAT = new byte[] { 0x80 }; ;
            if (NalogGroup == 1)
                VAT = new byte[] { 0x81 };
            else if (NalogGroup == 2)
                VAT = new byte[] { 0x82 };
            else if (NalogGroup == 3)
                VAT = new byte[] { 0x83 };
            else if (NalogGroup == 4)
                VAT = new byte[] { 0x84 };
            else if (NalogGroup == 5)
                VAT = new byte[] { 0x85 };
            forsending = byteHelper.Combine(forsending, VAT);

            if (MemoryGoodName)
                forsending = byteHelper.Combine(forsending, new byte[] { 255 });
            else
            {
                forsending = byteHelper.Combine(forsending, byteHelper.CodingStringToBytesWithLength(GoodName, MaxStringLenght));
            }
            forsending = byteHelper.Combine(forsending, byteHelper.ConvertUint64ToArrayByte6(StrCode));

            var tanswer = ExchangeWithFP(forsending);
            byte[] answer = tanswer.bytesReturn;
            if ((statusOperation) && (answer.Length == 8))
            {
                ReceiptInfo _checkinfo = new ReceiptInfo(tanswer, BitConverter.ToInt32(answer, 0), BitConverter.ToInt32(answer, 4));
                return _checkinfo;
            }
            return new ReceiptInfo();
        }

        /// <summary>
        /// Код: 18.Payment          регистрация оплаты и печать чека, если сума оплат не меньше
        /// Команда  запрещена при закрытом чеке. Чек закрывается автоматически и печатается, если  
        /// сумма оплат больше или равна сумме продаж или выплат, или установлен бит 31 в сумме оплат.В
        /// последнем случае сумма данной оплаты вычисляется ЭККР.Если сумма наличными больше суммы
        /// продаж, то будет  печататься сумма  сдачи.Оплата со  сдачей разрешена  только для  наличных.В
        /// чеке выплат оплата    наличными должна     быть не  более суммы     в денежном     ящике.Для
        /// нефискального чека  (обороты чека  не сохраняются  в дневных  счетчиках и  счетчиках артикулов)        
        /// рекомендуется  открывать чек  продаж.Нулевая оплата  не печатается  в чеках.Номер  пакета
        /// возвращается в случае закрытия чека.
        /// </summary>
        /// <param name="Payment_Status">статус (биты 0..3 - тип оплаты (см. команду 50);</param>
        /// <param name="Payment">оплата в коп. </param>
        /// <param name="CheckClose">автоматическое закрытие</param>
        /// <param name="FiscStatus">= 1 – закрытие чека как нефискальный</param>
        /// <param name="AuthorizationCode">код авторизации при оплате картой через платёжный терминал</param>
        /// <returns>остаток или сдача (бит 31 = 1 – сдача), номер пакета чека в КЛЕФ</returns>
        public override PaymentInfo FPPayment(byte Payment_Status, UInt32 Payment, bool CheckClose, bool FiscStatus, string AuthorizationCode = "")
        {
            byte[] forsending = new byte[] { 18 };
            
            Payment_Status = byteHelper.SetBit(Payment_Status, 6, !FiscStatus);
            forsending = byteHelper.Combine(forsending, new byte[] { Payment_Status });
            byte[] bytePayment = BitConverter.GetBytes(byteHelper.WriteBitUInt32(Payment, 31, CheckClose));
            //byte[] bytePayment = BitConverter.GetBytes(Payment);
            //if (CheckClose)
            //{
            //    bytePayment[3] = SetBit(bytePayment[3], 7, CheckClose);
            //}
            ////forsending = Combine(forsending, bytePayment);
            //int _Payment = (int)Payment;
            //if (CheckClose)
            //    //b_Payment[31] = true;
            //    _Payment = _Payment ^ (1 << 31);
            forsending = byteHelper.Combine(forsending, bytePayment);
            forsending = byteHelper.Combine(forsending, new byte[] { 0 });
            if (AuthorizationCode.Length != 0)
                forsending = byteHelper.Combine(forsending, byteHelper.CodingStringToBytesWithLength(AuthorizationCode, 50));


            var sanswer = ExchangeWithFP(forsending);

            //PaymentInfo _paymentInfo = 
            return new PaymentInfo(sanswer);
            //byte[] answer = _paymentInfo.returnedStruct.bytesReturn;
            //if ((statusOperation) && (answer.Length  > 3))
            //{

            //    UInt32 tinfo = BitConverter.ToUInt32(answer, 0);
            //    if (byteHelper.GetBit(answer[3],7))
            //    {
            //        tinfo = byteHelper.ClearBitUInt32(tinfo, 31);
            //        _paymentInfo.Renting = tinfo;
            //    }
            //    else
            //        _paymentInfo.Rest = tinfo;
            //    if (answer.Length>=8)
            //        _paymentInfo.NumberOfReceiptPackageInCPEF = BitConverter.ToUInt32(answer, 4);
            //    return _paymentInfo;
            //}
            //return new PaymentInfo();
        }

        /// <summary>
        /// Код: 13. Comment                  регистрация комментария в фискальном чеке
        /// Если  бит  7  длины  строки  равен  единице  (1)  при  первой  регистрации  в  чеке,  то  открывается  чек                                                      
        ///  выплат, иначе будет открыт чек продаж.В остальных случаях бит 7 не устанавливать!  Открыв
        /// чек комментарием(например, строкой   “НУЛЕВОЙ ЧЕК”)   и закрыв   его командой   20, можно
        /// напечатать нулевой чек.
        /// </summary>
        /// <param name="CommentLine">Строка комментария</param>
        /// <param name="OpenRefundReceipt">= 1 – открытие чека выплаты</param>
        public override ReturnedStruct FPCommentLine(string CommentLine, bool OpenRefundReceipt = false)
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            byte[] forsending = new byte[] { 13 };//Comment            
            byte length;
            byte[] stringBytes = byteHelper.CodingBytes(CommentLine, 27, out length);
            length = byteHelper.SetBit(length, 7, OpenRefundReceipt);
            forsending = byteHelper.Combine(forsending, new byte[] { length });
            forsending = byteHelper.Combine(forsending, stringBytes);
            return ExchangeWithFP(forsending);
        }

        /// <summary>
        ///  Discount                   регистрация скидки или наценки  
        ///  Код: 49.  
        /// </summary>
        /// <param name="typeDiscount"> 0 -  процентная скидка/наценка на последний товар;   
        ///           1 – абсолютная скидка/наценка на последний товар;   
        ///           2 - процентная скидка/наценка на промежуточную сумму;  
        ///            3 – абсолютная скидка/наценка на промежуточную сумму</param>
        /// <param name="value">% или сумма скидки/наценки, если процент то 9999(где 99.99% является макс значением)</param>
        /// <param name="comment">пояснительная строка</param>
        /// <returns></returns>
        public override DiscountInfo Discount(FPDiscount typeDiscount, short value, string comment)
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            //TODO test discount
            //проблему решить с комментариями!!!!!


            uint newval = 0;
            byte[] byteval;
            if (value > 0) // тогда скидка, если -"наценка"
            {
                newval = (uint)(value ^ (1 << 31));
            }
            else
            {
                newval = (uint)-value;
            }
            if ((typeDiscount == FPDiscount.PercentageDiscountMarkupAtIntermediateSum) || (typeDiscount == FPDiscount.PercentageDiscountMarkupAtLastGoods))
            {
                UInt16 tval = (UInt16)newval;
                byteval = BitConverter.GetBytes(tval);
                byteval = byteHelper.Combine(byteval, new byte[] { 0, 2 + 2 }); // 0 так как unint16 = 2 байтам, а надо 3.
            }
            else
            {
                byteval = BitConverter.GetBytes(newval);
            }
            byte[] forsending = new byte[] { 49, (byte)typeDiscount };
            forsending = byteHelper.Combine(forsending, byteval);
            forsending = byteHelper.Combine(forsending, byteHelper.CodingStringToBytesWithLength(comment, 25));
            return new DiscountInfo(ExchangeWithFP(forsending));
        }


        #endregion


        #region глобальные установки
        /// <summary>
        /// SetHeadLine реистрация атрибутов налогоплательщика Код: 22. 
        /// * - длина строки при печати двойной ширины символов. Если перед регистрацией атрибутов была регистрация фискального номера в нефискальном режиме, то произойдет фискализация с записью в фискальную память фискального номера, атрибутов и печатью чека фискализации. В этом случае предыдущая регистрация налоговых ставок сбрасывается. Налоговый номер нужно вводить без преамбулы „ПН” или “ІД”.
        /// </summary>
        /// <param name="Password">пароль программирования 0 - по умолчанию</param>
        /// <param name="StringInfo1">Строка 1 атрибутов налогоплательщика, макс 30 символов (20 если двойная ширина)</param>
        /// <param name="StringInfo1DoubleHeight"> печать двойной ширины</param>
        /// <param name="StringInfo1DoubleWidth"> печать двойной высоты</param>
        /// <param name="StringInfo2">Строка 2 атрибутов налогоплательщика, макс 30 символов (20 если двойная ширина)</param>
        /// <param name="StringInfo2DoubleHeight">печать двойной ширины</param>
        /// <param name="StringInfo2DoubleWidth">печать двойной высоты</param>
        /// <param name="StringInfo3">Строка 3 атрибутов налогоплательщика, макс 30 символов (20 если двойная ширина)</param>
        /// <param name="StringInfo3DoubleHeight">печать двойной ширины</param>
        /// <param name="StringInfo3DoubleWidth">печать двойной высоты</param>
        /// <param name="TaxNumber"></param>
        /// <param name="AddTaxInfo"></param>
        /// <returns></returns>
        public override ReturnedStruct FPSetHeadLine(ushort Password, string StringInfo1, bool StringInfo1DoubleHeight, bool StringInfo1DoubleWidth
            , string StringInfo2, bool StringInfo2DoubleHeight, bool StringInfo2DoubleWidth
            , string StringInfo3, bool StringInfo3DoubleHeight, bool StringInfo3DoubleWidth
            , string TaxNumber, bool AddTaxInfo)
        {
            byte[] forsending = new byte[] { 22 };
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(Password));

            byte length1;
            byte[] stringBytes1;
            if (StringInfo1DoubleHeight) stringBytes1 = byteHelper.CodingBytes(StringInfo1, 20, out length1);
            else stringBytes1 = byteHelper.CodingBytes(StringInfo1, 30, out length1);
            length1 = byteHelper.SetBit(length1, 6, StringInfo1DoubleHeight);
            length1 = byteHelper.SetBit(length1, 7, StringInfo1DoubleWidth);
            forsending = byteHelper.Combine(forsending, new byte[] { length1 });
            forsending = byteHelper.Combine(forsending, stringBytes1);

            byte length2;
            byte[] stringBytes2;
            if (StringInfo2DoubleHeight) stringBytes2 = byteHelper.CodingBytes(StringInfo2, 20, out length2);
            else stringBytes2 = byteHelper.CodingBytes(StringInfo2, 30, out length2);
            length2 = byteHelper.SetBit(length2, 6, StringInfo2DoubleHeight);
            length2 = byteHelper.SetBit(length2, 7, StringInfo2DoubleWidth);
            forsending = byteHelper.Combine(forsending, new byte[] { length2 });
            forsending = byteHelper.Combine(forsending, stringBytes2);

            byte length3;
            byte[] stringBytes3;
            if (StringInfo3DoubleHeight) stringBytes3 = byteHelper.CodingBytes(StringInfo3, 20, out length3);
            else stringBytes3 = byteHelper.CodingBytes(StringInfo3, 30, out length3);
            length3 = byteHelper.SetBit(length3, 6, StringInfo3DoubleHeight);
            length3 = byteHelper.SetBit(length3, 7, StringInfo3DoubleWidth);
            forsending = byteHelper.Combine(forsending, new byte[] { length3 });
            forsending = byteHelper.Combine(forsending, stringBytes3);



            byte legthTax;
            byte[] stringTax = byteHelper.CodingBytes(TaxNumber, 12, out legthTax);
            //legthTax = SetBit(legthTax, 7, AddTaxInfo); - не работает
            forsending = byteHelper.Combine(forsending, new byte[] { legthTax });
            forsending = byteHelper.Combine(forsending, stringTax);
            return ExchangeWithFP(forsending);
        }

        #endregion

        #region Налоговые ставки

        /// <summary>
        /// SetTaxRate установить налоговые ставки Код: 25. 
        /// В фискальном режиме запись ставок в фискальную память происходит при открытии первого чека в смене. Иначе при выключении установленные ставки теряются. Можно принудительно сохранить ставки в фискальной памяти без открытия смены, открыв чек, а затем его аннулировав. Смешанный налог только для вложенного НДС. Налоговая ставка или сумма налоговой ставки и ставки сбора не должны превышать 99,99%.
        /// </summary>
        /// <param name="Password"></param>
        /// <param name="tTaxes"></param>
        /// <returns></returns>
        public override ReturnedStruct FPSetTaxRate(ushort Password, Taxes tTaxes)
        {


            byte[] forsending = new byte[] { 25 };
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(Password)); //пароль программирования
            forsending = byteHelper.Combine(forsending, new byte[] { (byte)tTaxes.MaxGroup });
            if (tTaxes.MaxGroup > 0)
            {
                forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(tTaxes.TaxA.TaxRate));
            }
            if (tTaxes.MaxGroup > 1)
            {
                forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(tTaxes.TaxB.TaxRate));
            }
            if (tTaxes.MaxGroup > 2)
            {
                forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(tTaxes.TaxC.TaxRate));
            }
            if (tTaxes.MaxGroup > 3)
            {
                forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(tTaxes.TaxD.TaxRate));
            }
            if (tTaxes.MaxGroup > 4)
            {
                forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(tTaxes.TaxE.TaxRate));
            }
            byte tBit = 0;
            //TODO - в ходе экспериментов это не работало
            //tBit = SetBit(tBit, 1, true);
            //if (tTaxes.quantityOfDecimalDigitsOfMoneySum == 0)
            //    tBit = SetBit(tBit, 0, true);
            //if (tTaxes.quantityOfDecimalDigitsOfMoneySum == 1)
            //    tBit = SetBit(tBit, 1, true);
            //if (tTaxes.quantityOfDecimalDigitsOfMoneySum == 2)
            //    tBit = SetBit(tBit, 2, true);
            //if (tTaxes.quantityOfDecimalDigitsOfMoneySum == 3)
            //    tBit = SetBit(tBit, 3, true);

            tBit = byteHelper.SetBit(tBit, 4, tTaxes.VAT);
            tBit = byteHelper.SetBit(tBit, 5, tTaxes.ToProgramChargeRates);
            forsending = byteHelper.Combine(forsending, new byte[] { tBit });
            if (tTaxes.ToProgramChargeRates)
            {

                if (tTaxes.MaxGroup > 0)
                {
                    byte[] forSend = BitConverter.GetBytes(tTaxes.TaxA.ChargeRates);
                    byte firstByte = forSend[1];
                    firstByte = byteHelper.SetBit(firstByte, 7, tTaxes.TaxA.VATAtCharge);
                    firstByte = byteHelper.SetBit(firstByte, 6, tTaxes.TaxA.ChargeAtVAT);
                    forSend[1] = firstByte;
                    forsending = byteHelper.Combine(forsending, forSend);
                }
                if (tTaxes.MaxGroup > 1)
                {
                    byte[] forSend = BitConverter.GetBytes(tTaxes.TaxB.ChargeRates);
                    byte firstByte = forSend[1];
                    firstByte = byteHelper.SetBit(firstByte, 7, tTaxes.TaxB.VATAtCharge);
                    firstByte = byteHelper.SetBit(firstByte, 6, tTaxes.TaxB.ChargeAtVAT);
                    forSend[1] = firstByte;
                    forsending = byteHelper.Combine(forsending, forSend);
                }
                if (tTaxes.MaxGroup > 2)
                {
                    byte[] forSend = BitConverter.GetBytes(tTaxes.TaxC.ChargeRates);
                    byte firstByte = forSend[1];
                    firstByte = byteHelper.SetBit(firstByte, 7, tTaxes.TaxC.VATAtCharge);
                    firstByte = byteHelper.SetBit(firstByte, 6, tTaxes.TaxC.ChargeAtVAT);
                    forSend[1] = firstByte;
                    forsending = byteHelper.Combine(forsending, forSend);
                }
                if (tTaxes.MaxGroup > 3)
                {
                    byte[] forSend = BitConverter.GetBytes(tTaxes.TaxD.ChargeRates);
                    byte firstByte = forSend[1];
                    firstByte = byteHelper.SetBit(firstByte, 7, tTaxes.TaxD.VATAtCharge);
                    firstByte = byteHelper.SetBit(firstByte, 6, tTaxes.TaxD.ChargeAtVAT);
                    forSend[1] = firstByte;
                    forsending = byteHelper.Combine(forsending, forSend);
                }
                if (tTaxes.MaxGroup > 4)
                {
                    byte[] forSend = BitConverter.GetBytes(tTaxes.TaxE.ChargeRates);
                    byte firstByte = forSend[1];
                    firstByte = byteHelper.SetBit(firstByte, 7, tTaxes.TaxE.VATAtCharge);
                    firstByte = byteHelper.SetBit(firstByte, 6, tTaxes.TaxE.ChargeAtVAT);
                    forSend[1] = firstByte;
                    forsending = byteHelper.Combine(forsending, forSend);
                }

                forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(tTaxes.ChargeRateOfGroupЕ));
            }

            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(tTaxes.TaxD.TaxRate));


            //Console.WriteLine("{0}", byteHelper.PrintByteArrayX(forsending));
            return ExchangeWithFP(forsending);
        }

        /// <summary>
        /// GetTaxRates прочитать налоговые ставки Код: 44. 
        /// </summary>
        /// <returns></returns>
        public override ReturnedStruct FPGetTaxRate()
        {
            byte[] forsending = new byte[] { 44 };

            ReturnedStruct r = ExchangeWithFP(forsending);
            byte[] answer = r.bytesReturn;
            if ((statusOperation) && (answer.Length > 5))
            {
                int tst = 0;
                Taxes tTax = new Taxes();
                tTax.MaxGroup = (short)answer[tst];
                tst++;
                tTax.DateSet = byteHelper.returnDatefromByte(answer, tst);
                tst = tst + 3;
                if (tTax.MaxGroup > 0)
                {
                    tTax.TaxA = new Tax((byte)FPTaxgroup.A, 1, BitConverter.ToUInt16(answer, tst), 0, false, false);
                    //tTax.TaxA.TaxGroup = ;
                    //tTax.TaxA.TaxNumber = 1;
                    //tTax.TaxA.TaxRate = ;
                    tst = tst + 2;
                }
                if (tTax.MaxGroup > 1)
                {
                    tTax.TaxB = new Tax((byte)FPTaxgroup.B, 2, BitConverter.ToUInt16(answer, tst), 0, false, false);
                    //tTax.TaxB.TaxGroup = (byte)FPTaxgroup.B;
                    //tTax.TaxB.TaxNumber = 2;
                    //tTax.TaxB.TaxRate = BitConverter.ToUInt16(answer, tst);
                    tst = tst + 2;
                }
                if (tTax.MaxGroup > 2)
                {
                    tTax.TaxC = new Tax((byte)FPTaxgroup.C, 3, BitConverter.ToUInt16(answer, tst), 0, false, false);
                    //tTax.TaxC.TaxGroup = (byte)FPTaxgroup.C;
                    //tTax.TaxC.TaxNumber = 3;
                    //tTax.TaxC.TaxRate = BitConverter.ToUInt16(answer, tst);
                    tst = tst + 2;
                }
                if (tTax.MaxGroup > 3)
                {
                    tTax.TaxD = new Tax((byte)FPTaxgroup.D, 4, BitConverter.ToUInt16(answer, tst), 0, false, false);
                    //tTax.TaxD.TaxGroup = (byte)FPTaxgroup.D;
                    //tTax.TaxD.TaxNumber = 4;
                    //tTax.TaxD.TaxRate = BitConverter.ToUInt16(answer, tst);
                    tst = tst + 2;
                }
                if (tTax.MaxGroup > 4)
                {
                    tTax.TaxE = new Tax((byte)FPTaxgroup.E, 5, BitConverter.ToUInt16(answer, tst), 0, false, false);
                    //tTax.TaxE.TaxGroup = (byte)FPTaxgroup.E;
                    //tTax.TaxE.TaxNumber = 5;
                    //tTax.TaxE.TaxRate = BitConverter.ToUInt16(answer, tst);
                    tst = tst + 2;
                }
                byte tByteStatus = answer[tst];
                tst++;
                tTax.quantityOfDecimalDigitsOfMoneySum = 0;
                if (byteHelper.GetBit(tByteStatus, 0))
                    tTax.quantityOfDecimalDigitsOfMoneySum = 1;
                if (byteHelper.GetBit(tByteStatus, 1))
                    tTax.quantityOfDecimalDigitsOfMoneySum = 2;
                if (byteHelper.GetBit(tByteStatus, 2))
                    tTax.quantityOfDecimalDigitsOfMoneySum = 3;


                tTax.VAT = byteHelper.GetBit(tByteStatus, 4);
                tTax.ToProgramChargeRates = byteHelper.GetBit(tByteStatus, 5);
                if (tTax.ToProgramChargeRates)
                {
                    if (tTax.MaxGroup > 0)
                    {
                        byte[] byteget = new byte[] { answer[tst], answer[tst + 1] };
                        tTax.TaxA.VATAtCharge = byteHelper.GetBit(byteget[1], 7);
                        tTax.TaxA.ChargeAtVAT = byteHelper.GetBit(byteget[1], 6);
                        byteget[1] = byteHelper.SetBit(byteget[1], 7, false);
                        byteget[1] = byteHelper.SetBit(byteget[1], 6, false);
                        tTax.TaxA.ChargeRates = BitConverter.ToUInt16(byteget, 0);
                        tst = tst + 2;
                    }

                    if (tTax.MaxGroup > 1)
                    {
                        byte[] byteget = new byte[] { answer[tst], answer[tst + 1] };
                        tTax.TaxB.VATAtCharge = byteHelper.GetBit(byteget[1], 7);
                        tTax.TaxB.ChargeAtVAT = byteHelper.GetBit(byteget[1], 6);
                        byteget[1] = byteHelper.SetBit(byteget[1], 7, false);
                        byteget[1] = byteHelper.SetBit(byteget[1], 6, false);
                        tTax.TaxB.ChargeRates = BitConverter.ToUInt16(byteget, 0);
                        tst = tst + 2;
                    }
                    if (tTax.MaxGroup > 2)
                    {
                        byte[] byteget = new byte[] { answer[tst], answer[tst + 1] };
                        tTax.TaxC.VATAtCharge = byteHelper.GetBit(byteget[1], 7);
                        tTax.TaxC.ChargeAtVAT = byteHelper.GetBit(byteget[1], 6);
                        byteget[1] = byteHelper.SetBit(byteget[1], 7, false);
                        byteget[1] = byteHelper.SetBit(byteget[1], 6, false);
                        tTax.TaxC.ChargeRates = BitConverter.ToUInt16(byteget, 0);
                        tst = tst + 2;
                    }
                    if (tTax.MaxGroup > 3)
                    {
                        byte[] byteget = new byte[] { answer[tst], answer[tst + 1] };
                        tTax.TaxD.VATAtCharge = byteHelper.GetBit(byteget[1], 7);
                        tTax.TaxD.ChargeAtVAT = byteHelper.GetBit(byteget[1], 6);
                        byteget[1] = byteHelper.SetBit(byteget[1], 7, false);
                        byteget[1] = byteHelper.SetBit(byteget[1], 6, false);
                        tTax.TaxD.ChargeRates = BitConverter.ToUInt16(byteget, 0);
                        tst = tst + 2;
                    }
                    if (tTax.MaxGroup > 4)
                    {
                        byte[] byteget = new byte[] { answer[tst], answer[tst + 1] };
                        tTax.TaxE.VATAtCharge = byteHelper.GetBit(byteget[1], 7);
                        tTax.TaxE.ChargeAtVAT = byteHelper.GetBit(byteget[1], 6);
                        byteget[1] = byteHelper.SetBit(byteget[1], 7, false);
                        byteget[1] = byteHelper.SetBit(byteget[1], 6, false);
                        tTax.TaxE.ChargeRates = BitConverter.ToUInt16(byteget, 0);
                        tst = tst + 2;
                    }

                    byte[] bytegetE = new byte[] { answer[tst], answer[tst + 1] };
                    //tTax.TaxE.VATAtCharge = GetBit(bytegetE[1], 7);
                    //tTax.TaxE.ChargeAtVAT = GetBit(bytegetE[1], 6);
                    bytegetE[1] = byteHelper.SetBit(bytegetE[1], 7, false);
                    bytegetE[1] = byteHelper.SetBit(bytegetE[1], 6, false);
                    tTax.ChargeRateOfGroupЕ = BitConverter.ToUInt16(bytegetE, 0);

                }

                this.currentTaxes = tTax;
            }
            return r;
        }
        #endregion

        #region Отчеты


        /// <summary>
        /// Код: 10. ArtReport печать отчета по артикулам
        /// </summary>
        /// <param name="pass"></param>
        /// <param name="CodeBeginning"></param>
        /// <param name="CodeFinishing"></param>
        /// <returns></returns>
        public override ReturnedStruct FPArtReport(ushort pass = 0, UInt32? CodeBeginning = null, UInt32? CodeFinishing = null)
        {
            byte[] forsending = new byte[] { 10 };
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(pass));
            if (CodeBeginning != null)
            {
                forsending = byteHelper.Combine(forsending, byteHelper.ConvertTobyte(CodeBeginning));
                forsending = byteHelper.Combine(forsending, byteHelper.ConvertTobyte(CodeFinishing));
            }
            return ExchangeWithFP(forsending);
        }


        //public UInt32 FPDayClrReport(ushort pass = 0)
        /// <summary>
        /// Код: 8.DayClrReport   печать и регистрация дневного отчета по финансовым операциям с обнулением дневных регистров
        /// Печать Z-отчета.
        /// </summary>
        /// <param name="pass"></param>
        public override ReturnedStruct FPDayClrReport(ushort pass = 0)
        {
            byte[] forsending = new byte[] { 8 };
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(pass));
            ReturnedStruct rt = ExchangeWithFP(forsending);
            if (rt.bytesReturn.Length > 0)
            { 
                byte[] forsending1 = new byte[] { 30 };
                forsending1 = byteHelper.Combine(forsending1, rt.bytesReturn);
                var copy = ExchangeWithFP(forsending1);
            }
            FPGetTaxRate(); // читаем ставки
            //TODO
            //По документации тут должно быть ответ с № КЛЕФ
            // return BitConverter.ToUInt32(answer, 0);
            return rt;
        }

        /// <summary>
        /// DayReport Печать дневного отчета о финансовым операциям Код: 9. 
        /// </summary>
        /// <param name="pass"></param>
        /// <returns></returns>
        public override ReturnedStruct FPDayReport(ushort pass = 0)
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            byte[] forsending = new byte[] { 9 };
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(pass));
            var rt =  ExchangeWithFP(forsending);            
            //return base.FPDayReport(pass);
            return rt;
        }


        /// <summary>
        /// PeriodicReport отчет и фискальной памяти за период  Код: 17. 
        /// </summary>
        /// <param name="pass"></param>
        /// <param name="FirstDay"></param>
        /// <param name="LastDay"></param>
        /// <returns></returns>
        public override ReturnedStruct FPPeriodicReport(ushort pass, DateTime FirstDay, DateTime LastDay)
        {
            byte[] forsending = new byte[9];
            byte[] passByte = BitConverter.GetBytes(pass);
            forsending[0] = 17;
            forsending[1] = passByte[0];
            forsending[2] = passByte[1];

            forsending[3] = Convert.ToByte(Convert.ToInt32(FirstDay.ToString("dd"), 16));
            forsending[4] = Convert.ToByte(Convert.ToInt32(FirstDay.ToString("MM"), 16));
            forsending[5] = Convert.ToByte(Convert.ToInt32(FirstDay.ToString("yy"), 16));
            forsending[6] = Convert.ToByte(Convert.ToInt32(LastDay.ToString("dd"), 16));
            forsending[7] = Convert.ToByte(Convert.ToInt32(LastDay.ToString("MM"), 16));
            forsending[8] = Convert.ToByte(Convert.ToInt32(LastDay.ToString("yy"), 16));
            return ExchangeWithFP(forsending);
        }

        /// <summary>
        /// PeriodicReportShort  Периоический отчет с фискальной памяти короткий Код: 26. 
        /// </summary>
        /// <param name="pass"></param>
        /// <param name="FirstDay"></param>
        /// <param name="LastDay"></param>
        /// <returns></returns>
        public override ReturnedStruct FPPeriodicReportShort(ushort pass, DateTime FirstDay, DateTime LastDay)
        {
            byte[] forsending = new byte[9];
            byte[] passByte = BitConverter.GetBytes(pass);
            forsending[0] = 26;
            forsending[1] = passByte[0];
            forsending[2] = passByte[1];

            forsending[3] = Convert.ToByte(Convert.ToInt32(FirstDay.ToString("dd"), 16));
            forsending[4] = Convert.ToByte(Convert.ToInt32(FirstDay.ToString("MM"), 16));
            forsending[5] = Convert.ToByte(Convert.ToInt32(FirstDay.ToString("yy"), 16));
            forsending[6] = Convert.ToByte(Convert.ToInt32(LastDay.ToString("dd"), 16));
            forsending[7] = Convert.ToByte(Convert.ToInt32(LastDay.ToString("MM"), 16));
            forsending[8] = Convert.ToByte(Convert.ToInt32(LastDay.ToString("yy"), 16));
            return ExchangeWithFP(forsending);
        }

        /// <summary>
        /// PeriodicReport2 Периоический отчет и фискальной памяти по номерам Код: 31.
        /// </summary>
        /// <param name="pass"></param>
        /// <param name="FirstNumber"></param>
        /// <param name="LastNumber"></param>
        /// <returns></returns>
        public override ReturnedStruct FPPeriodicReport2(ushort pass, UInt16 FirstNumber, UInt16 LastNumber)
        {
            byte[] forsending = new byte[7];
            byte[] passByte = BitConverter.GetBytes(pass);
            forsending[0] = 31;
            forsending[1] = passByte[0];
            forsending[2] = passByte[1];
            byte[] passFirstNumber = BitConverter.GetBytes(FirstNumber);
            forsending[3] = passFirstNumber[0];
            forsending[4] = passFirstNumber[1];
            byte[] passLastNumber = BitConverter.GetBytes(LastNumber);
            forsending[5] = passFirstNumber[0];
            forsending[6] = passFirstNumber[1];

            return ExchangeWithFP(forsending);
        }
        #endregion


        #region customer display
        public override bool showTopString(string Info)
        {
            Encoding cp866 = Encoding.GetEncoding(866);
            string tempStr = Info.Substring(0, Math.Min(20, Info.Length));
            byte[] forsending = byteHelper.Combine(new byte[] { 0x1b, 0x00, (byte)tempStr.Length }, cp866.GetBytes(tempStr));
            var answer = ExchangeWithFP(forsending);
            return connFP.statusOperation;
        }

        public override bool showBottomString(string Info)
        {
            Encoding cp866 = Encoding.GetEncoding(866);
            string tempStr = Info.Substring(0, Math.Min(20, Info.Length));
            byte[] forsending = byteHelper.Combine(new byte[] { 0x1b, 0x01, (byte)tempStr.Length }, cp866.GetBytes(tempStr));
            var answer = ExchangeWithFP(forsending);
            return connFP.statusOperation;
        }
        #endregion


        public override KleffInfo getKleff()
        {
            return new KleffInfo();
        }

        public override ReturnedStruct FPPrintCopy()
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            byte[] forsending = new byte[] { 30 };
            return ExchangeWithFP(forsending);
        }


    }
}
