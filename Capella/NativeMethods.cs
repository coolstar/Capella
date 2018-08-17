using System;
using System.Runtime.InteropServices;

namespace Capella
{
    public static class NativeMethods
    {
        /// <summary>
        /// Brings the thread that created the specified window into the
        /// foreground and activates the window. Keyboard input is directed
        /// to the window, and various visual cues are changed for the user.
        /// The system assigns a slightly higher priority to the thread that
        /// created the foreground window than it does to other threads.
        /// </summary>
        /// <param name="hWnd">A handle to the window that should be
        /// activated and brought to the foreground.
        /// </param>
        /// <returns>If the window was brought to the foreground, the
        /// return value is nonzero. </returns>
        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>Shows a Window</summary>
        /// <remarks>
        /// <para>To perform certain special effects when showing or hiding a
        /// window, use AnimateWindow.</para>
        /// <para>The first time an application calls ShowWindow, it should use
        /// the WinMain function's nCmdShow parameter as its nCmdShow ..
        /// Subsequent calls to ShowWindow must use one of the values in the
        /// given list, instead of the one specified by the WinMain function's
        /// nCmdShow parameter.</para>
        /// <para>As noted in the discussion of the nCmdShow parameter, the
        /// nCmdShow value is ignored in the first call to ShowWindow if the
        /// program that launched the application specifies startup information
        /// in the structure. In this case, ShowWindow uses the information
        /// specified in the STARTUPINFO structure to show the window. On
        /// subsequent calls, the application must call ShowWindow with ..
        /// set to SW_SHOWDEFAULT to use the startup information provided by ..
        /// program that launched the application. This behavior is designed ..
        /// the following situations: </para>
        /// <list type="">
        ///    <item>Applications create their main window by calling ..
        ///    with the WS_VISIBLE flag set. </item>
        ///    <item>Applications create their main window by calling ..
        ///    with the WS_VISIBLE flag cleared, and later call ShowWindow ..
        ///    SW_SHOW flag set to make it visible.</item>
        /// </list></remarks>
        /// <param name="hWnd">Handle to the window.</param>
        /// <param name="nCmdShow">Specifies how the window is to be shown.
        /// This parameter is ignored the first time an application calls
        /// ShowWindow, if the program that launched the application provides a
        /// STARTUPINFO structure. Otherwise, the first time ShowWindow .. ,
        /// the value should be the value obtained by the WinMain function ..
        /// nCmdShow parameter. In subsequent calls, this parameter ..
        /// the WindowShowStyle members.</param>
        /// <returns>
        /// If the window was previously visible, the return value is nonzero.
        /// If the window was previously hidden, the return value is zero.
        /// </returns>
        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd,
            WindowShowStyle nCmdShow);
    }
}