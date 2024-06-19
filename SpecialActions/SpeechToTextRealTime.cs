using NAudio.CoreAudioApi;
using NAudio.Wave;
using Newtonsoft.Json; 
using skill_composer.Models; 
using System.Diagnostics; 
using System.Net.WebSockets;
using System.Text; 
using Task = System.Threading.Tasks.Task; 
using static skill_composer.Helper.AssemblyAiHelper;
using MathNet.Numerics.IntegralTransforms;
using System.Numerics;

namespace skill_composer.SpecialActions
{
    public class SpeechToTextRealTime : ISpecialAction
    {
        public async Task<Models.Task> ExecuteAsync(Models.Task task, Skill selectedSkill, Settings settings)
        {
            var aiToken = GetAssemblyAiWebsocketTemporaryToken(settings, 360000).Result;
            var assemblyAiWebSocket = new ClientWebSocket();
            var serverUri = new Uri($"wss://api.assemblyai.com/v2/realtime/ws?sample_rate=16000&token={aiToken}&encoding=pcm_s16le&language_code=es");

            await assemblyAiWebSocket.ConnectAsync(serverUri, CancellationToken.None);

            // Using WasapiLoopbackCapture to capture audio from playback device
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            Console.WriteLine("Available Playback Devices:");
            foreach (var device in devices)
            {
                Console.WriteLine($"Device ID: {device.ID}, Device Name: {device.FriendlyName}");
            }

            var defaultPlaybackDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            Console.WriteLine($"Default Playback Device: {defaultPlaybackDevice.FriendlyName}");

            using var capture = new WasapiLoopbackCapture(defaultPlaybackDevice);

            var ffmpegStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = "-f f32le -ar 48000 -ac 2 -i pipe:0 -f s16le -ar 16000 -ac 1 -",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var ffmpegProcess = new Process { StartInfo = ffmpegStartInfo };
            ffmpegProcess.Start();

            capture.DataAvailable += async (sender, e) =>
            {
                // Write the raw capture data to FFmpeg's stdin
                await ffmpegProcess.StandardInput.BaseStream.WriteAsync(e.Buffer.AsMemory(0, e.BytesRecorded));
                await ffmpegProcess.StandardInput.BaseStream.FlushAsync();

            };

            capture.RecordingStopped += (sender, e) =>
            {
                ffmpegProcess.StandardInput.Close(); // Signals FFmpeg to finish
            };

            var readBuffer = new byte[4096];
            var readTask = Task.Run(async () =>
            {
                int count;
                while ((count = await ffmpegProcess.StandardOutput.BaseStream.ReadAsync(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    await assemblyAiWebSocket.SendAsync(new ArraySegment<byte>(readBuffer, 0, count), WebSocketMessageType.Binary, true, CancellationToken.None);
                }
            });

            capture.StartRecording();

            await ReceiveMessages(assemblyAiWebSocket);

            Console.WriteLine("Recording... Press Enter to stop.");
            Console.ReadLine();

            capture.StopRecording();
            await readTask; // Ensure we finish reading FFmpeg's output

            await assemblyAiWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

            //Console.WriteLine("Playback Devices:");
            //DisplayPlaybackDeviceAudio();

            return task;
        }
        private const int FFTLength = 1024; // Power of two for FFT
        private static float[] fftBuffer = new float[FFTLength];
        private static Complex[] fftComplex = new Complex[FFTLength];
        private static int fftPos = 0;
        private static string[] barChars = { " ", ".", ":", "-", "=", "+", "*", "#", "%" };
        private static string audioDeviceName; // Store the audio device name

        public static void DisplayPlaybackDeviceAudio()
        {
            // Use MMDeviceEnumerator to get the default playback device
            var enumerator = new MMDeviceEnumerator();
            var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            audioDeviceName = defaultDevice.FriendlyName; // Store the friendly name

            using var capture = new WasapiLoopbackCapture(defaultDevice); // Initialize capture with the default device
            capture.DataAvailable += Capture_DataAvailable;

            Console.WriteLine("AudioDevices:");
            capture.StartRecording();
            Console.ReadLine();
            capture.StopRecording();
        }

        private static void Capture_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (e.BytesRecorded == 0) return;

            // Assuming 32 bit IEEE float audio format
            for (int index = 0; index < e.BytesRecorded; index += 4)
            {
                if (fftPos < FFTLength)
                {
                    float sample = BitConverter.ToSingle(e.Buffer, index);
                    fftBuffer[fftPos++] = sample;

                    if (fftPos == FFTLength)
                    {
                        ProcessFFT();
                        fftPos = 0;
                    }
                }
            }
        }

        private static void ProcessFFT()
        {
            // Convert real numbers to complex for FFT
            for (int i = 0; i < FFTLength; i++)
            {
                fftComplex[i] = new Complex(fftBuffer[i], 0);
            }

            Fourier.Forward(fftComplex, FourierOptions.Matlab); // Perform FFT

            // Map FFT output to bars
            string visualRepresentation = MapFrequenciesToBars();

            // Include the audio device name with the visual representation
            Console.Write($"\r{audioDeviceName}: {visualRepresentation}");
        }

        private static string MapFrequenciesToBars()
        {
            int bandCount = 8; // Divide the spectrum into 8 bands
            string[] volumeBars = new string[bandCount];

            for (int i = 0; i < bandCount; i++)
            {
                double avgMagnitude = 0;
                int start = i * (FFTLength / 2) / bandCount; // Only use first half of FFT results
                int end = (i + 1) * (FFTLength / 2) / bandCount;
                for (int j = start; j < end; j++)
                {
                    avgMagnitude += fftComplex[j].Magnitude;
                }
                avgMagnitude /= (end - start);

                // Map avgMagnitude to bar character
                int charIndex = (int)Math.Floor(avgMagnitude * 10); // Simplified scaling
                charIndex = Math.Min(charIndex, barChars.Length - 1);
                volumeBars[i] = barChars[charIndex];
            }

            return string.Join(" ", volumeBars);
        }

        private static async Task ReceiveMessages(ClientWebSocket assemblyAiWebSocket)
        {
            var buffer = new byte[1024 * 4];

            try
            {
                while (assemblyAiWebSocket.State == WebSocketState.Open)
                {
                    var result = await assemblyAiWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("WebSocket closed by the server.");
                        break;
                    }
                    else
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        var transcription = JsonConvert.DeserializeObject<AssemblyAiTranscription>(message);

                        if (transcription.message_type == "FinalTranscript")
                        {
                            //Console.WriteLine(JsonConvert.SerializeObject(transcription));
                            Console.WriteLine($"{transcription.text}");
                        }
                    }
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket exception: {ex.Message}");
            }
            finally
            {
                if (assemblyAiWebSocket.State != WebSocketState.Closed)
                {
                    await assemblyAiWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
            }
        }

    }
}
