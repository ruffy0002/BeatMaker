using UnityEngine;
using System.Collections;
using System.Threading;

public abstract class DecoderImporter : AudioImporter
{
    protected AudioInfo info { get; private set; }

    private void FillBuffer(float[] buffer)
    {
        int index = 0;
        while (index < buffer.Length)
        {
            int read = GetSamples(buffer, index, Mathf.Min(buffer.Length - index, 4096));
            index += read;                       
        }
    }

    IEnumerator LoadAudioClip(int samples)
    {
        samples = Mathf.Clamp(samples, 0, info.lengthSamples);
        float[] buffer = new float[samples];

        Thread bufferThread = new Thread(() => FillBuffer(buffer));
        bufferThread.Start();

        while (bufferThread.IsAlive)
        {
            OnProgress(GetProgress());
            yield return null;
        }

        AudioClip audioClip = AudioClip.Create("", info.lengthSamples / info.channels, info.channels, info.sampleRate, false);
        audioClip.SetData(buffer, 0);
        OnLoaded(audioClip);
    }

    protected override IEnumerator Load(string uri)
    {
        yield return StartCoroutine(Initialize(uri));

        if (isError)
            yield break;

        info = GetInfo();

        yield return StartCoroutine(LoadAudioClip(info.lengthSamples));

        OnProgress(1);

        Cleanup();
    }

    protected override IEnumerator LoadStreaming(string uri, int initialLength)
    {
        yield return StartCoroutine(Initialize(uri));

        if (isError)
            yield break;

        info = GetInfo();
        
        int loadedIndex = initialLength * info.sampleRate * info.channels;
        loadedIndex = Mathf.Clamp(loadedIndex, 44100, info.lengthSamples);

        yield return StartCoroutine(LoadAudioClip(loadedIndex));

        int bufferSize = info.sampleRate * info.channels / 10;
        float[] buffer = new float[bufferSize];
        
        int index = loadedIndex;
        while (index < info.lengthSamples)
        {
            int read = GetSamples(buffer, 0, bufferSize);
            audioClip.SetData(buffer, index / info.channels);
            index += read;
            OnProgress(GetProgress());
            yield return null;
        }

        OnProgress(1);

        Cleanup();
    }

    protected abstract IEnumerator Initialize(string uri);

    protected abstract int GetSamples(float[] buffer, int offset, int count);

    protected abstract float GetProgress();

    protected virtual void Cleanup()
    {

    }

    protected abstract AudioInfo GetInfo();

    protected class AudioInfo
    {
        public int lengthSamples { get; private set; }
        public int sampleRate { get; private set; }
        public int channels { get; private set; }

        public AudioInfo(int lengthSamples, int sampleRate, int channels)
        {
            this.lengthSamples = lengthSamples;
            this.sampleRate = sampleRate;
            this.channels = channels;
        }
    }
}
