namespace ShortLink.Domain.ValueObject;

public class ShortCode
{
    public string Value { get; private set; }

    private ShortCode(string value)
    {
        Value = value;
    }

    public static ShortCode Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value));
        
        if (value.Length is < 4 or > 10)
            throw new ArgumentOutOfRangeException(nameof(value));

        return new ShortCode(value);
    }
    
    public override string ToString() => Value;
}