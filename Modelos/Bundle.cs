using Windows.Storage; 

namespace Perfect_OCR.Modelos
{
    public class Bundle
    {

        private static ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
        private static string _string;
        private static bool _bool;
        private static bool? _bool2;
        private static int _int;
        private static double _double;

        public Bundle() { }

        #region Keys Salvas
        public static double GetDouble(string Key, double Default)
        {
            if (container.Values.ContainsKey(Key)) _double = (double)container.Values[Key]; else _double = Default;
            return _double;
        }
        public static string GetString(string Key, string Default)
        {
            if (container.Values.ContainsKey(Key)) _string = (string)container.Values[Key]; else _string = Default;
            return _string;
        }
        public static bool GetBool(string Key, bool Default)
        {
            if (container.Values.ContainsKey(Key)) _bool = (bool)container.Values[Key]; else _bool = Default;
            return _bool;
        }
        public static bool? GetBooleon(string Key, bool? Default)
        {
            if (container.Values.ContainsKey(Key)) _bool2 = (bool?)container.Values[Key]; else _bool2 = Default;
            return _bool2;
        }
        public static int GetInt(string Key, int Default)
        {
            if (container.Values.ContainsKey(Key)) _int = (int)container.Values[Key]; else _int = Default;
            return _int;
        }

        #endregion

        #region Keys Salvar

         
        public static void PutDouble(string Key, double Value)
        {
            if (!container.Values.ContainsKey(Key)) container.Values.Add(Key, Value); else container.Values[Key] = Value;
        }
        public static void PutString(string Key, string Value)
        {
            if (!container.Values.ContainsKey(Key)) container.Values.Add(Key, Value); else container.Values[Key] = Value;
        }
        public static void PutBool(string Key, bool Value)
        {
            if (!container.Values.ContainsKey(Key)) container.Values.Add(Key, Value); else container.Values[Key] = Value;
        }
        public static void PutBooleon(string Key, bool? Value)
        {
            if (!container.Values.ContainsKey(Key)) container.Values.Add(Key, Value); else container.Values[Key] = Value;
        }
        public static void PutInt(string Key, int Value)
        {
            if (!container.Values.ContainsKey(Key)) container.Values.Add(Key, Value); else container.Values[Key] = Value;
        }

        #endregion
       

    }
}
