using Enjune.Data.Codec;
using Enjune.Data.Json;
using Enjune.File;
using Enjune.Misc;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit.Abstractions;

namespace Tests;

public class CodecTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    private Error? Test<T>(T obj, ICodec<T> codec, Func<T, string> toStr)
    {
        _output.WriteLine("------------------");
        _output.WriteLine($"initial: {toStr(obj)}");
        var json = JsonSerde.Tight.Serialize(codec.Encode(obj));
        _output.WriteLine($"serialized: {json}");
        var dataObject = JsonSerde.Tight.Deserialize(json, out var error);
        if (dataObject is null)
        {
            return error;
        }
        var decoded = codec.Decode(dataObject);
        if (decoded.Error != null)
        {
            return decoded.Error;
        }
        var value = decoded.GetOrThrow();
        _output.WriteLine($"decoded: {toStr(value)}");
        
        Assert.Equal(obj, value);
        return null;
    }
    
    [Fact]
    public void BaseTests()
    {
        Assert.Null(
            Test(
                new Vector3(1, -5, 3.4f), 
                Codecs.Vector3, 
                o => o.ToString()
            ));
        
        Assert.Null(
            Test(
                new Vector3(1, -5, 3.4f), 
                Codecs.NullableOfStruct(Codecs.Vector3), 
                o => o?.ToString() ?? "null"
            ));
        
        Assert.Null(
            Test(
                [new Vector3(0, 0, 1), null, new Vector3(1, -5, 3.4f)], 
                Codecs.ArrayOf(Codecs.NullableOfStruct(Codecs.Vector3)), 
                o => o.ContentToString()
                ));
    }
}