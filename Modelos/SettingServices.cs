
using Windows.ApplicationModel; 
using Windows.ApplicationModel.Resources;
using Windows.Media.SpeechSynthesis;

namespace Perfect_OCR.Modelos
{
    public class SettingServices : Bundle
    {
        public bool IsHistorical => GetBool("IsHistorical", true);
        public static void Historical(bool save) => PutBool("IsHistorical", save);
        public bool IsCheckedBtn1 => GetBool("Btn1", true);
        public bool IsCheckedBtn2 => GetBool("Btn2", false);
        ResourceLoader loader = new ResourceLoader(); 

        public string DisplayName => Package.Current.DisplayName;

        public string Publisher => Package.Current.PublisherDisplayName;

        public string Version
        {
            get
            {
                var v = Package.Current.Id.Version;
                return $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            }
        }

        public string IsTextVelocidade
        {
            get
            {
                string name;
                if (VelocidadeLeitura == 0) name = loader.GetString("lenta");
                else if (VelocidadeLeitura == 1) name = loader.GetString("normal");
                else if (VelocidadeLeitura == 2) name = loader.GetString("rapida");
                else name = loader.GetString("Muitorapido");

                return $"{loader.GetString("velocidade")}: {name}";
            }
        }

        public string IsTextNivel
        {
            get
            {
                string name;
                if (NivelLeitura == 0) name = loader.GetString("grossa");
                else if (NivelLeitura == 1) name = loader.GetString("normal");
                else name = loader.GetString("fina");

                return $"{loader.GetString("densidade")}: {name}";
            }
        }

        public double VelocidadeLeitura
        {

            get
            {
                SpeechSynthesizer synthesizer = new SpeechSynthesizer();
                return GetDouble("VelocidadeLeitura", synthesizer.Options.SpeakingRate);
            }

        }

        public double NivelLeitura
        {

            get
            {
                SpeechSynthesizer synthesizer = new SpeechSynthesizer();
                return GetDouble("NivelLeitura", synthesizer.Options.AudioPitch);
            }

        }

        public string FontFamily => GetString("FontFamily", "Segoe UI"); 

        public static void PutFontFamily(string font) => PutString("FontFamily", font); 

        public static void IsCheckeds(string name, bool btn)
        {
            PutBool(name, btn);
        }

        public static void IsSliders(string name, double btn) => PutDouble(name, btn); 

        public string IsLanguageSelected
        {
            get
            {
                SpeechSynthesizer synthesizer = new SpeechSynthesizer();
                VoiceInformation currentVoice = synthesizer.Voice;

                return GetString("Language", currentVoice.Id);
            }
        }

        public static void LanguageSelected(string index) => PutString("Language", index);  
    }

}
