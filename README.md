# generic-gamepads-for-windows
If Windows won't recognise your no-name gamepad, no emulators or mappers work, and you don't want to buy another controller, then this method is a last resort. **[WIP]**</br>

## Test if the gamepad API supports your controller here: https://hardwaretester.com/gamepad/ to see if this could even work if it was finished

If your gamepad can't change any of the values, then this hack can't help you. You could try manual emulation or creating a driver yourself (the latter might not even help if your gamepad is recognised as a USB Composite device).
</br>
 ### The goal of the script is as follows:</br>
1. Set up a simple HTTP (localhost) webserver in C# 
2. Send the browser environment object values (navigator.getGamepads()) to the server.<br>
2.5. Mapping is done manually in the script, generic controllers will usually have different mappings.</br>
3.  Feed a vJoy device with the mapped browser values. The docs for the vJoy interface API are in the SDK on the vJoy GitHub/sourceforge.
4.  Maybe use UCR to merge the vJoy devices if necessary.
5.  (Optional) Make a more robust HTTP server in C# so that there won't be problems sending values to a server over the internet. The latency would be insane however.
6.  (Less optional) Send/Read the user-agent from the client and use a different method for Firefox, since it handles the Gamepad array differently.
7.  (Just as optional) Use KeepAlive so a new connection isn't made every POST request for lower latency.
8.  (More optional) Create a form for either the C# script or the HTML to enter the port number and remote target, and to map the gamepad inputs in a GUI.
</br></br>~~The vJoy gamepad will hopefully respond fast enough to be playable, otherwise AutoHotKey should be replaced, probably with C#.~~
The Edge JS runtime available to AHK can't access browser environment variables and I don't know a way to use AHK with a headless browser, so this will be changed to use C# since it can be used to make a webserver and can interface with vJoy easily.
