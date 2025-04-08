using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using vJoyInterfaceWrap;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
//using System.Runtime.InteropServices;

using webserv;
//if you think about it, the only thing that matters is where the POSTs are sent
vJoy joystick = new vJoy();
vJoy.JoystickState state = new vJoy.JoystickState();
//vJoy can't do 8 direction POV Hat, only 4 or continuous, so probably do continuous
//TODO: Feed values using mapping, monitor joystick to see if it works, maybe play game
uint id = 5;
VjdStat status = joystick.GetVJDStatus(id);//vJoyInterface.dll must be in system32/syswow64
switch (status)
{
    case VjdStat.VJD_STAT_OWN:
        Console.WriteLine("vJoy Device {0} is already owned by this feeder\n", id);
        break;
    case VjdStat.VJD_STAT_FREE:
        Console.WriteLine("vJoy Device {0} is free\n", id);
        break;
    case VjdStat.VJD_STAT_BUSY:
        Console.WriteLine(
         "vJoy Device {0} is already owned by another feeder\nCannot continue\n", id);
        break;
    case VjdStat.VJD_STAT_MISS:
        Console.WriteLine(
         "vJoy Device {0} is not installed or disabled\nCannot continue\n", id);
        break;
    default:
        Console.WriteLine("vJoy Device {0} general error\nCannot continue\n", id);
        break;
};
string prt;
if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!joystick.AcquireVJD(id))))
    prt = String.Format("Failed to acquire vJoy device number {0}.", id);
else
    prt = String.Format("Acquired: vJoy device number {0}.", id);
Console.WriteLine(prt);
state.bDevice = (byte)id;
Console.WriteLine(state.Buttons.ToString());
Console.WriteLine(state.AxisX.ToString());
//I don't get how this could set buttons one by one, would be useful though
//state.Buttons = (uint)(0x1 << (int)(i / 20));
//state.AxisX = 40000;
//for (int i = 1; i < 10; i++)
//{
    //joystick.SetBtn(Convert.ToBoolean(1), 5, (uint)i);
    //state.Buttons = (uint)i;
    //joystick.UpdateVJD(id, ref state);
//}

//switch case for button mapping, I think counting starts at 1 for buttons
//wtflip is going on? what's the difference between SetX(Y,5,Z) and state.X=Y then UpdateVJD(5, ref state)?
//And what does ResetVJD, RelinquishVJD even do, do I even need it
//Using state seems much faster but state.Buttons is a single integer, setting it to N turns on the Nth button
//But also turns the rest off
//state.ButtonsExX doesn't seem to do anything
joystick.ResetVJD(id);
joystick.SetBtn(false, id, (uint)7);
joystick.SetBtn(false, id, (uint)8);
string directory = Directory.GetCurrentDirectory();

//joystick.RelinquishVJD(id);

HttpListener _httpListener = new HttpListener();
_httpListener.IgnoreWriteExceptions = true;
Console.WriteLine("Starting server...");
_httpListener.Prefixes.Add("http://localhost:5000/"); // add prefix "http://localhost:5000/"
_httpListener.Start(); // start server (Run application as Administrator!)
Console.WriteLine("Server started.");
Thread _responseThread = new Thread(ResponseThread(_httpListener));
_responseThread.Start(); // start the response thread


ParameterizedThreadStart ResponseThread(HttpListener _httpListener)
{
    while (true)
    {
        HttpListenerContext context = _httpListener.GetContext(); // get a context
        HttpListenerRequest req = context.Request;
        //Console.WriteLine(req.Url); // Now, you'll find the request URL in context.Request.Url
        System.IO.Stream body = req.InputStream;
        System.Text.Encoding encoding = req.ContentEncoding;
        System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
        string s = reader.ReadToEnd();//convert POST body i.e. the gamepad state
        if (req.ContentType != null)
        {
            //Console.WriteLine("Client data content type {0} (Method: {1}) (Enc: {2})", req.ContentType, req.HttpMethod, req.ContentEncoding);
            Gamepad? gp = JsonSerializer.Deserialize<Gamepad>(s);//gp.Buttons now buttons list, gp.Axes axes list

            joystick.SetBtn(Convert.ToBoolean(gp.Buttons[0]), id, (uint)1);
            joystick.SetBtn(Convert.ToBoolean(gp.Buttons[1]), id, (uint)2);
            joystick.SetBtn(Convert.ToBoolean(gp.Buttons[3]), id, (uint)3);
            joystick.SetBtn(Convert.ToBoolean(gp.Buttons[4]), id, (uint)4);
            joystick.SetBtn(Convert.ToBoolean(gp.Buttons[6]), id, (uint)5);
            joystick.SetBtn(Convert.ToBoolean(gp.Buttons[7]), id, (uint)6);
            joystick.SetBtn(Convert.ToBoolean(gp.Buttons[13]), id, (uint)9);
            joystick.SetBtn(Convert.ToBoolean(gp.Buttons[14]), id, (uint)10);
            joystick.SetAxis((int)((1.000 + gp.Axes[0]) * 16384), id, HID_USAGES.HID_USAGE_X);
            joystick.SetAxis((int)((1.000 + gp.Axes[1]) * 16384), id, HID_USAGES.HID_USAGE_Y);
            joystick.SetAxis((int)((1.000 + gp.Axes[2]) * 16384), id, HID_USAGES.HID_USAGE_RX);
            joystick.SetAxis((int)((1.000 + gp.Axes[3]) * 16384), id, HID_USAGES.HID_USAGE_RZ);
            joystick.SetAxis((int)((1.000 + gp.Axes[4]) * 16384), id, HID_USAGES.HID_USAGE_Z);
            joystick.SetAxis((int)((1.000 + gp.Axes[5]) * 16384), id, HID_USAGES.HID_USAGE_RY);
            //Console.WriteLine(gp.Axes[9]);
            switch (gp.Axes[9])
            {
                case -1:
                    joystick.SetContPov(0, id, 1);
                    break;
                case 1:
                    joystick.SetContPov(31500, id, 1);
                    break;
                case -0.7142857313156128:
                    joystick.SetContPov(4500, id, 1);
                    break;
                case 0.7142857313156128:
                    joystick.SetContPov(27000, id, 1);
                    break;
                case -0.4285714030265808://Right
                    joystick.SetContPov(9000, id, 1);
                    break;
                case 0.4285714626312256:
                    joystick.SetContPov(22500, id, 1);
                    break;
                case -0.1428571343421936://Lower right
                    joystick.SetContPov(13500, id, 1);
                    break;
                case 0.14285719394683838:
                    joystick.SetContPov(18000, id, 1);
                    break;
                default:
                    joystick.SetContPov(-1, id, 1);
                    break;
            }
        }
        //Console.WriteLine("Client data content length {0}", req.ContentLength64);
        //Console.WriteLine(s);
        //Convert the json post to a string and display it on the console.
        string indexp;
        using (StreamReader streamReader = new StreamReader(directory + @"\index.html", Encoding.UTF8))
        {
            indexp = streamReader.ReadToEnd();
        }
        //figure out vJoy interface
        byte[] _responseArray = Encoding.UTF8.GetBytes(indexp); // get the bytes to response
        context.Response.OutputStream.Write(_responseArray, 0, _responseArray.Length); // write bytes to the output stream
        context.Response.KeepAlive = false; // set the KeepAlive bool to false
        context.Response.Close(); // close the connection
        //Console.WriteLine("Response given to a request.");
    }
}
namespace webserv
{
    public class Gamepad { 
        public IList<int>? Buttons {  get; set; }
        public IList<double>? Axes {  get; set; }
    }
    
}