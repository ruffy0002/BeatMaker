using System;
using System.Collections;
using UnityEngine;
using System.IO;

public abstract class AudioImporter : MonoBehaviour
{
    /// <summary>
    /// Occurs when the file has been loaded into an AudioClip.
    /// </summary>
    public event Action<AudioClip> Loaded;
    
    /// <summary>
    /// Occurs when <see cref="progress"/> is updated.
    /// </summary>
    public event Action<float> Progress;

    /// <summary>
    /// Occurs when there has been an error while importing the file.
    /// </summary>
    public event Action<string> Error;

    /// <summary>
    /// The path or url for the file that is being imported.
    /// </summary>
    public string uri { get; private set; }

    /// <summary>
    /// The loaded AudioClip.
    /// </summary>
    public AudioClip audioClip { get; private set; }

    /// <summary>
    /// Import progress ranging from 0-1
    /// </summary>
    public float progress { get; private set; }

    /// <summary>
    /// Is the file loaded into an AudioClip?
    /// </summary>
    public bool isLoaded { get; private set; }

    /// <summary>
    /// Was there an error while importing the file?
    /// </summary>
    public bool isError { get; private set; }

    /// <summary>
    /// An error message if there was an error while importing the file.
    /// </summary>
    public string error { get; private set; }

#if UNITY_5_3_OR_NEWER

    private ImportOperation operation;

    protected AudioImporter()
    {
        operation = new ImportOperation(this);
    }

    /// <summary>
    /// Import a file.
    /// </summary>
    /// <param name="uri">The file's path or url.</param>
    /// <returns>ImportOperation that can be used in a coroutine.</returns>
    public ImportOperation Import(string uri)
    {
        Cleanup();

        this.uri = GetUri(uri);

        StartCoroutine(Load(this.uri));

        return operation;
    }

    /// <summary>
    /// Import a file gradually with an optional initial length.
    /// </summary>
    /// <param name="uri">The file's path or url.</param>
    /// <param name="initialLength">The initial length in seconds.</param>
    /// <returns>ImportOperation that can be used in a coroutine.</returns>
    public ImportOperation ImportStreaming(string uri, int initialLength = 0)
    {
        Cleanup();

        this.uri = GetUri(uri);

        initialLength = Mathf.Max(1, initialLength);
        StartCoroutine(LoadStreaming(this.uri, initialLength));

        return operation;
    }

#else

    /// <summary>
    /// Import a file.
    /// </summary>
    /// <param name="uri">The file's path or url.</param>
    public void Import(string uri)
    {
        Cleanup();

        this.uri = GetUri(uri);

        StartCoroutine(Load(this.uri));
    }

    /// <summary>
    /// Import a file gradually with an optional initial length.
    /// </summary>
    /// <param name="uri">The file's path or url.</param>
    /// <param name="initialLength">The initial length in seconds.</param>
    public void ImportStreaming(string uri, int initialLength = 0)
    {
        Cleanup();

        this.uri = GetUri(uri);

        initialLength = Mathf.Max(1, initialLength);
        StartCoroutine(LoadStreaming(this.uri, initialLength));
    }
#endif

    protected virtual IEnumerator Load(string uri)
    {
        yield return null;
    }

    protected virtual IEnumerator LoadStreaming(string uri, int initialLength)
    {
        yield return null;
    }      

    protected virtual string GetName()
    {
        return Path.GetFileNameWithoutExtension(uri);
    }

    private string GetUri(string uri)
    {
        if (uri.StartsWith("file://") || uri.StartsWith("http://") || uri.StartsWith("https://"))
            return uri;

        return "file://" + uri;
    }

    private void Cleanup()
    {
        StopAllCoroutines();

        if (audioClip != null)
            Destroy(audioClip, 2);

        uri = null;
        audioClip = null;
        isLoaded = false;
        isError = false;
        error = null;
        progress = 0;
    }

    protected void OnLoaded(AudioClip audioClip)
    {
        audioClip.name = GetName();
        this.audioClip = audioClip;
        
        isLoaded = true;

        if (Loaded != null)
            Loaded.Invoke(audioClip);
    }

    protected void OnProgress(float progress)
    {
        if (this.progress == progress)
            return;

        this.progress = progress;

        if (Progress != null)
            Progress(progress);
    }

    protected void OnError(string error)
    {
        isError = true;
        this.error = error;
        Debug.LogError(error);

        if (Error != null)
            Error(error);
    }    
}

#if UNITY_5_3_OR_NEWER
public class ImportOperation : CustomYieldInstruction
{
    public override bool keepWaiting
    {
        get
        {
            return !importer.isLoaded && !importer.isError;
        }
    }

    public AudioImporter importer { get; private set; }

    public ImportOperation(AudioImporter importer)
    {
        this.importer = importer;
    }
}
#endif
