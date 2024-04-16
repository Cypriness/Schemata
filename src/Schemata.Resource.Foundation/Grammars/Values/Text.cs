using System.Linq.Expressions;
using Parlot;
using Schemata.Resource.Foundation.Grammars.Terms;

namespace Schemata.Resource.Foundation.Grammars.Values;

public class Text : IValue
{
    public Text(TextPosition position, string value) {
        Position = position;
        Value    = value;
    }

    public string Value { get; }

    #region IValue Members

    object IValue.Value => Value;

    public TextPosition Position { get; }

    public bool IsConstant => true;

    public Expression? ToExpression(Container ctx) {
        if (ctx.TryGetParameter(Value, out var value)) {
            return value;
        }

        return Expression.Constant(Value);
    }

    #endregion

    public override string ToString() {
        return $"\"{Value}\"";
    }
}