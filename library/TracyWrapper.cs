using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace tracy
{
    static class Tracy
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct ___tracy_source_location_data
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string name;
            [MarshalAs(UnmanagedType.LPStr)]
            public string function;
            [MarshalAs(UnmanagedType.LPStr)]
            public string file;
            public uint line;
            public uint color;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ___tracy_c_zone_context
        {
            public uint id;
            public int active;
        }

        [DllImport("TracyProfiler", EntryPoint = "___tracy_emit_zone_begin", CallingConvention = CallingConvention.Cdecl)]
        public static extern ___tracy_c_zone_context EmitZoneBegin(IntPtr srcloc, int active);

        [DllImport("TracyProfiler", EntryPoint = "___tracy_emit_zone_begin_test", CallingConvention = CallingConvention.Cdecl)]
        public static extern ___tracy_c_zone_context EmitZoneBeginTest(ref ___tracy_source_location_data srcloc);

        [DllImport("TracyProfiler", EntryPoint = "___tracy_alloc_src_loc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AllocSrcLocInternal([MarshalAs(UnmanagedType.LPStr)] string file, [MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string function, uint line, uint color);

        [DllImport("TracyProfiler", EntryPoint = "___tracy_emit_zone_end", CallingConvention = CallingConvention.Cdecl)]
        public static extern void EmitZoneEnd(___tracy_c_zone_context ctx);

        [DllImport("TracyProfiler", EntryPoint = "___tracy_emit_frame_mark", CallingConvention = CallingConvention.Cdecl)]
        public static extern void EmitFrameMarkInternal(IntPtr namePtr);

        private static MD5 md5 = MD5.Create();
        public static IntPtr AllocSrcLoc(string name, uint color = 0x0, [CallerFilePath] string file = "", [CallerMemberName] string function = "", [CallerLineNumber] uint line = 0)
        {
            if (color == 0x0)
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(file));
                color = (uint)(hash[0] & (hash[1] >> 8) & (hash[2] >> 16));
            }
            return AllocSrcLocInternal(file, name, function, line, color);
        }

        public static void EmitFrameMark()
        {
            EmitFrameMarkInternal(IntPtr.Zero);
        }

        public static ___tracy_c_zone_context BeginZoneNC(IntPtr strloc)
        {
            return EmitZoneBegin(strloc, 1);
        }
        public static void EndZone(___tracy_c_zone_context ctx)
        {
            EmitZoneEnd(ctx);
        }

        public class Zone : IDisposable
        {
            private ___tracy_c_zone_context ctx;
            private bool disposed = false;

            public Zone(IntPtr strloc)
            {
                ctx = EmitZoneBegin(strloc, 1);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                // Check to see if Dispose has already been called.
                if (!this.disposed)
                {
                    EmitZoneEnd(ctx);
                    this.disposed = true;
                }
            }
        }
    }

}
