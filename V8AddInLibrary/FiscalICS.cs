using System;
using System.Reflection;
using System.Runtime.InteropServices;
using V8.AddIn;

[ComVisible(true)]
[Guid("21521874-4EEF-4715-BD16-C20BAAB4A1FA")] // произвольный Guid-идентификатор Вашей компоненты
[ProgId("AddIn.FiscalRegistrator")] // это имя COM-объекта, по которому Вы будете ее подключать
[ClassInterface(ClassInterfaceType.AutoDispatch)]
public class FiscalICS : LanguageExtenderAddIn
{
    public FiscalICS() : base(typeof(FiscalRegistrator), 2000) { }
}