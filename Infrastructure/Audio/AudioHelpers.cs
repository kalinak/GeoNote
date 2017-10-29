using Infrastructure.IsolatedStorage;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Audio
{
    public static class AudioHelpers
    {
        public static bool IsRecording { get; set; }
        public static string RecordingFileName { get; set; }
        private static SoundEffectInstance effectInstance;
        readonly static string filesPrefix = "GeoNote_";
        private static Microphone microphone;
        private static MemoryStream stream = new MemoryStream();
        private static byte[] buffer;

        readonly static object fileLock = new object();
        static AudioHelpers()
        {
            if (microphone == null)
            {
                microphone = Microphone.Default;

                microphone.BufferDuration = TimeSpan.FromSeconds(1);
                microphone.BufferReady += HandleBufferReady;                                
            }

            int sampleSizeInBytes = microphone.GetSampleSizeInBytes(microphone.BufferDuration);
            buffer = new byte[sampleSizeInBytes];
        }

       public static void StartRecording()
        {
            stream = new MemoryStream();
            microphone.Start();
            IsRecording = true;
        }

        public static void StopRecording()
        {
            if (microphone.State != MicrophoneState.Stopped)
            {
                IsRecording = false;
            }
        }

        public static void PlayCurrentRecording()
        {
            Play(RecordingFileName);
        }

        public static void Play(string recordingName)
        {
            byte[] audioBytes = IsolatedStorageHelpers.ReadFile(recordingName);

            if (audioBytes == null)
                return;

            if (audioBytes == null || audioBytes.Length == 0
                || effectInstance != null
                && effectInstance.State == SoundState.Playing)
            {
                return;
            }

            var soundEffect = new SoundEffect(audioBytes, microphone.SampleRate, AudioChannels.Mono);
            if (effectInstance != null)
            {
                effectInstance.Dispose();
            }
            effectInstance = soundEffect.CreateInstance();
            effectInstance.Play();
        }

        private static void HandleBufferReady(object sender, EventArgs e)
        {
            microphone.GetData(buffer);
            stream.Write(buffer, 0, buffer.Length);

            if (!IsRecording)
            {
                stream.Flush();
                microphone.Stop();
                RecordingFileName = filesPrefix + Guid.NewGuid();
                IsolatedStorageHelpers.WriteFile(stream, RecordingFileName);
            }
        }
    }
}
