﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ACT.FFXIVPing
{
    class WindowsMessagePump : IPluginComponent, IDisposable
    {
        private FFXIVPingPlugin _plugin;
        private MainController _controller;
        
        private WinEventDelegate _hookPtrDele;
        private IntPtr _hookPtrForeground = IntPtr.Zero;
        private string _lastActivatedProcessPath = null;

        public void AttachToAct(FFXIVPingPlugin plugin)
        {
            _plugin = plugin;
            _controller = plugin.Controller;
        }

        public void PostAttachToAct(FFXIVPingPlugin plugin)
        {
            _hookPtrDele = WinEventProc;
            _hookPtrForeground = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_MINIMIZEEND, IntPtr.Zero, _hookPtrDele, 0, 0, WINEVENT_OUTOFCONTEXT);
            _hookPtrDele(IntPtr.Zero, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, 0, 0, 0, 0);
        }

        public void Dispose()
        {
            UnhookWinEvent(_hookPtrForeground);
            _hookPtrForeground = IntPtr.Zero;
            _hookPtrDele = null;
            _plugin = null;
        }

        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int UnhookWinEvent(IntPtr hWinEventHook);

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 0x3;
        private const uint EVENT_SYSTEM_MENUPOPUPSTART = 0x6;
        private const uint EVENT_SYSTEM_MENUPOPUPEND = 0x7;
        private const uint EVENT_SYSTEM_MINIMIZEEND = 0x17;

//        [DllImport("user32.dll")]
//        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
//
//        private string GetActiveWindowTitle()
//        {
//            const int nChars = 256;
//            IntPtr handle = IntPtr.Zero;
//            StringBuilder Buff = new StringBuilder(nChars);
//            handle = Win32APIUtils.GetForegroundWindow();
//
//            if (GetWindowText(handle, Buff, nChars) > 0)
//            {
//                return Buff.ToString();
//            }
//            return null;
//        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (eventType != EVENT_SYSTEM_FOREGROUND && eventType != EVENT_SYSTEM_MENUPOPUPSTART &&
                eventType != EVENT_SYSTEM_MENUPOPUPEND && eventType != EVENT_SYSTEM_MINIMIZEEND)
            {
                return;
            }
            string path = null;
            if (hwnd != IntPtr.Zero)
            {
                path = Win32APIUtils.GetProcessPathByWindow(hwnd);
            }
            if (path == null)
            {
                path = Win32APIUtils.GetForgegroundProcessPath();
            }
            if (path == null)
            {
                return;
            }

            if (_lastActivatedProcessPath != path)
            {
                _lastActivatedProcessPath = path;
                _controller.NotifyActivatedProcessPathChanged(false, path);
            }
            //            Log.Text += GetActiveWindowTitle() + "\r\n";
//            _controller.NotifyLogMessageAppend(false, path + "\r\n");

        }
    }
}