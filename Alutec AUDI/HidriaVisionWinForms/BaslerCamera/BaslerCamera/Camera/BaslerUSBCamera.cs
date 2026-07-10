//using Basler.Pylon;
//using BaslerCam;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BaslerCam
//{
//    class BaslerUSBCamera : BaslerCamera
//    {

//        #region public methods


//        /// <summary>
//        /// Sets the exposure of the camera.
//        /// </summary>
//        /// <param name="exposure">Exposure of the camera in raw format</param>
//        /// <exception cref="CameraException"></exception>
//        /// <exception cref="InvalidOperationException"></exception>
//        /// <exception cref="ObjectDisposedException"></exception>
//        public void SetExposureUSB(long exposure)
//        {
//            // Check if object has been already disposed
//            if (DisposedValue)
//                throw new ObjectDisposedException(GetType().FullName);

//            // Check if camera is opened before setting the exposure
//            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
//                throw new InvalidOperationException("Camera not opened or connected!");

//            try
//            {
//                long Min = (long)Cam.Parameters[PLCamera.ExposureTime].GetMinimum();
//                long Max = (long)Cam.Parameters[PLCamera.ExposureTime].GetMaximum();

//                // Check if exposure is within the range and set the value
//                if (exposure < Min)
//                    Cam.Parameters[PLCamera.ExposureTime].SetValue(Min);
//                else if (exposure > Max)
//                    Cam.Parameters[PLCamera.ExposureTime].SetValue(Max);
//                else
//                    Cam.Parameters[PLCamera.ExposureTime].SetValue(exposure);
//            }
//            catch (Exception ex)
//            {
//                throw new CameraException("Cannot set the exposure!", ex);
//            }
//        }

//        /// <summary>
//        /// Gets the the exposure of the camera.
//        /// </summary>
//        /// <returns>Returns the exposure of the camera in raw format</returns>
//        /// <exception cref="CameraException"></exception>
//        /// <exception cref="InvalidOperationException"></exception>
//        /// <exception cref="ObjectDisposedException"></exception>
//        public long GetExposureUSB()
//        {
//            // Check if object has been already disposed
//            if (DisposedValue)
//                throw new ObjectDisposedException(GetType().FullName);

//            // Check if camera is opened before getting the exposure
//            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
//                throw new InvalidOperationException("Camera not opened or connected!");

//            try
//            {
//                return (long)Cam.Parameters[PLCamera.ExposureTime].GetValue();
//            }
//            catch (Exception ex)
//            {
//                throw new CameraException("Cannot get the exposure!", ex);
//            }
//        }

//        #endregion
//    }
//}
