// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.DigitReader;

using System.IO;
using System.Numerics.Tensors;
using Microsoft.ML.OnnxRuntime;

public sealed class DigitClassifierOnnx(string modelName)
    : IDisposable,
        IDigitClassifier
{
    private readonly Lazy<InferenceSession> session = new(() =>
    {
        var path = GetData($"{modelName}.onnx");
        return new InferenceSession(path);
    });

    private InferenceSession Session => this.session.Value;

    public int PredictPng(string path)
    {
        using Mat src = Cv2.ImRead(path, ImreadModes.Grayscale);
        return this.PredictImage(src) ?? -1;
    }

    public int? PredictImage(Mat grayImage)
    {
        var inputSize = this.Session.InputMetadata.First().Value.Dimensions;
        var data = GetData(grayImage, inputSize);

        using var inputOrtValue = OrtValue.CreateTensorValueFromMemory(
            data,
            [.. inputSize.Select(x => (long)x)]
        );

        var inputs = new Dictionary<string, OrtValue>
        {
            { this.Session.InputNames[0], inputOrtValue },
        };

        using RunOptions ro = new();
        using var output = this.Session.Run(
            ro,
            inputs,
            [this.Session.OutputNames[0]]
        );

        var output_0 = output[0];
        var outputData = output_0.GetTensorDataAsSpan<float>();

        var result = TensorPrimitives.IndexOfMax(outputData);
        return result;
    }

    public void Dispose()
    {
        if (this.session.IsValueCreated)
        {
            this.session.Value.Dispose();
        }
    }

    private static string GetData(string name)
    {
        var dir = AppContext.BaseDirectory;
        var result = Path.Combine(dir, name);
        return !File.Exists(result)
            ? throw new FileNotFoundException($"File not found: {result}")
            : result;
    }

    private static float[] GetData(Mat grayImage, int[] dimensions)
    {
        using Mat convertedImage = new();
        grayImage.ConvertTo(convertedImage, MatType.CV_32FC1, 1.0 / 255.0);
        var resizeImage = convertedImage;

        using Mat src28 = resizeImage.Resize(
            new Size(dimensions[^2], dimensions[^1])
        );

        if (!src28.GetArray<float>(out var data))
        {
            throw new ArgumentException(
                $"Failed to get float array out of {src28}."
            );
        }

        int numChannels = dimensions[1];
        return numChannels == 1 ? data : Repeat(data, numChannels);
    }

    private static float[] Repeat(float[] data, int numChannels)
    {
        return Enumerable
            .Repeat(data, numChannels)
            .SelectMany(x => x)
            .ToArray();
    }
}
