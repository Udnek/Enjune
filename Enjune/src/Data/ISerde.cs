namespace Enjune.Data;

public interface ISerde
{
    string Serialize(DataObject dataObject);
    DataObject? Deserialize(string data, out Error? error);
}