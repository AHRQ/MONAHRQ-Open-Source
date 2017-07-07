using System;
namespace Monahrq.Sdk.DataProvider.Builder
{
    public interface IConnectionStringBuilderView
    {
        IConnectionStringBuilderViewModel Model { get; }
    }
}
