using HeBianGu.Base.WpfBase;
using System.ComponentModel;
using System.Windows;

namespace HeBianGu.App.WebProxy
{
    internal class DataSourceLocator
    {
        public DataSourceLocator()
        {
            ServiceRegistry.Instance.Register<ShellViewModel>();
        }
        public ShellViewModel ShellViewModel => ServiceRegistry.Instance.GetInstance<ShellViewModel>();

    }
}
