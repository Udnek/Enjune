using Enjune.Data.Codec;
using Enjune.Data.Json;
using Enjune.Misc;
using Xunit.Abstractions;

namespace Tests;

public class CodecTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    private Error? Test<T>(T obj, ICodec<T> codec, Func<T, string> toStr)
    {
        _output.WriteLine("------------------");
        _output.WriteLine($"initial: {toStr(obj)}");
        var encodeResult = codec.Encode(obj);
        if (encodeResult.Error != null)
            return encodeResult.Error;
        var json = JsonSerde.Tight.Serialize(encodeResult.GetOrThrow());
        _output.WriteLine($"serialized: {json}");
        var dataObject = JsonSerde.Tight.Deserialize(json, out var error);
        if (dataObject is null)
        {
            return error;
        }
        var decoded = codec.Decode(dataObject);
        return decoded.Map<Error?>(
            value =>
            {
                _output.WriteLine($"decoded: {toStr(value)}");
                Assert.Equal(obj, value);
                return null;
            },
            err => err);
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