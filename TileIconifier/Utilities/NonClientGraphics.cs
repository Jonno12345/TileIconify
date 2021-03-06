﻿using System;
using System.Drawing;

namespace TileIconifier.Utilities
{
    //A little convinience wrapper that creates a non client Graphics from handles 
    //and releases all the associated resources when it's disposed.
    class NonClientGraphics : IDisposable
    {
        private readonly IntPtr _hWnd = IntPtr.Zero;
        private IntPtr _hRgnClip = IntPtr.Zero;
        private IntPtr _hDc = IntPtr.Zero;

        public Graphics Graphics { get; }

        public NonClientGraphics(IntPtr hWnd, IntPtr hRgnClip)
        {
            _hWnd = hWnd;

            var flagsDcx = NativeMethods.DCX_WINDOW | NativeMethods.DCX_CACHE | NativeMethods.DCX_CLIPSIBLINGS;
            if (hRgnClip != IntPtr.Zero && hRgnClip != (IntPtr)1)
            {
                //GetDCEx takes ownership of the region, so we give it a copy. This also means that we don't need to delete it afterward.
                _hRgnClip = LayoutAndPaintUtils.CopyHRgn(hRgnClip);
                //Only set this flag if we provide a valid region to GetDCEx. Otherwise, the latter fails and return null.
                flagsDcx |= NativeMethods.DCX_INTERSECTRGN;
            }

            _hDc = NativeMethods.GetDCEx(_hWnd, _hRgnClip, flagsDcx);

            if (_hDc != IntPtr.Zero)
            {
                Graphics = Graphics.FromHdc(_hDc);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NonClientGraphics()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Graphics?.Dispose();
            }
            if (_hDc != IntPtr.Zero)
            {
                //The DC has been created and still exists
                NativeMethods.ReleaseDC(_hWnd, _hDc);
                _hDc = IntPtr.Zero;
                //The system has taken ownership of the region, so no need to delete it, just mark it as null.
                _hRgnClip = IntPtr.Zero;
            }
            else if (_hRgnClip != IntPtr.Zero)
            {
                //The region was created, but GetDCEx probably failed, so the system hasn't taken ownership 
                //of the region, so we need to delete it ourselves.
                NativeMethods.DeleteObject(_hRgnClip);
                _hRgnClip = IntPtr.Zero;
            }
        }
    }
}
