﻿using System;
using System.Reflection;

namespace Editor
{
    internal static class Global
    {
        /// <summary>
        /// Helper method for getting a "pack://" URI for a given relative file based on the
        /// assembly that this class is in.
        /// </summary>
        public static Uri GetPackUri(string relativeFile)
        {
            return new Uri(GetPackUriString(relativeFile));
        }
 
        public static string GetPackUriString(string relativeFile)
        {
            return $"pack://application:,,,/{AssemblyShortName};component/{relativeFile}";
        }

        private static string AssemblyShortName
        {
            get
            {
                if (_assemblyShortName != null)
                    return _assemblyShortName;

                var assembly = typeof(Global).GetTypeInfo().Assembly;
                _assemblyShortName = assembly.ToString().Split(',')[0];
                return _assemblyShortName;
            }
        }
        private static string _assemblyShortName;
    }
}