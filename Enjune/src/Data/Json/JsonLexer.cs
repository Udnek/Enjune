using System.Diagnostics.Contracts;
using Enjune.Misc;

namespace Enjune.Data.Json;

public class JsonLexer
{
    private readonly string _inputString;
    private int _pointer = -1;

    public TokenType CurrentToken { get; private set; } = TokenType.End;
    
    public char CurrentChar
    {
        get
        {
            if (_pointer < 0 || _pointer >= _inputString.Length) return ' ';
            return _inputString[_pointer];
        }
    }
    
    public JsonLexer(string inputString)
    {
        _inputString = inputString;
        ConsumeAny();
    }

    public String HighlightCharWithContext(int range = 10)
        => "... " +
           _inputString.SafeSubstringFromTo(_pointer - range, _pointer) +
           ">" + CurrentChar + "<" +
           _inputString.SafeSubstringFromTo(_pointer + 1, _pointer + 1 + range) +
           " ...";

    public Error UnexpectedTokenError(params TokenType[] expected) 
        => $"expected token {expected.ContentToString()}, but got {CurrentToken} in '{HighlightCharWithContext()}'";

    public Error UnexpectedTokenError() 
        => $"unexpected token {CurrentToken} in '{HighlightCharWithContext()}'";
    

    public void ConsumeSpaces()
    {
        while (CurrentToken == TokenType.Space) ConsumeAny();
    }
    
    public Error? ConsumeExpected(TokenType expected)
    {
        if (CurrentToken != expected)
            return UnexpectedTokenError(expected);
        ConsumeAny();
        return null;
    }
    
    public void ConsumeAny()
    {
        if (_pointer >= _inputString.Length-1)
        {
            CurrentToken = TokenType.End;
        }
        else
        {
            _pointer++;
            CurrentToken = GetType(_inputString[_pointer]);
        }
    }
    
    public TokenType NextToken()
    {
        ConsumeAny();
        return CurrentToken;
    }
    
    [Pure]
    private static TokenType GetType(char ch)
    {
        TokenType? type = ch switch
        {
            '{' => TokenType.LeftBrace,
            '}' => TokenType.RightBrace,
            '[' => TokenType.LeftBracket,
            ']' => TokenType.RightBracket,
            ':' => TokenType.Colon,
            '"' => TokenType.Quote,
            ',' => TokenType.Comma,
            _ => null
        };
        if (type is not null) return type.Value;
        return char.IsWhiteSpace(ch) ? TokenType.Space : TokenType.RegularChar;
    }
    
    public enum TokenType
    {
        End,
        
        Space,
        LeftBrace, RightBrace, // { }
        LeftBracket, RightBracket, // [ ]
        Colon, // :
        Quote, // "
        Comma, // ,
        
        RegularChar
    }
}