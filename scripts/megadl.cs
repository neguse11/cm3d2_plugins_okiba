// http://hacktracking.blogspot.ca/2013/07/download-mega-files-from-command-line.html
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;

class Program
{
    static readonly int retryMax = 10;
    static readonly int retrySleep = 10000; // in milliseconds

#if DEBUG
    static bool bLogEnable = true;
#else
    static bool bLogEnable = false;
#endif

    static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: megadl <mega.nz URL> <output filename>");
            return;
        }
        GetMegaFile(args[1], args[0]);
    }

    static byte[] GetMegaFile(string outFilePath, string megaUrl)
    {
        byte[] file = GetMegaFile(megaUrl);
        if (file != null)
        {
            File.WriteAllBytes(outFilePath, file);
        }
        return file;
    }

    public static byte[] GetMegaFile(string megaUrl)
    {
        byte[] result = null;

        string id;
        string key;
        {
            string[] ss = megaUrl.Split('!');
            if (ss.Length < 2)
            {
                Console.WriteLine("Error : Bad URL (megaUrl={0})", megaUrl);
                return result;
            }
            id = ss[1];
            key = ss[2];
        }
        Log("megaUrl={0}, id={1}, key={2}", megaUrl, id, key);

        byte[] aesKey = new byte[16];
        byte[] aesIv = new byte[16];    // counter
        {
            string b64Key = key.Replace("-", "+").Replace("_", "+").PadRight(((key.Length + 3) / 4) * 4, '=');
            Log("b64Key={0}", b64Key);
            byte[] binKey = Convert.FromBase64String(b64Key);
            if (binKey.Length != 32)
            {
                return result;
            }
            for (int i = 0; i < aesKey.Length; i++)
            {
                aesKey[i] = (byte)(binKey[i] ^ binKey[i + 16]);
            }
            for (int i = 0; i < aesIv.Length; i++)
            {
                aesIv[i] = i < 8 ? binKey[i + 16] : (byte)0;
            }
        }
        Log("aesKey = {0}", ByteArrayToString(aesKey));
        Log("aesIv  = {0}", ByteArrayToString(aesIv));

        string encUrl = null;
        {
            string postResult = "";
            string postUri = "https://eu.api.mega.co.nz/cs";
            string postParams = string.Format(@"[{{""a"":""g"",""g"":1,""p"":""{0}""}}]", id);
            WebClientWithRetry(retryMax, (webClient) =>
            {
                webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                postResult = webClient.UploadString(postUri, postParams);
                return !string.IsNullOrEmpty(postResult);
            });
            Match match = Regex.Match(postResult, @"""g"":""([^""]*)""");
            if (match.Groups.Count != 2)
            {
                return result;
            }
            encUrl = match.Groups[1].Value;
        }
        Log("encUrl = {0}", encUrl);

        byte[] encFile = null;
        WebClientWithRetry(retryMax, (webClient) =>
        {
            encFile = webClient.DownloadData(encUrl);
            return encFile != null && encFile.Length > 0;
        });
        if (encFile == null)
        {
            return result;
        }

        {
            byte[] paddedEncFile = new byte[((encFile.Length + 15) / 16) * 16];
            Log("encFile.Length={0}", encFile.Length);
            Log("paddedEncFile.Length={0}", paddedEncFile.Length);
            encFile.CopyTo(paddedEncFile, 0);
            for (int i = encFile.Length; i < paddedEncFile.Length; i++)
            {
                paddedEncFile[i] = 0;
            }

            var aes = new Aes128CounterMode(aesIv);
            var dec = aes.CreateDecryptor(aesKey, null);

            byte[] paddedDecFile = new byte[paddedEncFile.Length];
            dec.TransformBlock(paddedEncFile, 0, paddedEncFile.Length, paddedDecFile, 0);
            Array.Resize(ref paddedDecFile, encFile.Length);
            result = paddedDecFile;
        }
        return result;
    }

    static void WebClientWithRetry(int maxRetryCount, Func<WebClient, bool> func)
    {
        using (WebClient webClient = new WebClient())
        {
            bool result = false;
            for (int i = 0; !result && i < maxRetryCount; i++)
            {
                if (i != 0)
                {
                    Console.WriteLine("Retrying (#{0}/{1})...", i, maxRetryCount);
                    Thread.Sleep(retrySleep);
                }
                try
                {
                    result = func(webClient);
                }
                catch (WebException)
                {
                    Console.WriteLine("Failed(#{0})", i + 1);
                }
            }
        }
    }

    static void Log(string format, params object[] args)
    {
        if (bLogEnable)
        {
            string s = string.Format(format, args);
            Console.WriteLine(s);
        }
    }

    static string ByteArrayToString(byte[] ba)
    {
        string hex = BitConverter.ToString(ba);
        return hex.Replace("-", "");
    }
}


