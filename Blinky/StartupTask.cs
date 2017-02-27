using System;
using Windows.Devices.Gpio;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using Blinky.Distance;
using System.Threading;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Blinky
{
    public sealed class StartupTask : IBackgroundTask
    {
        const int LED_PIN = 5;
        GpioPin pin;
        Timer timer;
        Timer ledTimer;
        BackgroundTaskDeferral _deferral;
        HCSR04 HCSR04 = new HCSR04(20,21,20);
        TimeSpan period = TimeSpan.FromMilliseconds(500);
        public int TimeoutMilliseconds { get; set; } = 20;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            InitGPIO();
            timer = new Timer(new TimerCallback(TimerProc), null, 0, 100);
            ledTimer = new Timer(new TimerCallback(led_Timer), null, 0, 500);
            //ledTimer = ThreadPoolTimer.CreatePeriodicTimer(led_timer, period);
            //timer = ThreadPoolTimer.CreatePeriodicTimer(timer_Tick, TimeSpan.FromMilliseconds(500));

        }

        private void led_Timer(object state)
        {
            if (pin.Read() == GpioPinValue.High)
            {
                pin.Write(GpioPinValue.Low);
            }
            else
            {
                pin.Write(GpioPinValue.High);
            }
        }

        private void TimerProc(object state)
        {
            double distance = HCSR04.GetDistance();
            ledTimer.Change(0, TimeSpan.FromMilliseconds(distance * 1000).Milliseconds);
            System.Diagnostics.Debug.WriteLine(TimeSpan.FromMilliseconds(distance * 1000).Milliseconds);

        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                pin = null;
                return;
            }

            pin = gpio.OpenPin(LED_PIN);

            if (pin == null)
            {
                return;
            }

            pin.Write(GpioPinValue.High);
            pin.SetDriveMode(GpioPinDriveMode.Output);


        }
        private void led_timer(ThreadPoolTimer timer)
        {
            System.Diagnostics.Debug.WriteLine(pin.Read());
           
        }

       

        private void timer_Tick(ThreadPoolTimer timer)
        {
            double distance = HCSR04.GetDistance();
            //ledTimer.Cancel();
            //ledTimer = ThreadPoolTimer.CreatePeriodicTimer(led_timer, TimeSpan.FromMilliseconds(distance * 300));
        }
    }
}
