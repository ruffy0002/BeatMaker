using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;
using NAudio.Wave;

/// <summary>
/// An Importer that uses NAudio for importing audio files.
/// </summary>
[AddComponentMenu("Audio/NAudio Importer")]
public class NAudioImporter : DecoderImporter
{
    private Mp3FileReader reader;
    private ISampleProvider sampleProvider;

    protected override AudioInfo GetInfo()
    {
        WaveFormat format = reader.WaveFormat;
        int lengthSamples = (int)reader.Length / (format.BitsPerSample / 8);
        return new AudioInfo(lengthSamples, format.SampleRate, format.Channels);
    }

    protected override float GetProgress()
    {
        return (float)reader.Position / reader.Length;
    }

    protected override int GetSamples(float[] buffer, int offset, int count)
    {
        return sampleProvider.Read(buffer, offset, count);
    }

    protected override IEnumerator Initialize(string uri)
    {
        Cleanup();

        Stream stream;

#if UNITY_5_4_OR_NEWER
        using (var request = UnityEngine.Networking.UnityWebRequest.Get(uri))
        {
            yield return request.SendWebRequest();
            stream = new MemoryStream(request.downloadHandler.data);
        }
#else
        using (var www = new WWW(uri))
        {
            yield return www;            
            stream = new MemoryStream(www.bytes);
        }
#endif

        Thread loadThread = new Thread(() => LoadReader(stream));
        loadThread.Start();

        while (loadThread.IsAlive)
            yield return null;

        if (isError)
            yield break;

        sampleProvider = reader.ToSampleProvider();
    }

    protected override void Cleanup()
    {
        if (reader != null)
            reader.Dispose();

        reader = null;
        sampleProvider = null;
    }

    private void LoadReader(Stream stream)
    {
        try
        {
            reader = new Mp3FileReader(stream);
        }
        catch (System.Exception e)
        {
            OnError(e.Message);
        }
    }
}
