using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using IotDemo.Services;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDemo.ViewModels
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            var ioc = SimpleIoc.Default;

            ioc.Register<MainViewModel>();

            if (ViewModelBase.IsInDesignModeStatic)
            {
                ioc.Register<IGpioService, DummyGpioService>();
                ioc.Register<INetService, DummyNetService>();
            }
            else
            {
                ioc.Register<IGpioService, GpioService>();
                ioc.Register<INetService, NetService>();
            }

            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }
    }
}
