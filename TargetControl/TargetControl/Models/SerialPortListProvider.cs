namespace TargetControl
{
    public interface ISerialPortListProvider
    {
        string[] GetPortNames();
    }

    public class SerialPortListProvider : ISerialPortListProvider
    {
        public string[] GetPortNames()
        {
            return System.IO.Ports.SerialPort.GetPortNames();
        }
    }
}