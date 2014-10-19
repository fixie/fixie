﻿namespace Fixie
{
    public class CommandLineOption
    {
        public const string NUnitXml = "NUnitXml";
        public const string XUnitXml = "XUnitXml";
        public const string TeamCity = "TeamCity";
        public const string CustomListener = "CustomListener";
        public const string Parameter = "Parameter";

        public static string[] GetAll()
        {
            return new[]
            {
                NUnitXml,
                XUnitXml,
                TeamCity,
                CustomListener,
                Parameter
            };
        }
    }
}