using System.Globalization;
using System.Text;
using Enjune.Misc;
using static Enjune.Data.Json.JsonLexer.TokenType;

namespace Enjune.Data.Json;

public class JsonSerde : ISerde
{
    public static readonly JsonSerde Tight = new(0);
    public static readonly JsonSerde Indent4 = new(4);
    public static JsonSerde Indent(int indent) => new(indent);

    private readonly int _indent;
    
    private JsonSerde(int indent)
    {
        _indent = indent;
    }

    public DataObject? Deserialize(string data, out Error? error)
    {
        var lexer = new JsonLexer(data);

        lexer.ConsumeSpaces();
        var res = Parse(out error);
        if (error != null) return null;
        
        lexer.ConsumeSpaces();
        
        error = lexer.ConsumeExpected(End);
        if (error != null) return null;
        
        return res;
        

        DataObject? Parse(out Error? error)
        {
            var token = lexer.CurrentToken;
            switch (token)
            {
                // {
                case LeftBrace:
                    return ParseMap(out error);
                // [
                case LeftBracket:
                    return ParseArray(out error);
                // "
                case Quote:
                    return ParseString(out error);
                // anything else
                case RegularChar:
                    return ParseNumberOrBoolOrNull(out error);
                default:
                    error = lexer.UnexpectedTokenError(LeftBrace, LeftBracket, Quote, RegularChar);
                    return null;
            }
        }
        
        DataObject? ParseNumberOrBoolOrNull(out Error? error)
        {
            var builder = new StringBuilder();
            builder.Append(lexer.CurrentChar);
            while (true)
            {
                var token = lexer.NextToken();
                if (token != RegularChar) break;
                builder.Append(lexer.CurrentChar);
            }
            var str = builder.ToString();
            error = null;
            switch (str)
            {
                case "null":
                    return DataObject.Null;
                case "true":
                    return DataObject.Boolean.True;
                case "false":
                    return DataObject.Boolean.False; 
                default:
                    var dec = str.ParseDecimalOrNull();
                    if (dec is null)
                    {
                        error = $"can not parse number: {str} in '{lexer.HighlightCharWithContext()}'";
                        return null;
                    }
                    return new DataObject.Number(dec.Value);
            }
        }
        
        DataObject.String? ParseString(out Error? error)
        {
            error = lexer.ConsumeExpected(Quote);
            if (error != null) return null;
            
            var builder = new StringBuilder();
            builder.Append(lexer.CurrentChar);
            while (true)
            {
                var token = lexer.NextToken();
                if (token == End)
                {
                    error = lexer.UnexpectedTokenError();
                    return null;
                }
                if (token == Quote)
                {
                    lexer.ConsumeAny();
                    break;
                }
                builder.Append(lexer.CurrentChar);
            }
            error = null;
            return builder.ToString();
        }
        
        DataObject.Map? ParseMap(out Error? error)
        {
            error = lexer.ConsumeExpected(LeftBrace);
            if (error != null) return null;
            
            var map = new Dictionary<string, DataObject>();
            while (true)
            {
                lexer.ConsumeSpaces();
                if (lexer.CurrentToken == RightBrace)
                {
                    lexer.ConsumeAny();
                    break;
                }
                var key = ParseString(out error);
                if (key == null) return null;
                lexer.ConsumeSpaces();

                error = lexer.ConsumeExpected(Colon);
                if (error != null) return null;
                lexer.ConsumeSpaces();
                
                var value = Parse(out error);
                if (value == null) return null;
                map[key.Val] = value;
                lexer.ConsumeSpaces();

                if (lexer.CurrentToken == Comma)
                    lexer.ConsumeAny();
                else if (lexer.CurrentToken != RightBrace) // must end dict
                {
                    error = lexer.UnexpectedTokenError(RightBrace);
                    return null;
                } 
            }

            return new DataObject.Map(map);
        }
        
        DataObject.Array? ParseArray(out Error? error)
        {
            error = lexer.ConsumeExpected(LeftBracket);
            if (error != null) return null;
            
            var list = new List<DataObject>();
            while (true)
            {
                lexer.ConsumeSpaces();
                if (lexer.CurrentToken == RightBracket)
                {
                    lexer.ConsumeAny();
                    break;
                }
                var value = Parse(out error);
                if (value == null) return null;
                list.Add(value);
                lexer.ConsumeSpaces();

                if (lexer.CurrentToken == Comma)
                    lexer.ConsumeAny();
                else if (lexer.CurrentToken != RightBracket) // must end array
                {
                    error = lexer.UnexpectedTokenError(RightBracket);
                    return null;
                } 
            }

            return new DataObject.Array(list.ToArray());
        }
    }

    public string Serialize(DataObject dataObject) => Serialize(new StringBuilder(), dataObject, 0).ToString();

    public StringBuilder Serialize(StringBuilder builder, DataObject dataObject, int depth)
    {
        switch (dataObject)
        {
            case DataObject.NullVal:
                return builder.Append("null");
            case DataObject.Number flatC:
                return builder.Append(flatC.Decimal.ToString(CultureInfo.InvariantCulture));
            case DataObject.String strC:
                return builder.Append('\"' + strC.Val + '\"');
            case DataObject.Boolean boolC:
                return builder.Append(boolC.Val ? "true" : "false"); 
            case DataObject.Array arrayC:
            {
                if (arrayC.Val.Length == 0) return builder.Append("[]");
                builder.Append('[');
                var first = true;
                for (var i = 0; i < arrayC.Val.Length; i++)
                {
                    if (!first) 
                        builder.Append(", ");
                    if (_indent > 0)
                    {
                        builder.Append('\n');
                        builder.Append(new string(' ', _indent * (depth+1)));
                    }
                        
                    Serialize(builder, arrayC.Val[i], depth+1);
                    first = false;
                }
                if (_indent > 0)
                {
                    builder.Append('\n');
                    builder.Append(new string(' ', _indent * depth));
                }
                return builder.Append(']');
            }
            case DataObject.Map mapC:
            {
                if (mapC.Val.Count == 0) return builder.Append("{}");
                builder.Append('{');
                var first = true;
                foreach (var pair in mapC.Val)
                {
                    if (!first)
                        builder.Append(", ");
                    if (_indent > 0)
                    {
                        builder.Append('\n');
                        builder.Append(new string(' ', _indent * (depth + 1)));
                    }
                    builder.Append('\"');
                    builder.Append(pair.Key);
                    builder.Append('\"');
                    builder.Append(": ");
                    Serialize(builder, pair.Value, depth+1);
                    first = false;
                }
                if (_indent > 0)
                {
                    builder.Append('\n');
                    builder.Append(new string(' ', _indent * depth));
                }
                return builder.Append('}');
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(dataObject));
        }
    }
}