
using CentralLib.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CentralLib.Connections
{
    public class ConnectFactory : SerialPort, IConnectFactory
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private int FPNumber;
        public byte[] bytesBegin = { (byte)WorkByte.DLE, (byte)WorkByte.STX };
        public byte[] bytesEnd = { (byte)WorkByte.DLE, (byte)WorkByte.ETX };

        private byte[] glbytesResponse;
        private int waiting; // waiting time for serial port answer

        public bool statusOperation { get; set; }
        public byte ByteStatus { get; set; } // Возврат ФР статус
        public byte ByteResult { get; set; } // Возврат ФР результат
        public byte ByteReserv { get; set; } // Возврат ФР результат
        public string errorInfo { get; set; }
        public int ConsecutiveNumber { get; set; }
        public byte[] glbytesForSend { get; set; }
        public byte[] glbytesPrepare { get; set; }
        public bool useCRC16 { get; set; }

        private bool autoOpen=false, autoClose=false;

        private string IpAdress;
        private int port;
        
        private ByteHelper byteHelper;

        public bool isOpen { get { return base.IsOpen; } }

        public ConnectFactory(DefaultPortCom defPortCom,bool autoOpen=false, bool autoClose=false, int inFPnumber=0)
        {
            this.autoOpen = autoOpen;
            this.autoClose = autoClose;

            this.FPNumber = inFPnumber;
            NLog.GlobalDiagnosticsContext.Set("FPNumber", inFPnumber);
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            this.IpAdress = base.PortName;            

            byteHelper = new ByteHelper();
            byteHelper.initialCrc16();
            base.PortName = defPortCom.sPortNumber;
            base.BaudRate = defPortCom.baudRate;
            base.Parity = defPortCom.parity;
            base.DataBits = defPortCom.dataBits;
            base.StopBits = defPortCom.stopBits;
            base.WriteTimeout = defPortCom.writeTimeOut;
            base.ReadTimeout = defPortCom.readTimeOut;
            this.waiting = defPortCom.waiting;
            this.errorInfo = "";
            this.ConsecutiveNumber = 0;
            if (autoOpen)
                Open();
        }

       

        private void setError(string errorInfo = "Unknown error", byte ByteStatus = 255, byte ByteResult = 255, byte ByteReserv = 255)
        {
            this.ByteStatus = ByteStatus;
            this.ByteResult = ByteResult;
            this.ByteReserv = ByteReserv;
            this.statusOperation = false;
            this.errorInfo += errorInfo + "; ";            
        }

        //        private byte[] ExchangeFP(byte[] inputbyte, bool useCRC16 = false, bool repeatError = false)
        //        {

        //            byte[] unsigned = null;

        //            this.ByteStatus = 0;
        //            this.ByteResult = 0;
        //            this.ByteReserv = 0;
        //            this.statusOperation = false;
        //            this.errorInfo = "";
        //            if (!repeatError)
        //            {
        //                this.ConsecutiveNumber++;
        //            }
        //            this.glbytesForSend = inputbyte;

        //            if (!base.IsOpen)
        //                base.Open();
        //            if (!base.IsOpen)
        //            {
        //                setError("Не возможно подключиться к порту:" + base.PortName.ToString());
        //                throw new ArgumentException(this.errorInfo);
        //            }
        //#if Debug
        //            Console.ForegroundColor = ConsoleColor.Blue;
        //            Console.WriteLine("подготовка к отправке:{0}", byteHelper.PrintByteArrayX(inputbyte));         
        //#endif
        //            base.BaseStream.Write(inputbyte, 0, inputbyte.Length);
        //            //base.Write(inputbyte, 0, inputbyte.Length);

        //#if Debug
        //            Console.ForegroundColor = ConsoleColor.Blue;
        //            Console.WriteLine("отправлено");
        //#endif
        //            byte[] result = new byte[] { };
        //            Thread.Sleep(this.waiting);
        //            int bufferSize = base.BytesToRead;
        //            int twait = 0;
        //            do
        //            {
        //                byte[] result_fromPort = new byte[bufferSize];

        //#if Debug
        //                Console.ForegroundColor = ConsoleColor.Green;
        //                Console.WriteLine("подготовка к к получению {0}", bufferSize);
        //#endif
        //                if (bufferSize == 0)
        //                {
        //#if Debug
        //                    Console.ForegroundColor = ConsoleColor.Yellow;
        //                    Console.WriteLine("bufferSize == 0;время ожидания:{0}", 600);
        //#endif
        //                    Thread.Sleep(600);
        //                    bufferSize = base.BytesToRead;
        //                    result_fromPort = new byte[bufferSize];// на поиск ошибки в этой строке потратил 3 часа.... не пиши синкр
        //                    if (bufferSize == 0)
        //                    {
        //                        break;
        //                    }
        //                }
        //                int x = base.BaseStream.Read(result_fromPort, 0, bufferSize);
        //                //base.Read(result_fromPort, 0, bufferSize);

        //#if Debug
        //                Console.ForegroundColor = ConsoleColor.Green;
        //                Console.WriteLine("получено:{0}", PrintByteArray(result_fromPort));
        //#endif
        //                int count_wait = 0;
        //                for (int tByte = 0; tByte < bufferSize; tByte++)
        //                {
        //                    if ((result_fromPort[tByte] == (byte)WorkByte.ACK) || (result_fromPort[tByte] == (byte)WorkByte.SYN))
        //                    {
        //                        count_wait++;
        //                    }
        //                }
        //                if (bufferSize == 1 && result_fromPort[0] == (byte)WorkByte.ACK)
        //                {
        //#if Debug
        //                    Console.ForegroundColor = ConsoleColor.Yellow;
        //                    Console.WriteLine("bufferSize==1&&ACK Ждем:{0}", 1000);
        //#endif
        //                    Thread.Sleep(1000);
        //                }
        //                else if ((bufferSize < 10) || (bufferSize == count_wait) || ((count_wait > 0) && (bufferSize / count_wait < 2)))
        //                {
        //#if Debug
        //                    //Console.ForegroundColor = ConsoleColor.Green;
        //                    //Console.WriteLine("коефициент байтов ожидания:{0}", bufferSize / count_wait);
        //#endif  
        //                    twait++;

        //#if Debug
        //                    Console.ForegroundColor = ConsoleColor.Green;
        //                    Console.WriteLine("время ожидания:{0}", twait * 300);
        //#endif
        //                    Thread.Sleep(twait * 300);
        //                }
        //                if (twait > 10) break;
        //                result = byteHelper.Combine(result, result_fromPort);

        //                bufferSize = base.BytesToRead;
        //            } while (bufferSize > 0);

        //            byte[] BytesBegin = new byte[4];
        //            Buffer.BlockCopy(inputbyte, 0, BytesBegin, 0, 4);

        //            int positionPacketBegin = byteHelper.ByteSearch(result, BytesBegin);

        //            if (positionPacketBegin < 0)
        //            {
        //                setError("В байтах ответа не найдено начало, порт:" + base.PortName.ToString());
        //                throw new ArgumentException(this.errorInfo);
        //                // return null;
        //            }
        //            //}
        //            int positionPacketEnd = -1;
        //            int tCurrentPos = positionPacketBegin + 7;
        //            int tPostEnd = -1;
        //            do
        //            {
        //                tCurrentPos++;
        //                tPostEnd = byteHelper.ByteSearch(result, bytesEnd, tCurrentPos);
        //                if (tPostEnd != -1)
        //                {
        //                    tCurrentPos = tPostEnd;

        //                    if (result[tPostEnd - 1] != (byte)WorkByte.DLE)
        //                    {
        //                        positionPacketEnd = tPostEnd;
        //                        break;
        //                    }
        //                    else if ((result[tPostEnd - 1] == (byte)WorkByte.DLE) && (result[tPostEnd - 2] == (byte)WorkByte.DLE))
        //                    {
        //                        positionPacketEnd = tPostEnd;
        //                        // break; 
        //                    }
        //                }
        //            } while (tCurrentPos < result.Length);
        //            if (positionPacketEnd < 0)
        //            {
        //                setError("В байтах ответа не найдено конец, порт:" + base.PortName.ToString());
        //                throw new ArgumentException(this.errorInfo);
        //            }

        //            unsigned = new byte[positionPacketEnd - positionPacketBegin + 2];
        //            Buffer.BlockCopy(result, positionPacketBegin, unsigned, 0, positionPacketEnd - positionPacketBegin + 2);

        //            //this.bytesOutput = unsigned;
        //            //TODO: доработать проверку CRC && CRC16


        //            unsigned = byteHelper.returnWithOutDublicateDLE(unsigned);
        //            this.glbytesResponse = unsigned;

        //            byte byteCheckSum = 0;

        //            byteCheckSum = unsigned[unsigned.Length - 3];
        //            unsigned[unsigned.Length - 3] = 0;

        //            if (byteCheckSum != byteHelper.getchecksum(unsigned,useCRC16))
        //            {
        //                //не совпала чек сумма
        //                this.statusOperation = false;
        //                setError("Не правильная чек сумма обмена, Ком порт:" + this.PortName);
        //                throw new ArgumentException(this.errorInfo);
        //            }

        //            this.statusOperation = true;
        //            this.ByteStatus = unsigned[4];
        //            this.ByteResult = unsigned[5];
        //            this.ByteReserv = unsigned[6];
        //            this.errorInfo = "";
        //            //Console.WriteLine(PrintByteArray(unsigned));
        //            //Console.WriteLine(PrintByteArray(unsigned.Skip(8).Take(unsigned.Length - 7 - 3 - ((useCRC16) ? 2 : 0)).ToArray()));
        //            return unsigned;//.Skip(8).Take(unsigned.Length-7-3- ((useCRC16)?2:0) ).ToArray();
        //        }

        /// <summary>
        /// Если что то начинает не работать рекомендую заменить async на этот метод
        /// </summary>
        /// <param name="inputbyte"></param>
        /// <param name="useCRC16"></param>
        /// <param name="repeatError"></param>
        /// <returns></returns>
        private ReturnedStruct ExchangeFPW(byte[] inputbyte, bool useCRC16 = false, bool repeatError = false)
        {            
            ReturnedStruct rRs = new ReturnedStruct()
            {
                ByteStatus = 0,
                ByteResult = 0,
                ByteReserv = 0,
                statusOperation = false,
                errorInfo = "",
                fullBytesSend = inputbyte,
                command = inputbyte[0]
            };
            this.ByteStatus = 0;
            this.ByteResult = 0;
            this.ByteReserv = 0;
            this.statusOperation = false;
            this.errorInfo = "";
            if (!repeatError)
            {
                this.ConsecutiveNumber++;
            }
            this.glbytesForSend = inputbyte;
            int taskTry = -1;
         
         
            byte[] unsigned = null;
            //using (TcpClient client = new TcpClient())
            {
              //  logger.Trace(">>>>Connect {0}:{1}", IPAddress.Parse(IpAdress), port);
                //client.Connect(IPAddress.Parse(IpAdress), port);
                //logger.Trace("<<<Connect {0}:{1}", IPAddress.Parse(IpAdress), port);
                //client.ReceiveTimeout = 10000;
                //client.SendTimeout = 10000;
#if Debug
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("подготовка к отправке:{0}", byteHelper.PrintByteArrayX(inputbyte));
#endif
                //using (var networkStream = client.GetStream())
                {
                    //networkStream.WriteTimeout = 10000;
                    //networkStream.ReadTimeout = 10000;
                Begin:
                  //  logger.Trace("begin:{0}", taskTry);
                    if (taskTry > 9)
                    {
                        string sf = String.Format("Выполнено {0} циклов, ответа нет {1}", taskTry, base.PortName);
                        setError(sf);
                    //    logger.Trace(sf);
                        rRs.ByteStatus = ByteStatus;
                        rRs.ByteResult = ByteResult;
                        rRs.ByteReserv = ByteReserv;
                        rRs.statusOperation = false;
                        rRs.errorInfo += errorInfo + "; ";
                        //return rRs;
                        throw new ApplicationException(sf);
                    }
                    taskTry++;
                    //logger.Trace("Write try{1}, send to FP:{0}", byteHelper.PrintByteArrayX(inputbyte), taskTry);
                    //networkStream.Write(inputbyte, 0, inputbyte.Length);
                    base.Write(inputbyte, 0, inputbyte.Length);
                    //logger.Trace("Write");
                    //networkStream.Flush();
                    
                    //logger.Trace("Flush");
                    //base.Write(inputbyte, 0, inputbyte.Length);

#if Debug
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("отправлено");
#endif
                    byte[] BytesBegin = new byte[4];
                    Buffer.BlockCopy(inputbyte, 0, BytesBegin, 0, 4);

                    int positionPacketEnd = -1;
                    int tCurrentPos = 7;
                    //int tPostEnd = -1;



                    byte[] result = new byte[] { };

                    for (int x = 1; x < 15; x++)
                    {
                        //logger.Trace("FOR {0} in 15", x);
                        int coef = x;
                        if ((inputbyte[2] == 13) || (inputbyte[2] == 9)) // Если отчеты то ждем 2 раза дольше
                        {
                            coef = x * 2;
                        }
                        Thread.Sleep(coef * this.waiting);
                        //logger.Trace("ожидание в цикле {1} = {0}", coef * this.waiting, x);

                        //int bufferSize = base.BytesToRead;

                        int twait = 0;
                        
                        while (base.BytesToRead > 0)
                        {
                          //  logger.Trace("DataAvailable");
                            byte[] result_fromPort = new byte[1024];
                            int bufferSize = base.BytesToRead;
                            base.Read(result_fromPort, 0, bufferSize);
                            //int bufferSize = base.Read(result_fromPort, 0, result_fromPort);
                            //int bufferSize = networkStream.Read(result_fromPort, 0, result_fromPort.Length);
                            if (bufferSize <= 0)
                                break;
                            byte[] byteRead = result_fromPort.Take(bufferSize).ToArray();
                            //logger.Trace("DataAvailable:{0}", byteHelper.PrintByteArrayX(byteRead));
                            if ((byteRead.Length == 1) && (byteRead[0] == (byte)WorkByte.NAK))
                            {
                              //  logger.Trace("NAK={0}", byteRead[0]);
                                Thread.Sleep(200);
                                //networkStream.Flush();
                                goto Begin;
                            }
                            int count_wait = 0;
                            for (int tByte = 0; tByte < bufferSize; tByte++)
                            {
                                if ((result_fromPort[tByte] == (byte)WorkByte.ACK) || (result_fromPort[tByte] == (byte)WorkByte.SYN))
                                {
                                    count_wait++;
                                }
                            }
                            if (bufferSize == 1 && ((result_fromPort[0] == (byte)WorkByte.ACK) || (result_fromPort[0] == (byte)WorkByte.SYN)))
                            {
                                Thread.Sleep(200);
                                //logger.Trace("byte read:{0}, sleep:{1}", result_fromPort[0], 200);
                            }
                            else if ((bufferSize < 10) || (bufferSize == count_wait) || ((count_wait > 0) && (bufferSize / count_wait < 2)))
                            {
                                twait++;
                                Thread.Sleep(twait * 500);
                                //logger.Trace("bytes read:{0}, sleep:{1}", byteHelper.PrintByteArrayX(byteRead), 400);
                            }
                            if (twait > 10) break;
                            result = byteHelper.Combine(result, byteRead);
                        };
                        //logger.Trace("DataAvailable exit while");
                        int psPacketBegin = byteHelper.ByteSearch(result, BytesBegin);
                        int psPacketEnd = byteHelper.ByteSearch(result, bytesEnd, psPacketBegin);
                        if ((result.Length > 9) && (psPacketBegin > 0) && (psPacketEnd > 0))
                        {
                            //logger.Trace("Good read");
                            break;
                        }
                        if ((x > 5) && (result.Length < 10))
                        {
                            Thread.Sleep(200);
                            //logger.Trace("Flush");
                            //networkStream.Flush();
                            //logger.Trace("goto Begin");
                            goto Begin;
                        }
                    }

                    if (result.Length < 9)
                    {
                        setError("Не полный ответ сервера");
                        rRs.ByteStatus = ByteStatus;
                        rRs.ByteResult = ByteResult;
                        rRs.ByteReserv = ByteReserv;
                        rRs.statusOperation = false;
                        rRs.bytesReturn = result;
                        rRs.errorInfo += errorInfo + "; ";
                        //return rRs;
                        throw new ArgumentException(this.errorInfo);
                    }
                    //networkStream.Close();
                    //client.Close();

                    //logger.Warn("full bytes read:{0}", byteHelper.PrintByteArrayX(result));
                    int positionPacketBegin = byteHelper.ByteSearch(result, BytesBegin);

                    if (positionPacketBegin < 0)
                    {
                        if (taskTry < 10)
                        {
                            setError("В байтах ответа по операции " + inputbyte[2].ToString() + " не найдено \"начало\", port:" + base.PortName + " ответ:" + byteHelper.PrintByteArrayX(result));
                            //logger.Trace("Повторяем отправку байт");
                            Thread.Sleep(100);
                            //networkStream.Flush();
                            //logger.Trace("goto Begin");
                            goto Begin;
                        }
                        setError("В байтах ответа по операции " + inputbyte[2].ToString() + " не найдено \"начало\", port:" + base.PortName + " ответ:" + byteHelper.PrintByteArrayX(result));
                        rRs.ByteStatus = ByteStatus;
                        rRs.ByteResult = ByteResult;
                        rRs.ByteReserv = ByteReserv;
                        rRs.statusOperation = false;
                        rRs.bytesReturn = result;
                        rRs.errorInfo += errorInfo + "; ";
                        return rRs;
                        //throw new ArgumentException(this.errorInfo);
                        // return null;
                    }
                    //}
                    positionPacketEnd = -1;
                    tCurrentPos = positionPacketBegin + 7;
                    //tPostEnd = -1;
                    //do
                    //{
                    //    tCurrentPos++;
                    //    tPostEnd = byteHelper.ByteSearch(result, bytesEnd, tCurrentPos);
                    //    if (tPostEnd != -1)
                    //    {
                    //        tCurrentPos = tPostEnd;

                    //        if (result[tPostEnd - 1] != (byte)WorkByte.DLE)
                    //        {
                    //            positionPacketEnd = tPostEnd;
                    //            break;
                    //        }
                    //        else if ((result[tPostEnd - 1] == (byte)WorkByte.DLE) && (result[tPostEnd - 2] == (byte)WorkByte.DLE))
                    //        {
                    //            positionPacketEnd = tPostEnd;
                    //            // break; 
                    //        }
                    //    }
                    //} while (tCurrentPos < result.Length);
                    for (int curPos = result.Length - 1; curPos > tCurrentPos; curPos--)
                    {
                        if ((result[curPos] == (byte)WorkByte.ETX) && (result[curPos - 1] == (byte)WorkByte.DLE))
                        {
                            positionPacketEnd = curPos - 1;
                            break;
                        }
                    }

                    if (positionPacketEnd < 0)
                    {
                        setError("В байтах ответа по операции " + inputbyte[2].ToString() + " не найдено \"конец\", port:" + base.PortName + " ответ:" + byteHelper.PrintByteArrayX(result));
                        rRs.ByteStatus = ByteStatus;
                        rRs.ByteResult = ByteResult;
                        rRs.ByteReserv = ByteReserv;
                        rRs.statusOperation = false;
                        rRs.errorInfo += errorInfo + "; ";
                        rRs.bytesReturn = result;
                        //return rRs;
                        throw new ArgumentException(this.errorInfo);
                    }
                    //e  } while (base.BytesToRead>0);


                    //if (useCRC16)
                    //{
                    //    unsigned = new byte[positionPacketEnd - positionPacketBegin + 4];
                    //    Buffer.BlockCopy(result, positionPacketBegin, unsigned, 0, positionPacketEnd - positionPacketBegin + 4);
                    //}
                    //else
                    //{
                    unsigned = new byte[positionPacketEnd - positionPacketBegin + 2];
                    Buffer.BlockCopy(result, positionPacketBegin, unsigned, 0, positionPacketEnd - positionPacketBegin + 2);
                    //}
                    //this.bytesOutput = unsigned;
                    //TODO: доработать проверку CRC && CRC16
                    unsigned = byteHelper.returnWithOutDublicateDLE(unsigned);
                    this.glbytesResponse = unsigned;
                    byte byteCheckSum = 0;
                    //if (useCRC16)
                    //{
                    //    byteCheckSum = unsigned[unsigned.Length - 5];
                    //    unsigned[unsigned.Length - 5] = 0;
                    //}
                    //else
                    //{
                    byteCheckSum = unsigned[unsigned.Length - 3];
                    unsigned[unsigned.Length - 3] = 0;
                    //  }
                    //if (unsigned[2] < 2 && unsigned[3] == 0) // Если только начали работать то контрольную сумму еще не можем проверить
                    //{

                    //}
                    //else
                    //{
                    if ((byteCheckSum != byteHelper.getchecksum(unsigned)))
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("Не правильная check сумма {1}!={2} обмена, port:{0}", base.PortName, byteCheckSum, byteHelper.getchecksum(unsigned));
                        this.statusOperation = false;
                        setError(sb.ToString());
                        //networkStream.Flush();
                        //logger.Trace("goto Begin");
                        goto Begin;
                        //не совпала чек сумма


                        //throw new ArgumentException(this.errorInfo);
                    }
                    //}
                    //logger.Warn("End<<<<<<<<");
                    this.statusOperation = true;
                    this.ByteStatus = unsigned[4];
                    this.ByteResult = unsigned[5];
                    this.ByteReserv = unsigned[6];
                    //this.errorInfo = "";
                    rRs.fullBytesReturn = result;
                    rRs.ByteStatus = unsigned[4];
                    rRs.ByteResult = unsigned[5];
                    rRs.ByteReserv = unsigned[6];
                    rRs.bytesReturn = unsigned;
                    rRs.statusOperation = true;
                    //rRs.errorInfo += errorInfo + "; ";                    
                }
            }
            //Console.WriteLine(PrintByteArray(unsigned));
            //Console.WriteLine(PrintByteArray(unsigned.Skip(8).Take(unsigned.Length - 7 - 3 - ((useCRC16) ? 2 : 0)).ToArray()));
            return rRs;//.Skip(8).Take(unsigned.Length-7-3- ((useCRC16)?2:0) ).ToArray();
        }




        public virtual byte[] dataExchange(byte[] input, bool repeatError = false)
        {
            //logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            return this.dataExchange(input, this.useCRC16, repeatError);
        }

        public virtual byte[] dataExchange(byte[] input)
        {
            //logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            return this.dataExchange(input, this.useCRC16, false);
        }

        public virtual byte[] dataExchange(byte[] input, bool useCRC16 = false, bool repeatError = false)
        {
            //logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            try
            {
                var sReturn = ExchangeFPW(byteHelper.prepareForSend(ConsecutiveNumber, input, useCRC16, repeatError), useCRC16, repeatError);
                if (sReturn.bytesReturn != null)
                    return byteHelper.returnBytesWithoutSufixAndPrefix(sReturn.bytesReturn);
                else
                {
                   // logger.Error("null reference returned");
                    throw new ApplicationException("null reference returned");
                }
            }
            catch (Exception ex)
            {
             //   logger.Error(ex);
                setError(ex.Message);
                return null;
               // throw ex;
            }
            //            Func<byte[], Task<byte[]>> function = async (byte[] inByte) =>
            //            {
            //                logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            //                this.glbytesPrepare = inByte;
            //                logger.Trace(">>>ExchangeFP");
            //                byte[] treturn = await ExchangeFP(byteHelper.prepareForSend(ConsecutiveNumber, inByte, useCRC16, repeatError), useCRC16, repeatError);
            //                logger.Trace("<<<ExchangeFP");
            //                return treturn;
            //            };
            //            logger.Trace(">>>function(input)");
            //            Task<byte[]> answer = function(input);
            //            logger.Trace("<<<function(input)");
            //            //byte[] answer = ExchangeFP(prepareForSend(input, useCRC16));
            //            //return await answer;
            //            try
            //            {
            //                return byteHelper.returnBytesWithoutSufixAndPrefix(answer.Result);
            //            }
            //            catch (AggregateException e)
            //            {
            //                StringBuilder sb = new StringBuilder();
            //                sb.AppendFormat("Caught {0}, exception: {1}", e.InnerExceptions.Count, string.Join(", ", e.InnerExceptions.Select(x => x.Message)));
            //                setError(sb.ToString());
            //                throw new ArgumentException(this.errorInfo);
            //#if Debug
            //                Console.ForegroundColor = ConsoleColor.Red;
            //                Console.WriteLine("Описание ошибки:{0}", this.errorInfo);
            //#endif
            //            }
        }








        public virtual new void Open()
        {
            if (base.IsOpen)
            {
                base.Close();
            }
            try
            {
                base.Open();
                this.statusOperation = true;
            }
            catch (IOException e)
            {
                this.statusOperation = false;


                setError(e.Message);
                #if Debug
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Описание ошибки:{0}", this.errorInfo);
                #endif
            }
            catch (AggregateException e)
            {
                this.statusOperation = false;
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Caught {0}, exception: {1}", e.InnerExceptions.Count, string.Join(", ", e.InnerExceptions.Select(x => x.Message)));

                setError(sb.ToString());
#if Debug
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Описание ошибки:{0}", this.errorInfo);
#endif
            }
        }

        public virtual new void Close()
        {
            base.Close();
        }


    }
}
