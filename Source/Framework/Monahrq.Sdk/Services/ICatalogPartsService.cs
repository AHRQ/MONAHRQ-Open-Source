using System;
namespace Monahrq.Sdk.Services
{
    public interface IContainerPartsService
    {
        System.Collections.Generic.IEnumerable<T> GetRegisteredExportAttributes<T>() 
            where T : System.ComponentModel.Composition.ExportAttribute;
    }
}
