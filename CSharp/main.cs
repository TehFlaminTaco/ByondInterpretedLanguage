using System;
using ByondLang.Tokenizer;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using RGiesecke.DllExport;

namespace ByondLang
{
    class MainProgram
    {
        public static Listener listener;
        static void Main(string[] args)
        {
            TokenCompiler.GetTokens();
            TokenCompiler.CompileTokens();

            listener = new Listener();
        }

        [DllExport("interact", CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static string Interact(int numArgs, IntPtr argPtr){
            string[] data = ProcessArgs(numArgs, argPtr);
            return listener.Interact(data[0]);
        }

        /// <summary>
        /// Processes the arguments for BYOND DLL calls and returns an array of
        /// strings for easier processing.
        /// </summary>
        /// <param name="numArgs">The length of string arguments received via the DLL call.</param>
        /// <param name="argPtr">The pointer to the character arrays where the string lie.</param>
        /// <returns>A list of strings. The list is numArgs members long.</returns>
        private static string[] ProcessArgs(int numArgs, IntPtr argPtr)
        {
            string[] strOut = new string[numArgs];
            IntPtr[] argsArray = new IntPtr[numArgs];

            Marshal.Copy(argPtr, argsArray, 0, numArgs);

            for (int i = 0; i < numArgs; i++)
            {
                strOut[i] = Marshal.PtrToStringAnsi(argsArray[i]);
            }

            return strOut;
        }
    }
}
