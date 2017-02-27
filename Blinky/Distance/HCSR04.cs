﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace Blinky.Distance
{
    public sealed class HCSR04
    {
        private GpioController gpio = GpioController.GetDefault();

        private GpioPin trig;
        private GpioPin echo;

        private int trig_Pin;
        private int echo_Pin;


        static Object deviceLock = new object();

        Stopwatch sw = new Stopwatch();

        public int TimeoutMilliseconds { get; set; } = 20;

        /// <summary>
        /// Create an HCSR04 Sensor
        /// </summary>
        /// <param name="trig_Pin"></param>
        /// <param name="echo_Pin"></param>
        /// <param name="timeoutMilliseconds">defaults to 20</param>
        public HCSR04(byte trig_Pin, byte echo_Pin, int timeoutMilliseconds)
        {
            Initialise(trig_Pin, echo_Pin, timeoutMilliseconds);
        }

        /// <summary>
        /// Initialise ultra sonic distance sensor
        /// </summary>
        /// <param name="trig_Pin"></param>
        /// <param name="echo_Pin"></param>
        /// <param name="maxDistance">Set Ultra Sonic maximum distance to detect.  This is approximate only.  Based on 34.3 cm per millisecond, 20 degrees C at sea level</param>
        //public HCSR04(byte trig_Pin, byte echo_Pin, int maxDistance)
        //{
        //    int milliSeconds = (int)(maxDistance / 34.3 * 2);
        //    Initialise(trig_Pin, echo_Pin, milliSeconds);
        //}

        private void Initialise(byte trig_Pin, byte echo_Pin, int timeoutMilliseconds)
        {
            this.trig_Pin = trig_Pin;
            this.echo_Pin = echo_Pin;

            TimeoutMilliseconds = timeoutMilliseconds;

            trig = gpio.OpenPin(trig_Pin);
            echo = gpio.OpenPin(echo_Pin);

            trig.SetDriveMode(GpioPinDriveMode.Output);
            echo.SetDriveMode(GpioPinDriveMode.Input);

            trig.Write(GpioPinValue.Low);
        }

    
        public double GetDistance()
        {
            return GetDistance(TimeoutMilliseconds);
        }

        /// <summary>
        /// Measures distance in centimeters
        /// </summary>
        /// <param name="timeoutMilliseconds">20 milliseconds is enough time to measure up to 3 meters</param>
        /// <returns></returns>
        public double GetDistance(int TimeoutMilliseconds)
        {
            lock (deviceLock)
            {
                
                trig.Write(GpioPinValue.Low);                     // ensure the trigger is off
                Task.Delay(TimeSpan.FromMilliseconds(1)).Wait();  // wait for the sensor to settle

                    trig.Write(GpioPinValue.High);                          // turn on the pulse
                    Task.Delay(TimeSpan.FromMilliseconds(.01)).Wait();      // let the pulse run for 10 microseconds
                    trig.Write(GpioPinValue.Low);                           // turn off the pulse

                var time = PulseIn(echo, GpioPinValue.High, TimeoutMilliseconds);

                // https://en.wikipedia.org/wiki/Speed_of_sound
                // speed of sound is 34300 cm per second or 34.3 cm per millisecond
                // since the sound waves traveled to the obstacle and back to the sensor
                // I am dividing by 2 to represent travel time to the obstacle                

                return time; //* 34.3 / 2.0; // at 20 degrees at sea level
            }
        }

        private double PulseIn(GpioPin pin, GpioPinValue value, int timeout)
        {
            sw.Restart();

            // Wait for pulse
            while (sw.ElapsedMilliseconds < timeout && pin.Read() != value) { }

            sw.Restart();

            // Wait for pulse end
            while (sw.ElapsedMilliseconds < timeout && pin.Read() == value) { }

            sw.Stop();

            return sw.Elapsed.TotalMilliseconds;
        }
    }
}
