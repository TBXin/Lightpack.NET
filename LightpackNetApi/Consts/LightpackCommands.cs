namespace LightpackNetApi.Consts
{
    static class LightpackCommands
    {
        // Common commands
        public const string ApiKey = "apikey:{0}";
        public const string Lock = "lock";
        public const string Unlock = "unlock";

        // Get commands
        public const string GetStatus = "getstatus";
        public const string GetStatusApi = "getstatusapi";
        public const string GetProfile = "getprofile";
        public const string GetProfiles = "getprofiles";
        public const string GetCountLeds = "getcountleds";

        // Set commands
        public const string SetColorCommand = "setcolor:";
        public const string SetColorArgs = "{0}-{1},{2},{3};";
        public const string SetColor = SetColorCommand + SetColorArgs;
        public const string SetGamma = "setgamma:{0}";
        public const string SetSmooth = "setsmooth:{0}";
        public const string SetBrightness = "setbrightness:{0}";
        public const string SetProfile = "setprofile:{0}";
        public const string SetStatus = "setstatus:{0}";
    }
}