// https://gist.github.com/hanswolff/8809275
public class Aes128CounterMode : SymmetricAlgorithm
{
    private readonly byte[] _counter;
    private readonly AesManaged _aes;

    public Aes128CounterMode(byte[] counter)
    {
        if (counter == null) throw new ArgumentNullException("counter");
        if (counter.Length != 16)
            throw new ArgumentException(String.Format("Counter size must be same as block size (actual: {0}, expected: {1})",
                counter.Length, 16));

        _aes = new AesManaged
        {
            Mode = CipherMode.ECB,
            Padding = PaddingMode.None
        };

        _counter = counter;
    }

    public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] ignoredParameter)
    {
        return new CounterModeCryptoTransform(_aes, rgbKey, _counter);
    }

    public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] ignoredParameter)
    {
        return new CounterModeCryptoTransform(_aes, rgbKey, _counter);
    }

    public override void GenerateKey()
    {
        _aes.GenerateKey();
    }

    public override void GenerateIV()
    {
        // IV not needed in Counter Mode
    }
}

public class CounterModeCryptoTransform : ICryptoTransform
{
    private readonly byte[] _counter;
    private readonly ICryptoTransform _counterEncryptor;
    private readonly Queue<byte> _xorMask = new Queue<byte>();
    private readonly SymmetricAlgorithm _symmetricAlgorithm;

    public CounterModeCryptoTransform(SymmetricAlgorithm symmetricAlgorithm, byte[] key, byte[] counter)
    {
        if (symmetricAlgorithm == null) throw new ArgumentNullException("symmetricAlgorithm");
        if (key == null) throw new ArgumentNullException("key");
        if (counter == null) throw new ArgumentNullException("counter");
        if (counter.Length != symmetricAlgorithm.BlockSize / 8)
            throw new ArgumentException(String.Format("Counter size must be same as block size (actual: {0}, expected: {1})",
                counter.Length, symmetricAlgorithm.BlockSize / 8));

        _symmetricAlgorithm = symmetricAlgorithm;
        _counter = counter;

        var zeroIv = new byte[_symmetricAlgorithm.BlockSize / 8];
        _counterEncryptor = symmetricAlgorithm.CreateEncryptor(key, zeroIv);
    }

    public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
    {
        var output = new byte[inputCount];
        TransformBlock(inputBuffer, inputOffset, inputCount, output, 0);
        return output;
    }

    public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
    {
        for (var i = 0; i < inputCount; i++)
        {
            if (NeedMoreXorMaskBytes()) EncryptCounterThenIncrement();

            var mask = _xorMask.Dequeue();
            outputBuffer[outputOffset + i] = (byte)(inputBuffer[inputOffset + i] ^ mask);
        }

        return inputCount;
    }

    private bool NeedMoreXorMaskBytes()
    {
        return _xorMask.Count == 0;
    }

    private void EncryptCounterThenIncrement()
    {
        var counterModeBlock = new byte[_symmetricAlgorithm.BlockSize / 8];

        _counterEncryptor.TransformBlock(_counter, 0, _counter.Length, counterModeBlock, 0);
        IncrementCounter();

        foreach (var b in counterModeBlock)
        {
            _xorMask.Enqueue(b);
        }
    }

    private void IncrementCounter()
    {
        for (var i = _counter.Length - 1; i >= 0; i--)
        {
            if (++_counter[i] != 0)
                break;
        }
    }

    public int InputBlockSize { get { return _symmetricAlgorithm.BlockSize / 8; } }
    public int OutputBlockSize { get { return _symmetricAlgorithm.BlockSize / 8; } }
    public bool CanTransformMultipleBlocks { get { return true; } }
    public bool CanReuseTransform { get { return false; } }

    public void Dispose()
    {
    }
}
