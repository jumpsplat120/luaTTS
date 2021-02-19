using System;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;

namespace tts {
    public class MainClass {
        [DllExport("cereproc")]
        public static string CereVoiceTTS(string arg_string) {
            if (String.IsNullOrEmpty(arg_string)) { return "OUT=string voice_file|string lic_file|string ssml|string out_location(|string root_file|string cert_file|string key_file)"; }
            
            string[] args = arg_string.Split('|');

            string voice_file   = "";
            string lic_file     = "";
            string root_file    = "";
            string cert_file    = "";
            string key_file     = "";
            string ssml         = "";
            string out_location = "";
            string return_value = "";

            for (int i = 0; i < args.Length; i++) {
                if (i == 0) {
                    voice_file = args[i];
                } else if (i == 1) {
                    lic_file = args[i];
                } else if (i == 2) {
                    ssml = args[i];
                } else if (i == 3) {
                    out_location = args[i];
                } else if (i == 4) {
                    root_file = args[i];
                } else if (i == 5) {
                    cert_file = args[i];
                } else if (i == 6) {
                    key_file = args[i];
                } else {
                    return_value = "ERR=Excess amount of arguments; expected 4 or 7 arguments, recieved " + args.Length.ToString() + " args.";
                }
                if (!String.IsNullOrEmpty(return_value)) { break; }
            }

            if (String.IsNullOrEmpty(return_value)) {
                if (args.Length >= 4) {
                    if (args.Length == 4 || args.Length == 7) {
                        string text = "<speak>" + ssml + "</speak>";

                        SWIGTYPE_p_CPRCEN_engine engine = cerevoice_eng.CPRCEN_engine_new();

                        int result = cerevoice_eng.CPRCEN_engine_load_voice(engine, voice_file, "", 0, lic_file, root_file, cert_file, key_file);

                        if (result != 0) {
                            try {
                                int chan = cerevoice_eng.CPRCEN_engine_open_default_channel(engine);
                                cerevoice_eng.CPRCEN_engine_channel_to_file(engine, chan, out_location, CPRCEN_AUDIO_FORMAT.CPRCEN_RIFF);
                                cerevoice_eng.CPRCEN_engine_channel_speak(engine, chan, text, text.Length, 1);
                                cerevoice_eng.CPRCEN_engine_delete(engine);
                                return_value = "OUT=true";
                            } catch (Exception e) {
                                return_value = "ERR=" + e.ToString();
                            }
                        } else {
                            return_value = "ERR=Failed to create engine with passed arguments!";
                        }
                    } else {
                        //excess is caught in the loop, so this is for a mismatched amount only
                        return "ERR=Invalid amount of arguments; expected 4 or 7 arguments, recieved " + args.Length.ToString() + " args.";
                    }
                } else {
                    return "ERR=Missing required arguments; expected 4 or 7 arguments, recieved " + args.Length.ToString() + " args.";
                }
            }

            return return_value;
        }
        [DllExport("ms")]
        public static string MicrosoftTTS(string arg_string) {
            if (String.IsNullOrEmpty(arg_string)) { return "OUT=bool get_voices|int sample|int bit_depth|int channel_mode|string voice|string ssml|string output"; }

            string[] args = arg_string.Split('|');

            bool get_voices     = false;
            int sample          = 0;
            int bit_depth       = 0;
            int channel_mode    = 0;
            string voice        = "";
            string ssml         = "";
            string output       = "";
            string return_value = "";

            for (int i = 0; i < args.Length; i++) {
                if (i == 0) {
                    get_voices = args[i].ToLower() == "true";
                } else if (i == 1) {
                    bool success = Int32.TryParse(args[i], out sample);
                    if (!success) { return_value = "ERR=Unable to parse " + args[i] + " to int."; }
                } else if (i == 2) {
                    bool success = Int32.TryParse(args[i], out bit_depth);
                    if (!success) { return_value = "ERR=Unable to parse " + args[i] + " to int."; }
                } else if (i == 3) {
                    bool success = Int32.TryParse(args[i], out channel_mode);
                    if (!success) { return_value = "ERR=Unable to parse " + args[i] + " to int."; }
                } else if (i == 4) {
                    voice = args[i];
                } else if (i == 5) {
                    ssml = args[i];
                } else if (i == 6) {
                    output = args[i];
                } else {
                    return_value = "ERR=Excess amount of arguments; expected 7 arguments, recieved " + args.Length.ToString() + " args.";
                }
                if (!String.IsNullOrEmpty(return_value)) { break; }
            }

            if (String.IsNullOrEmpty(return_value)) {
                if (args.Length == 7) {
                    SpeechSynthesizer synth = new SpeechSynthesizer();

                    if (get_voices) {
                        return_value = "OUT=";
                        foreach (InstalledVoice installed_voice in synth.GetInstalledVoices()) { return_value += installed_voice.VoiceInfo.Name + "|"; }
                    } else {
                        AudioChannel ch = AudioChannel.Mono;
                        AudioBitsPerSample bps = AudioBitsPerSample.Eight;
                        SpeechAudioFormatInfo format = new SpeechAudioFormatInfo(1, bps, ch);

                        if (bit_depth == 8) { bps = AudioBitsPerSample.Eight; } else if (bit_depth == 16) { bps = AudioBitsPerSample.Sixteen; } else { return_value = "ERR=Invalid bit depth; bit depth must be either 8 or 16."; }
                        if (channel_mode == 1) { ch = AudioChannel.Mono; } else if (channel_mode == 2) { ch = AudioChannel.Stereo; } else { return_value = "ERR=Invalid channel mode; channel must be either 1 or 2 (mono or stereo)."; }

                        if (String.IsNullOrEmpty(return_value)) {
                            try {
                                format = new SpeechAudioFormatInfo(sample, bps, ch);
                                synth.SelectVoice(voice);
                                synth.SetOutputToWaveFile(output, format);
                                synth.SpeakSsml("<speak version='1.0' xml:lang='en'>" + ssml + "</speak>");
                                return_value = "OUT=true";
                            } catch(Exception e) {
                                return_value = "ERR=" + e.ToString();
                            }
                        }
                    }
                } else {
                    return_value = "ERR=Missing required arguments; expected 7 arguments, recieved " + args.Length.ToString() + " args.";
                }
            }

            return return_value;
        }
    }
}
