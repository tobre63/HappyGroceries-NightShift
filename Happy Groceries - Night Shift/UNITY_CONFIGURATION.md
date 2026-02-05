# Unity Configuration Notes

## UDP Port Configuration for Unity/Visual Studio Messaging

If you encounter a warning about UDP port 56390 being unavailable:

```
Unable to use UDP port 56390 for VS/Unity messaging.
You should check if another process is already bound to this port
or if your firewall settings are compatible.
```

### Solutions:

1. **Check for Port Conflicts**
   - Ensure no other application is using UDP port 56390
   - Use `netstat -ano | findstr 56390` (Windows) or `lsof -i :56390` (macOS/Linux) to check

2. **Disable Unity/VS Messaging** (if not needed)
   - In Unity Editor: Edit → Preferences → External Tools
   - Uncheck "Editor Attaching" option
   - This will disable the debugger attachment feature but resolves the port conflict

3. **Configure Firewall**
   - Ensure your firewall allows Unity Editor to use UDP port 56390
   - Add an exception for Unity Editor in your firewall settings

4. **Use a Different Port** (Advanced)
   - This requires editing Unity's internal configuration
   - Generally not recommended unless necessary

### Note
This is an editor-specific warning and does not affect the built game. It only impacts the debugging connection between Unity Editor and Visual Studio.

## Input System Configuration

The project uses both the legacy Input Manager and the new Input System (`activeInputHandler: 2`).
This is the recommended setting during the transition period to the new Input System.

The deprecation warning about Input Manager is informational and does not require immediate action.
