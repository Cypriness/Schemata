using System.Linq.Expressions;
using Parlot;
using Schemata.Resource.Foundation.Filters.Terms;

namespace Schemata.Resource.Foundation.Filters.Values;

public class Null : IValue
{
    public Null(TextPosition position) {
        Position = position;
    }

    #region IValue Members

    public TextPosition Position { get; }

    public bool IsConstant => true;

    public Expression? ToExpression(Container ctx) {
        return Expression.Constant(null);
    }

    #endregion

    public override string ToString() {
        return "\u2205";
    }
}
